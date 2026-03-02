using Application.Abstractions.Cqrs;
using Application.Contracts;
using Application.Measurements;
using Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Infrastructure.Messaging;

public sealed class RabbitMqMeasurementConsumer : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RabbitMqMeasurementConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqMeasurementConsumer(
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<RabbitMqMeasurementConsumer> logger)
    {
        _options = options.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                EnsureConnected();
                await StartConsumerLoopAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ consumer encountered an error and will reconnect");
                DisposeConnection();
                await Task.Delay(TimeSpan.FromSeconds(_options.ReconnectDelaySeconds), stoppingToken);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ consumer");
        DisposeConnection();
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        DisposeConnection();
        base.Dispose();
    }

    private void EnsureConnected()
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
        {
            return;
        }

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = false
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.BasicQos(0, _options.PrefetchCount, false);

        _channel.ExchangeDeclare(_options.DeadLetterExchange, ExchangeType.Fanout, durable: true);
        _channel.QueueDeclare(_options.DeadLetterQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_options.DeadLetterQueue, _options.DeadLetterExchange, string.Empty);

        _channel.ExchangeDeclare(
            exchange: _options.ExchangeName,
            type: ExchangeType.Direct,
            durable: _options.Durable,
            autoDelete: false);

        var arguments = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = _options.DeadLetterExchange
        };

        _channel.QueueDeclare(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments);

        _channel.QueueBind(
            queue: _options.QueueName,
            exchange: _options.ExchangeName,
            routingKey: _options.RoutingKey);

        _logger.LogInformation(
            "Connected to RabbitMQ at {Host}:{Port}. Queue {QueueName} bound to exchange {ExchangeName} with routing key {RoutingKey}.",
            _options.HostName,
            _options.Port,
            _options.QueueName,
            _options.ExchangeName,
            _options.RoutingKey);
    }

    private async Task StartConsumerLoopAsync(CancellationToken stoppingToken)
    {
        if (_channel is null)
        {
            throw new InvalidOperationException("RabbitMQ channel is not initialized.");
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, args) =>
        {
            await HandleMessageAsync(args, stoppingToken);
        };

        _channel.BasicConsume(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("RabbitMQ consumer started for queue {QueueName}", _options.QueueName);

        while (!stoppingToken.IsCancellationRequested && _channel.IsOpen && _connection is { IsOpen: true })
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        if (_channel is null)
        {
            return;
        }

        var payload = Encoding.UTF8.GetString(args.Body.ToArray());
        try
        {
            var dto = MeasurementMessageParser.Parse(payload);

            using var scope = _serviceScopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<IngestMeasurementCommand>>();

            await handler.HandleAsync(new IngestMeasurementCommand(dto), cancellationToken);

            _channel.BasicAck(args.DeliveryTag, multiple: false);

            _logger.LogInformation(
                "Message processed and acknowledged. DeliveryTag={DeliveryTag}, AquariumId={AquariumId}, Timestamp={Timestamp}",
                args.DeliveryTag,
                dto.AquariumId,
                dto.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed processing message. DeliveryTag={DeliveryTag}, Payload={Payload}",
                args.DeliveryTag,
                payload);

            _channel.BasicNack(args.DeliveryTag, multiple: false, requeue: false);
        }
    }

    private void DisposeConnection()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while disposing RabbitMQ resources");
        }
        finally
        {
            _channel = null;
            _connection = null;
        }
    }
}

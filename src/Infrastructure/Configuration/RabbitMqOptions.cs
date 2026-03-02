namespace Infrastructure.Configuration;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string UserName
    {
        get => Username;
        set => Username = value;
    }
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "aquarium.telemetry";
    public string RoutingKey { get; set; } = "aquarium.measurements";
    public bool Durable { get; set; } = true;
    public string QueueName { get; set; } = "measurements.ingest";
    public string DeadLetterExchange { get; set; } = "measurements.dlx";
    public string DeadLetterQueue { get; set; } = "measurements.dead";
    public ushort PrefetchCount { get; set; } = 50;
    public int ReconnectDelaySeconds { get; set; } = 5;
}

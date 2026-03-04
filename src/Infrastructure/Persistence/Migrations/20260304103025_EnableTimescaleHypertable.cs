using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnableTimescaleHypertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    CREATE EXTENSION IF NOT EXISTS timescaledb;
                EXCEPTION
                    WHEN SQLSTATE '42710' THEN
                        -- TimescaleDB is already loaded in this backend with another version.
                        -- In that case we continue and rely on the already loaded extension.
                        NULL;
                END
                $$;
                """);

            migrationBuilder.Sql(
                """
                SELECT create_hypertable(
                    'measurements',
                    'timestamp',
                    'aquarium_id',
                    4,
                    if_not_exists => TRUE);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: converting a hypertable back to a regular table is intentionally not automated.
        }
    }
}

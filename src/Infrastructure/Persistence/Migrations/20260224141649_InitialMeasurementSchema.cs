using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMeasurementSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "measurements",
                columns: table => new
                {
                    aquarium_id = table.Column<string>(type: "text", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    temperature = table.Column<decimal>(type: "numeric", nullable: true),
                    mg = table.Column<decimal>(type: "numeric", nullable: true),
                    kh = table.Column<decimal>(type: "numeric", nullable: true),
                    ca = table.Column<decimal>(type: "numeric", nullable: true),
                    ph = table.Column<decimal>(type: "numeric", nullable: true),
                    oxygen = table.Column<decimal>(type: "numeric", nullable: true),
                    pump = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_measurements", x => new { x.aquarium_id, x.timestamp });
                });

            migrationBuilder.CreateIndex(
                name: "ix_measurements_aquarium_timestamp_desc",
                table: "measurements",
                columns: new[] { "aquarium_id", "timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_measurements_timestamp",
                table: "measurements",
                column: "timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "measurements");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelPms.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddPricingSnapshots : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "base_nightly_rate_amount",
            table: "room_types",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<string>(
            name: "base_nightly_rate_currency",
            table: "room_types",
            type: "character varying(3)",
            maxLength: 3,
            nullable: false,
            defaultValue: "KRW");

        migrationBuilder.AddColumn<decimal>(
            name: "total_amount",
            table: "reservations",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<string>(
            name: "total_currency",
            table: "reservations",
            type: "character varying(3)",
            maxLength: 3,
            nullable: false,
            defaultValue: "KRW");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "base_nightly_rate_amount",
            table: "room_types");

        migrationBuilder.DropColumn(
            name: "base_nightly_rate_currency",
            table: "room_types");

        migrationBuilder.DropColumn(
            name: "total_amount",
            table: "reservations");

        migrationBuilder.DropColumn(
            name: "total_currency",
            table: "reservations");
    }
}

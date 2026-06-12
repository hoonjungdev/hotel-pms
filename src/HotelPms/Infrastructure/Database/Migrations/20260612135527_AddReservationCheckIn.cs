using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelPms.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddReservationCheckIn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "assigned_room_id",
            table: "reservations",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddUniqueConstraint(
            name: "AK_rooms_tenant_id_id",
            table: "rooms",
            columns: new[] { "tenant_id", "id" });

        migrationBuilder.CreateIndex(
            name: "ix_reservations_tenant_id_assigned_room_id",
            table: "reservations",
            columns: new[] { "tenant_id", "assigned_room_id" });

        migrationBuilder.AddForeignKey(
            name: "FK_reservations_rooms_tenant_id_assigned_room_id",
            table: "reservations",
            columns: new[] { "tenant_id", "assigned_room_id" },
            principalTable: "rooms",
            principalColumns: new[] { "tenant_id", "id" },
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_reservations_rooms_tenant_id_assigned_room_id",
            table: "reservations");

        migrationBuilder.DropUniqueConstraint(
            name: "AK_rooms_tenant_id_id",
            table: "rooms");

        migrationBuilder.DropIndex(
            name: "ix_reservations_tenant_id_assigned_room_id",
            table: "reservations");

        migrationBuilder.DropColumn(
            name: "assigned_room_id",
            table: "reservations");
    }
}

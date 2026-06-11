using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelPms.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddReservations : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddUniqueConstraint(
            name: "AK_guests_tenant_id_id",
            table: "guests",
            columns: new[] { "tenant_id", "id" });

        migrationBuilder.CreateTable(
            name: "reservations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                primary_guest_id = table.Column<Guid>(type: "uuid", nullable: false),
                room_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                guest_count = table.Column<int>(type: "integer", nullable: false),
                status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                check_out_date = table.Column<DateOnly>(type: "date", nullable: false),
                check_in_date = table.Column<DateOnly>(type: "date", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_reservations", x => x.id);
                table.ForeignKey(
                    name: "FK_reservations_guests_tenant_id_primary_guest_id",
                    columns: x => new { x.tenant_id, x.primary_guest_id },
                    principalTable: "guests",
                    principalColumns: new[] { "tenant_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_reservations_room_types_tenant_id_room_type_id",
                    columns: x => new { x.tenant_id, x.room_type_id },
                    principalTable: "room_types",
                    principalColumns: new[] { "tenant_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_reservations_tenant_id",
            table: "reservations",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "IX_reservations_tenant_id_primary_guest_id",
            table: "reservations",
            columns: new[] { "tenant_id", "primary_guest_id" });

        migrationBuilder.CreateIndex(
            name: "ix_reservations_tenant_id_room_type_id",
            table: "reservations",
            columns: new[] { "tenant_id", "room_type_id" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "reservations");

        migrationBuilder.DropUniqueConstraint(
            name: "AK_guests_tenant_id_id",
            table: "guests");
    }
}

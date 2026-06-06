using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelPms.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddRooms : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "rooms",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                condition = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_rooms", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_rooms_tenant_id_number",
            table: "rooms",
            columns: new[] { "tenant_id", "number" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "rooms");
    }
}

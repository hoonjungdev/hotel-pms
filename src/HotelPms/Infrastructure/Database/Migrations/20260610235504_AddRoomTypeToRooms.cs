using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelPms.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddRoomTypeToRooms : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "room_type_id",
            table: "rooms",
            type: "uuid",
            nullable: true);

        migrationBuilder.Sql(
            """
            INSERT INTO room_types (id, tenant_id, code, name, base_occupancy, max_occupancy)
            SELECT (md5(rooms.tenant_id::text || ':legacy-room-type'))::uuid,
                   rooms.tenant_id,
                   'LEGACY',
                   'Legacy Room Type',
                   1,
                   1
            FROM rooms
            WHERE NOT EXISTS (
                SELECT 1
                FROM room_types
                WHERE room_types.tenant_id = rooms.tenant_id
            )
            GROUP BY rooms.tenant_id
            ON CONFLICT (tenant_id, code) DO NOTHING;

            UPDATE rooms
            SET room_type_id = (
                SELECT room_types.id
                FROM room_types
                WHERE room_types.tenant_id = rooms.tenant_id
                ORDER BY room_types.code
                LIMIT 1
            )
            WHERE rooms.room_type_id IS NULL;
            """);

        migrationBuilder.AlterColumn<Guid>(
            name: "room_type_id",
            table: "rooms",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);

        migrationBuilder.AddUniqueConstraint(
            name: "AK_room_types_tenant_id_id",
            table: "room_types",
            columns: ["tenant_id", "id"]);

        migrationBuilder.CreateIndex(
            name: "ix_rooms_tenant_id_room_type_id",
            table: "rooms",
            columns: ["tenant_id", "room_type_id"]);

        migrationBuilder.AddForeignKey(
            name: "FK_rooms_room_types_tenant_id_room_type_id",
            table: "rooms",
            columns: ["tenant_id", "room_type_id"],
            principalTable: "room_types",
            principalColumns: ["tenant_id", "id"],
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_rooms_room_types_tenant_id_room_type_id",
            table: "rooms");

        migrationBuilder.DropIndex(
            name: "ix_rooms_tenant_id_room_type_id",
            table: "rooms");

        migrationBuilder.DropUniqueConstraint(
            name: "AK_room_types_tenant_id_id",
            table: "room_types");

        migrationBuilder.DropColumn(
            name: "room_type_id",
            table: "rooms");
    }
}

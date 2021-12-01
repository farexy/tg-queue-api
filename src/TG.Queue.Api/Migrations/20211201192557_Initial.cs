using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TG.Queue.Api.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "battles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    open = table.Column<bool>(type: "boolean", nullable: false),
                    battle_type = table.Column<string>(type: "text", nullable: false),
                    server_port = table.Column<int>(type: "integer", nullable: true),
                    server_ip = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expected_start_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_battles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "battle_users",
                columns: table => new
                {
                    battle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_battle_users", x => new { x.battle_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_battle_users_battles_battle_id",
                        column: x => x.battle_id,
                        principalTable: "battles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "battle_users");

            migrationBuilder.DropTable(
                name: "battles");
        }
    }
}

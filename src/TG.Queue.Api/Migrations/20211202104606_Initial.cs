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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "battles");
        }
    }
}

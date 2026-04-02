using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PushAndPull.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "room");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "user",
                schema: "auth",
                columns: table => new
                {
                    steam_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    nickname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.steam_id);
                });

            migrationBuilder.CreateTable(
                name: "room",
                schema: "room",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    room_name = table.Column<string>(type: "text", nullable: false),
                    room_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    steam_lobby_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    host_steam_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    current_players = table.Column<int>(type: "integer", nullable: false),
                    max_players = table.Column<int>(type: "integer", nullable: false),
                    is_private = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room", x => x.id);
                    table.ForeignKey(
                        name: "FK_room_user_host_steam_id",
                        column: x => x.host_steam_id,
                        principalSchema: "auth",
                        principalTable: "user",
                        principalColumn: "steam_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_room_expires_at",
                schema: "room",
                table: "room",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_room_host_steam_id",
                schema: "room",
                table: "room",
                column: "host_steam_id");

            migrationBuilder.CreateIndex(
                name: "idx_room_room_code",
                schema: "room",
                table: "room",
                column: "room_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_room_status",
                schema: "room",
                table: "room",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_room_status_created_at",
                schema: "room",
                table: "room",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_room_status_private",
                schema: "room",
                table: "room",
                columns: new[] { "status", "is_private" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "room",
                schema: "room");

            migrationBuilder.DropTable(
                name: "user",
                schema: "auth");
        }
    }
}

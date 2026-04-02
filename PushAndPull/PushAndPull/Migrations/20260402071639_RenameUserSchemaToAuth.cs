using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PushAndPull.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserSchemaToAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_room_user_host_steam_id",
                schema: "room",
                table: "room");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.RenameTable(
                name: "user",
                schema: "game_user",
                newName: "user",
                newSchema: "auth");

            migrationBuilder.AddForeignKey(
                name: "FK_room_user_host_steam_id",
                schema: "room",
                table: "room",
                column: "host_steam_id",
                principalSchema: "auth",
                principalTable: "user",
                principalColumn: "steam_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_room_user_host_steam_id",
                schema: "room",
                table: "room");

            migrationBuilder.EnsureSchema(
                name: "game_user");

            migrationBuilder.RenameTable(
                name: "user",
                schema: "auth",
                newName: "user",
                newSchema: "game_user");

            migrationBuilder.AddForeignKey(
                name: "FK_room_user_host_steam_id",
                schema: "room",
                table: "room",
                column: "host_steam_id",
                principalSchema: "game_user",
                principalTable: "user",
                principalColumn: "steam_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

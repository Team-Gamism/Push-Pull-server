using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PushAndPull.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomGuestHeartbeatAndLengthLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "nickname",
                schema: "auth",
                table: "user",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "room_name",
                schema: "room",
                table: "room",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<decimal>(
                name: "guest_steam_id",
                schema: "room",
                table: "room",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_heartbeat_at",
                schema: "room",
                table: "room",
                type: "timestamptz",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "guest_steam_id",
                schema: "room",
                table: "room");

            migrationBuilder.DropColumn(
                name: "last_heartbeat_at",
                schema: "room",
                table: "room");

            migrationBuilder.AlterColumn<string>(
                name: "nickname",
                schema: "auth",
                table: "user",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "room_name",
                schema: "room",
                table: "room",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}

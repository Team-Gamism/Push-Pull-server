using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PushAndPull.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestLastHeartbeatAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "guest_last_heartbeat_at",
                schema: "room",
                table: "room",
                type: "timestamptz",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "guest_last_heartbeat_at",
                schema: "room",
                table: "room");
        }
    }
}

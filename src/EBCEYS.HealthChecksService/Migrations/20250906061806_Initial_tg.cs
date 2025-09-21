using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EBCEYS.HealthChecksService.Migrations
{
    /// <inheritdoc />
    public partial class Initial_tg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tg_bot_subscriptions",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    subscriber = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    chat_id = table.Column<long>(type: "INTEGER", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tg_bot_subscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tg_bot_subscriptions_chat_id",
                table: "tg_bot_subscriptions",
                column: "chat_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tg_bot_subscriptions_subscriber",
                table: "tg_bot_subscriptions",
                column: "subscriber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tg_bot_subscriptions");
        }
    }
}

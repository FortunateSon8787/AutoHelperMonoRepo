using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoHelper.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AUT18_AddInvalidChatRequestsAndAiQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AiRequestsRemaining",
                table: "customers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "invalid_chat_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserInput = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invalid_chat_requests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_invalid_chat_requests_ChatId",
                table: "invalid_chat_requests",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_invalid_chat_requests_CustomerId",
                table: "invalid_chat_requests",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invalid_chat_requests");

            migrationBuilder.DropColumn(
                name: "AiRequestsRemaining",
                table: "customers");
        }
    }
}

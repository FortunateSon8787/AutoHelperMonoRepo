using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoHelper.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AUT162_AddChatbotConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chatbot_config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MaxCharsPerField = table.Column<int>(type: "integer", nullable: false),
                    DailyLimitByPlan = table.Column<string>(type: "jsonb", nullable: false),
                    TopUpPriceUsd = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TopUpRequestCount = table.Column<int>(type: "integer", nullable: false),
                    DisablePartnerSuggestionsInMode1 = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chatbot_config", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chatbot_config");
        }
    }
}

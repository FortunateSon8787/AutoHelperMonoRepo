using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AutoHelper.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AUT22_AddSubscriptionPlanConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subscription_plan_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Plan = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PriceUsd = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    MonthlyQuota = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_plan_configs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "subscription_plan_configs",
                columns: new[] { "Id", "MonthlyQuota", "Plan", "PriceUsd" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), 10, "Normal", 4.99m },
                    { new Guid("11111111-0000-0000-0000-000000000002"), 20, "Pro", 7.99m },
                    { new Guid("11111111-0000-0000-0000-000000000003"), 40, "Max", 12.99m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_plan_configs_Plan",
                table: "subscription_plan_configs",
                column: "Plan",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subscription_plan_configs");
        }
    }
}

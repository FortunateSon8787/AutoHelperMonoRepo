using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoHelper.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ExecutorName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExecutorContacts = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Operations = table.Column<string>(type: "jsonb", nullable: false),
                    DocumentUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_records", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_service_records_VehicleId",
                table: "service_records",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_records");
        }
    }
}

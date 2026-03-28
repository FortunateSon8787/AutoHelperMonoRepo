using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoHelper.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPartners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "partners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Specialization = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    location_lat = table.Column<double>(type: "double precision", nullable: false),
                    location_lng = table.Column<double>(type: "double precision", nullable: false),
                    working_open_from = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    working_open_to = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    working_days = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    contacts_phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    contacts_website = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    contacts_messenger_links = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsPotentiallyUnfit = table.Column<bool>(type: "boolean", nullable: false),
                    ShowBannersToAnonymous = table.Column<bool>(type: "boolean", nullable: false),
                    AccountUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partners", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_partners_AccountUserId",
                table: "partners",
                column: "AccountUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "partners");
        }
    }
}

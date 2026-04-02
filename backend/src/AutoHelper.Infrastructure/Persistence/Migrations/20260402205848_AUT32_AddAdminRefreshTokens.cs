using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoHelper.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AUT32_AddAdminRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_admin_refresh_tokens_admin_users_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "admin_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_refresh_tokens_AdminUserId",
                table: "admin_refresh_tokens",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_admin_refresh_tokens_Token",
                table: "admin_refresh_tokens",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_refresh_tokens");
        }
    }
}

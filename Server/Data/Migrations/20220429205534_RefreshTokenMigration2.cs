using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PutAway.Server.Data.Migrations
{
    public partial class RefreshTokenMigration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRefreshToken_Users_UserId",
                table: "UserRefreshToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRefreshToken",
                table: "UserRefreshToken");

            migrationBuilder.RenameTable(
                name: "UserRefreshToken",
                newName: "UserRefreshTokens");

            migrationBuilder.RenameIndex(
                name: "IX_UserRefreshToken_UserId",
                table: "UserRefreshTokens",
                newName: "IX_UserRefreshTokens_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRefreshTokens",
                table: "UserRefreshTokens",
                column: "UserRefreshTokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRefreshTokens_Users_UserId",
                table: "UserRefreshTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRefreshTokens_Users_UserId",
                table: "UserRefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRefreshTokens",
                table: "UserRefreshTokens");

            migrationBuilder.RenameTable(
                name: "UserRefreshTokens",
                newName: "UserRefreshToken");

            migrationBuilder.RenameIndex(
                name: "IX_UserRefreshTokens_UserId",
                table: "UserRefreshToken",
                newName: "IX_UserRefreshToken_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRefreshToken",
                table: "UserRefreshToken",
                column: "UserRefreshTokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRefreshToken_Users_UserId",
                table: "UserRefreshToken",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

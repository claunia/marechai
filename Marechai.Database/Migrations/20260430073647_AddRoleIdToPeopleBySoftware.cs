using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleIdToPeopleBySoftware : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoleId",
                table: "PeopleBySoftware",
                type: "char(3)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleBySoftware_RoleId",
                table: "PeopleBySoftware",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_PeopleBySoftware_DocumentRoles_RoleId",
                table: "PeopleBySoftware",
                column: "RoleId",
                principalTable: "DocumentRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeopleBySoftware_DocumentRoles_RoleId",
                table: "PeopleBySoftware");

            migrationBuilder.DropIndex(
                name: "IX_PeopleBySoftware_RoleId",
                table: "PeopleBySoftware");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "PeopleBySoftware");
        }
    }
}

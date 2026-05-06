using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwareDlcKindAndBaseSoftware : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "BaseSoftwareId",
                table: "Softwares",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Softwares_BaseSoftwareId",
                table: "Softwares",
                column: "BaseSoftwareId");

            migrationBuilder.AddForeignKey(
                name: "FK_Softwares_Softwares_BaseSoftwareId",
                table: "Softwares",
                column: "BaseSoftwareId",
                principalTable: "Softwares",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Softwares_Softwares_BaseSoftwareId",
                table: "Softwares");

            migrationBuilder.DropIndex(
                name: "IX_Softwares_BaseSoftwareId",
                table: "Softwares");

            migrationBuilder.DropColumn(
                name: "BaseSoftwareId",
                table: "Softwares");
        }
    }
}

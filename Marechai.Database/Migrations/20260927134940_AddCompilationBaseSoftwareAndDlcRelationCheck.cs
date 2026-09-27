using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCompilationBaseSoftwareAndDlcRelationCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "BaseSoftwareId",
                table: "SoftwareCompilations",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DlcRelationCheckedAt",
                table: "MobyGamesImportStates",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareCompilations_BaseSoftwareId",
                table: "SoftwareCompilations",
                column: "BaseSoftwareId");

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareCompilations_Softwares_BaseSoftwareId",
                table: "SoftwareCompilations",
                column: "BaseSoftwareId",
                principalTable: "Softwares",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareCompilations_Softwares_BaseSoftwareId",
                table: "SoftwareCompilations");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareCompilations_BaseSoftwareId",
                table: "SoftwareCompilations");

            migrationBuilder.DropColumn(
                name: "BaseSoftwareId",
                table: "SoftwareCompilations");

            migrationBuilder.DropColumn(
                name: "DlcRelationCheckedAt",
                table: "MobyGamesImportStates");
        }
    }
}

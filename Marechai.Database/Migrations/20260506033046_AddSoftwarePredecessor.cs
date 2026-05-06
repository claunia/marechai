using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwarePredecessor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "PredecessorId",
                table: "Softwares",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Softwares_PredecessorId",
                table: "Softwares",
                column: "PredecessorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Softwares_Softwares_PredecessorId",
                table: "Softwares",
                column: "PredecessorId",
                principalTable: "Softwares",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Softwares_Softwares_PredecessorId",
                table: "Softwares");

            migrationBuilder.DropIndex(
                name: "IX_Softwares_PredecessorId",
                table: "Softwares");

            migrationBuilder.DropColumn(
                name: "PredecessorId",
                table: "Softwares");
        }
    }
}

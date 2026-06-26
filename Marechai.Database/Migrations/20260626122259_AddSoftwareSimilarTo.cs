using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwareSimilarTo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SoftwareSimilarTo",
                columns: table => new
                {
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    SimilarSoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareSimilarTo", x => new { x.SoftwareId, x.SimilarSoftwareId });
                    table.ForeignKey(
                        name: "fk_software_similar_to_similar_software",
                        column: x => x.SimilarSoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_software_similar_to_software",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_software_similar_to_similar_software",
                table: "SoftwareSimilarTo",
                column: "SimilarSoftwareId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SoftwareSimilarTo");
        }
    }
}

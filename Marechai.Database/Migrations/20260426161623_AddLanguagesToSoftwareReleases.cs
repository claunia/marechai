using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguagesToSoftwareReleases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LanguageBySoftwareRelease",
                columns: table => new
                {
                    SoftwareReleaseId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageBySoftwareRelease", x => new { x.SoftwareReleaseId, x.LanguageCode });
                    table.ForeignKey(
                        name: "FK_LanguageBySoftwareRelease_ISO_639-3_LanguageCode",
                        column: x => x.LanguageCode,
                        principalTable: "ISO_639-3",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LanguageBySoftwareRelease_SoftwareReleases_SoftwareReleaseId",
                        column: x => x.SoftwareReleaseId,
                        principalTable: "SoftwareReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageBySoftwareRelease_LanguageCode",
                table: "LanguageBySoftwareRelease",
                column: "LanguageCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LanguageBySoftwareRelease");
        }
    }
}

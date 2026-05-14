using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwareScreenshotCaptionTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SoftwareScreenshotCaptionTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ScreenshotId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LanguageCode = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Caption = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareScreenshotCaptionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "fk_software_screenshot_caption_translations_language",
                        column: x => x.LanguageCode,
                        principalTable: "ISO_639-3",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_software_screenshot_caption_translations_screenshot",
                        column: x => x.ScreenshotId,
                        principalTable: "SoftwareScreenshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_software_screenshot_caption_translations_screenshot_language",
                table: "SoftwareScreenshotCaptionTranslations",
                columns: new[] { "ScreenshotId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareScreenshotCaptionTranslations_LanguageCode",
                table: "SoftwareScreenshotCaptionTranslations",
                column: "LanguageCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SoftwareScreenshotCaptionTranslations");
        }
    }
}

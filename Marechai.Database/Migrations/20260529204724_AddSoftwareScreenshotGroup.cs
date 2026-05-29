using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwareScreenshotGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "SoftwareScreenshots",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SoftwareScreenshotGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareScreenshotGroups", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareScreenshotGroupTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareScreenshotGroupTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "fk_software_screenshot_group_translations_group",
                        column: x => x.GroupId,
                        principalTable: "SoftwareScreenshotGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_software_screenshot_group_translations_language",
                        column: x => x.LanguageCode,
                        principalTable: "ISO_639-3",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareScreenshots_GroupId",
                table: "SoftwareScreenshots",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareScreenshotGroups_Name",
                table: "SoftwareScreenshotGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_software_screenshot_group_translations_group_language",
                table: "SoftwareScreenshotGroupTranslations",
                columns: new[] { "GroupId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareScreenshotGroupTranslations_LanguageCode",
                table: "SoftwareScreenshotGroupTranslations",
                column: "LanguageCode");

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareScreenshots_SoftwareScreenshotGroups_GroupId",
                table: "SoftwareScreenshots",
                column: "GroupId",
                principalTable: "SoftwareScreenshotGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareScreenshots_SoftwareScreenshotGroups_GroupId",
                table: "SoftwareScreenshots");

            migrationBuilder.DropTable(
                name: "SoftwareScreenshotGroupTranslations");

            migrationBuilder.DropTable(
                name: "SoftwareScreenshotGroups");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareScreenshots_GroupId",
                table: "SoftwareScreenshots");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "SoftwareScreenshots");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwareGenreTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SoftwareGenreTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GenreId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareGenreTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "fk_software_genre_translations_genre",
                        column: x => x.GenreId,
                        principalTable: "SoftwareGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_software_genre_translations_language",
                        column: x => x.LanguageCode,
                        principalTable: "ISO_639-3",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_software_genre_translations_genre_language",
                table: "SoftwareGenreTranslations",
                columns: new[] { "GenreId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareGenreTranslations_LanguageCode",
                table: "SoftwareGenreTranslations",
                column: "LanguageCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SoftwareGenreTranslations");
        }
    }
}

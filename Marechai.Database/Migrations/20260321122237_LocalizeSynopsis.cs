using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class LocalizeSynopsis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create the new synopsis tables FIRST (before dropping old columns)
            migrationBuilder.CreateTable(
                name: "BookSynopses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BookId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    LanguageCode = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Text = table.Column<string>(type: "longtext", maxLength: 262144, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookSynopses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookSynopses_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DocumentSynopses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    LanguageCode = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Text = table.Column<string>(type: "longtext", maxLength: 262144, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSynopses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentSynopses_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MagazineSynopses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MagazineId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    LanguageCode = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Text = table.Column<string>(type: "longtext", maxLength: 262144, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MagazineSynopses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MagazineSynopses_Magazines_MagazineId",
                        column: x => x.MagazineId,
                        principalTable: "Magazines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Step 2: Fix LanguageCode collation to match ISO_639-3.Id (utf8mb4_general_ci)
            migrationBuilder.Sql(
                "ALTER TABLE `BookSynopses` MODIFY `LanguageCode` char(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL");

            migrationBuilder.Sql(
                "ALTER TABLE `DocumentSynopses` MODIFY `LanguageCode` char(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL");

            migrationBuilder.Sql(
                "ALTER TABLE `MagazineSynopses` MODIFY `LanguageCode` char(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL");

            // Step 3: Add FK constraints to ISO_639-3 (after collation fix)
            migrationBuilder.AddForeignKey(
                name: "fk_book_synopses_language",
                table: "BookSynopses",
                column: "LanguageCode",
                principalTable: "ISO_639-3",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_document_synopses_language",
                table: "DocumentSynopses",
                column: "LanguageCode",
                principalTable: "ISO_639-3",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_magazine_synopses_language",
                table: "MagazineSynopses",
                column: "LanguageCode",
                principalTable: "ISO_639-3",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Step 4: Migrate existing synopsis data to new tables
            migrationBuilder.Sql(
                @"INSERT INTO `BookSynopses` (`BookId`, `LanguageCode`, `Text`, `CreatedOn`, `UpdatedOn`)
                  SELECT `Id`, 'eng', `Synopsis`, `CreatedOn`, `UpdatedOn`
                  FROM `Books`
                  WHERE `Synopsis` IS NOT NULL AND `Synopsis` != ''");

            migrationBuilder.Sql(
                @"INSERT INTO `DocumentSynopses` (`DocumentId`, `LanguageCode`, `Text`, `CreatedOn`, `UpdatedOn`)
                  SELECT `Id`, 'eng', `Synopsis`, `CreatedOn`, `UpdatedOn`
                  FROM `Documents`
                  WHERE `Synopsis` IS NOT NULL AND `Synopsis` != ''");

            migrationBuilder.Sql(
                @"INSERT INTO `MagazineSynopses` (`MagazineId`, `LanguageCode`, `Text`, `CreatedOn`, `UpdatedOn`)
                  SELECT `Id`, 'eng', `Synopsis`, `CreatedOn`, `UpdatedOn`
                  FROM `Magazines`
                  WHERE `Synopsis` IS NOT NULL AND `Synopsis` != ''");

            // Step 5: Drop old Synopsis columns and full-text indexes
            migrationBuilder.DropIndex(
                name: "IX_Magazines_Synopsis",
                table: "Magazines");

            migrationBuilder.DropIndex(
                name: "IX_Documents_Synopsis",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Books_Synopsis",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Synopsis",
                table: "Magazines");

            migrationBuilder.DropColumn(
                name: "Synopsis",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Synopsis",
                table: "Books");

            // Step 6: Create indexes on new tables
            migrationBuilder.CreateIndex(
                name: "idx_book_synopses_book_language",
                table: "BookSynopses",
                columns: new[] { "BookId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookSynopses_LanguageCode",
                table: "BookSynopses",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_BookSynopses_Text",
                table: "BookSynopses",
                column: "Text")
                .Annotation("MySql:FullTextIndex", true);

            migrationBuilder.CreateIndex(
                name: "idx_document_synopses_document_language",
                table: "DocumentSynopses",
                columns: new[] { "DocumentId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSynopses_LanguageCode",
                table: "DocumentSynopses",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSynopses_Text",
                table: "DocumentSynopses",
                column: "Text")
                .Annotation("MySql:FullTextIndex", true);

            migrationBuilder.CreateIndex(
                name: "idx_magazine_synopses_magazine_language",
                table: "MagazineSynopses",
                columns: new[] { "MagazineId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MagazineSynopses_LanguageCode",
                table: "MagazineSynopses",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineSynopses_Text",
                table: "MagazineSynopses",
                column: "Text")
                .Annotation("MySql:FullTextIndex", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookSynopses");

            migrationBuilder.DropTable(
                name: "DocumentSynopses");

            migrationBuilder.DropTable(
                name: "MagazineSynopses");

            migrationBuilder.AddColumn<string>(
                name: "Synopsis",
                table: "Magazines",
                type: "longtext",
                maxLength: 262144,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Synopsis",
                table: "Documents",
                type: "longtext",
                maxLength: 262144,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Synopsis",
                table: "Books",
                type: "longtext",
                maxLength: 262144,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Magazines_Synopsis",
                table: "Magazines",
                column: "Synopsis")
                .Annotation("MySql:FullTextIndex", true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Synopsis",
                table: "Documents",
                column: "Synopsis")
                .Annotation("MySql:FullTextIndex", true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_Synopsis",
                table: "Books",
                column: "Synopsis")
                .Annotation("MySql:FullTextIndex", true);
        }
    }
}

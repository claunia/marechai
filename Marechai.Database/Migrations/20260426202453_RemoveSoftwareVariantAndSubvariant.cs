using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSoftwareVariantAndSubvariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop idempotently — objects may not exist on all environments
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS=0;");
            migrationBuilder.Sql("ALTER TABLE `SoftwareReleases` DROP FOREIGN KEY IF EXISTS `FK_SoftwareReleases_SoftwareSubvariants_SubvariantId`;");
            migrationBuilder.Sql("ALTER TABLE `SoftwareReleases` DROP FOREIGN KEY IF EXISTS `FK_SoftwareReleases_SoftwareVariants_VariantId`;");
            migrationBuilder.Sql("ALTER TABLE `StandaloneFiles` DROP FOREIGN KEY IF EXISTS `FK_StandaloneFiles_SoftwareVariants_SoftwareVariantId`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `CompaniesBySoftwareVariants`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `SoftwareSubvariantLanguages`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `SoftwareVariantLanguages`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `SoftwareSubvariants`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `SoftwareVariants`;");
            migrationBuilder.Sql("ALTER TABLE `SoftwareReleases` DROP INDEX IF EXISTS `IX_SoftwareReleases_SubvariantId`;");
            migrationBuilder.Sql("ALTER TABLE `SoftwareReleases` DROP INDEX IF EXISTS `IX_SoftwareReleases_VariantId`;");
            migrationBuilder.Sql("ALTER TABLE `SoftwareReleases` DROP COLUMN IF EXISTS `SubvariantId`;");
            migrationBuilder.Sql("ALTER TABLE `SoftwareReleases` DROP COLUMN IF EXISTS `VariantId`;");
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS=1;");

            // Rename column/index — use raw SQL to handle already-renamed case
            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StandaloneFiles' AND COLUMN_NAME = 'SoftwareVariantId');
                SET @sql = IF(@col_exists > 0,
                    'ALTER TABLE `StandaloneFiles` CHANGE COLUMN `SoftwareVariantId` `SoftwareReleaseId` bigint(20) unsigned NOT NULL',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @idx_exists = (SELECT COUNT(*) FROM information_schema.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StandaloneFiles' AND INDEX_NAME = 'IX_StandaloneFiles_SoftwareVariantId');
                SET @sql = IF(@idx_exists > 0,
                    'ALTER TABLE `StandaloneFiles` RENAME INDEX `IX_StandaloneFiles_SoftwareVariantId` TO `IX_StandaloneFiles_SoftwareReleaseId`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Add FK if not exists
            migrationBuilder.Sql(@"
                SET @fk_exists = (SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StandaloneFiles'
                    AND CONSTRAINT_NAME = 'FK_StandaloneFiles_SoftwareReleases_SoftwareReleaseId');
                SET @sql = IF(@fk_exists = 0,
                    'ALTER TABLE `StandaloneFiles` ADD CONSTRAINT `FK_StandaloneFiles_SoftwareReleases_SoftwareReleaseId` FOREIGN KEY (`SoftwareReleaseId`) REFERENCES `SoftwareReleases` (`Id`) ON DELETE CASCADE',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StandaloneFiles_SoftwareReleases_SoftwareReleaseId",
                table: "StandaloneFiles");

            migrationBuilder.RenameColumn(
                name: "SoftwareReleaseId",
                table: "StandaloneFiles",
                newName: "SoftwareVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_StandaloneFiles_SoftwareReleaseId",
                table: "StandaloneFiles",
                newName: "IX_StandaloneFiles_SoftwareVariantId");

            migrationBuilder.AddColumn<ulong>(
                name: "SubvariantId",
                table: "SoftwareReleases",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "VariantId",
                table: "SoftwareReleases",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SoftwareVariants",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareVersionId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareVariants_SoftwareVersions_SoftwareVersionId",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SoftwareVariants_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompaniesBySoftwareVariants",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int(11)", nullable: false),
                    RoleId = table.Column<string>(type: "char(3)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompaniesBySoftwareVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVariants_SoftwareRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "SoftwareRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVariants_SoftwareVariants_SoftwareVariant~",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVariants_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareSubvariants",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareSubvariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareSubvariants_SoftwareVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareVariantLanguages",
                columns: table => new
                {
                    VariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LanguageId = table.Column<string>(type: "char(3)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareVariantLanguages", x => new { x.VariantId, x.LanguageCode });
                    table.ForeignKey(
                        name: "FK_SoftwareVariantLanguages_ISO_639-3_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "ISO_639-3",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SoftwareVariantLanguages_SoftwareVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareSubvariantLanguages",
                columns: table => new
                {
                    SubvariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LanguageId = table.Column<string>(type: "char(3)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareSubvariantLanguages", x => new { x.SubvariantId, x.LanguageCode });
                    table.ForeignKey(
                        name: "FK_SoftwareSubvariantLanguages_ISO_639-3_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "ISO_639-3",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SoftwareSubvariantLanguages_SoftwareSubvariants_SubvariantId",
                        column: x => x.SubvariantId,
                        principalTable: "SoftwareSubvariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareReleases_SubvariantId",
                table: "SoftwareReleases",
                column: "SubvariantId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareReleases_VariantId",
                table: "SoftwareReleases",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVariants_CompanyId",
                table: "CompaniesBySoftwareVariants",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVariants_RoleId",
                table: "CompaniesBySoftwareVariants",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVariants_SoftwareVariantId",
                table: "CompaniesBySoftwareVariants",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareSubvariantLanguages_LanguageId",
                table: "SoftwareSubvariantLanguages",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareSubvariants_VariantId_Name",
                table: "SoftwareSubvariants",
                columns: new[] { "VariantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariantLanguages_LanguageId",
                table: "SoftwareVariantLanguages",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_SoftwareId_Name",
                table: "SoftwareVariants",
                columns: new[] { "SoftwareId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_SoftwareVersionId",
                table: "SoftwareVariants",
                column: "SoftwareVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareReleases_SoftwareSubvariants_SubvariantId",
                table: "SoftwareReleases",
                column: "SubvariantId",
                principalTable: "SoftwareSubvariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareReleases_SoftwareVariants_VariantId",
                table: "SoftwareReleases",
                column: "VariantId",
                principalTable: "SoftwareVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StandaloneFiles_SoftwareVariants_SoftwareVariantId",
                table: "StandaloneFiles",
                column: "SoftwareVariantId",
                principalTable: "SoftwareVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

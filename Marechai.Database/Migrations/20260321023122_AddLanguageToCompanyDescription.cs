using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageToCompanyDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add column only if it doesn't already exist (handles partial re-apply)
            migrationBuilder.Sql("""
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'CompanyDescriptions' AND COLUMN_NAME = 'LanguageCode');
                SET @sql = IF(@col_exists = 0,
                    'ALTER TABLE `CompanyDescriptions` ADD `LanguageCode` char(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT ''''',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            // Ensure column has correct collation (fixes partial applies with wrong default collation)
            migrationBuilder.Sql("ALTER TABLE `CompanyDescriptions` MODIFY `LanguageCode` char(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT ''");

            // Set all existing descriptions to English
            migrationBuilder.Sql("UPDATE `CompanyDescriptions` SET `LanguageCode` = 'eng' WHERE `LanguageCode` = '' OR `LanguageCode` IS NULL");

            // Create composite unique index only if it doesn't already exist
            migrationBuilder.Sql("""
                SET @idx_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'CompanyDescriptions' AND INDEX_NAME = 'idx_company_descriptions_company_language');
                SET @sql = IF(@idx_exists = 0,
                    'CREATE UNIQUE INDEX `idx_company_descriptions_company_language` ON `CompanyDescriptions` (`CompanyId`, `LanguageCode`)',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            // Drop old single-column index AFTER composite index is created
            // (MySQL needs an index starting with CompanyId for the FK)
            migrationBuilder.Sql("""
                SET @idx_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'CompanyDescriptions' AND INDEX_NAME = 'IX_CompanyDescriptions_CompanyId');
                SET @sql = IF(@idx_exists > 0,
                    'ALTER TABLE `CompanyDescriptions` DROP INDEX `IX_CompanyDescriptions_CompanyId`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            // Create language code index only if it doesn't already exist
            migrationBuilder.Sql("""
                SET @idx_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'CompanyDescriptions' AND INDEX_NAME = 'IX_CompanyDescriptions_LanguageCode');
                SET @sql = IF(@idx_exists = 0,
                    'CREATE INDEX `IX_CompanyDescriptions_LanguageCode` ON `CompanyDescriptions` (`LanguageCode`)',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            // Add FK only if it doesn't already exist
            migrationBuilder.Sql("""
                SET @fk_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'CompanyDescriptions' AND CONSTRAINT_NAME = 'fk_company_descriptions_language');
                SET @sql = IF(@fk_exists = 0,
                    'ALTER TABLE `CompanyDescriptions` ADD CONSTRAINT `fk_company_descriptions_language` FOREIGN KEY (`LanguageCode`) REFERENCES `ISO_639-3` (`Id`) ON DELETE CASCADE',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_company_descriptions_language",
                table: "CompanyDescriptions");

            migrationBuilder.DropIndex(
                name: "idx_company_descriptions_company_language",
                table: "CompanyDescriptions");

            migrationBuilder.DropIndex(
                name: "IX_CompanyDescriptions_LanguageCode",
                table: "CompanyDescriptions");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "CompanyDescriptions");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDescriptions_CompanyId",
                table: "CompanyDescriptions",
                column: "CompanyId");
        }
    }
}

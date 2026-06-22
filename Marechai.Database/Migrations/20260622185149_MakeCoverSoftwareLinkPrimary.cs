using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class MakeCoverSoftwareLinkPrimary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Every step below is existence-checked: MySQL/MariaDB DDL is not transactional, so a
            // statement failing partway through this migration can leave earlier ALTERs already
            // committed. Re-running it after a partial failure must complete the remaining work
            // instead of erroring on steps that already applied.

            migrationBuilder.Sql(@"
                SET @fk_exists = (SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers'
                    AND CONSTRAINT_NAME = 'FK_SoftwareCovers_SoftwareReleases_SoftwareReleaseId');
                SET @sql = IF(@fk_exists > 0,
                    'ALTER TABLE `SoftwareCovers` DROP FOREIGN KEY `FK_SoftwareCovers_SoftwareReleases_SoftwareReleaseId`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @col_nullable = (SELECT IS_NULLABLE FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers' AND COLUMN_NAME = 'SoftwareReleaseId');
                SET @sql = IF(@col_nullable = 'NO',
                    'ALTER TABLE `SoftwareCovers` MODIFY COLUMN `SoftwareReleaseId` bigint unsigned NULL',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers' AND COLUMN_NAME = 'GroupId');
                SET @sql = IF(@col_exists = 0,
                    'ALTER TABLE `SoftwareCovers` ADD COLUMN `GroupId` varchar(64) CHARACTER SET utf8mb4 NULL',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers' AND COLUMN_NAME = 'SoftwareId');
                SET @sql = IF(@col_exists = 0,
                    'ALTER TABLE `SoftwareCovers` ADD COLUMN `SoftwareId` bigint unsigned NULL',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // A previous partial application of this migration (MySQL/MariaDB DDL isn't
            // transactional) may have already added SoftwareId as NOT NULL DEFAULT 0 before
            // failing on a later step. Make sure it ends up nullable regardless of which path
            // added it, otherwise the backfill below fails for compilation-release covers.
            migrationBuilder.Sql(@"
                SET @col_nullable = (SELECT IS_NULLABLE FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers' AND COLUMN_NAME = 'SoftwareId');
                SET @sql = IF(@col_nullable = 'NO',
                    'ALTER TABLE `SoftwareCovers` MODIFY COLUMN `SoftwareId` bigint unsigned NULL',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Backfill: every row existing before this migration has a SoftwareReleaseId. Most
            // resolve to a single owning Software (direct, or via the release's SoftwareVersion);
            // covers attached to a compilation release (which bundles several Software entries
            // with no single owner) are left NULL here and stay anchored by SoftwareReleaseId
            // alone, same as before this migration. Naturally idempotent — only touches rows that
            // still have a SoftwareReleaseId and re-derives the same value on a re-run.
            migrationBuilder.Sql(@"
                UPDATE SoftwareCovers sc
                JOIN SoftwareReleases sr ON sr.Id = sc.SoftwareReleaseId
                LEFT JOIN SoftwareVersions sv ON sv.Id = sr.SoftwareVersionId
                SET sc.SoftwareId = COALESCE(sr.SoftwareId, sv.SoftwareId)
                WHERE sc.SoftwareReleaseId IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                SET @idx_exists = (SELECT COUNT(*) FROM information_schema.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers' AND INDEX_NAME = 'IX_SoftwareCovers_GroupId');
                SET @sql = IF(@idx_exists = 0,
                    'CREATE INDEX `IX_SoftwareCovers_GroupId` ON `SoftwareCovers` (`GroupId`)',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @idx_exists = (SELECT COUNT(*) FROM information_schema.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers' AND INDEX_NAME = 'IX_SoftwareCovers_SoftwareId');
                SET @sql = IF(@idx_exists = 0,
                    'CREATE INDEX `IX_SoftwareCovers_SoftwareId` ON `SoftwareCovers` (`SoftwareId`)',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @fk_exists = (SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers'
                    AND CONSTRAINT_NAME = 'FK_SoftwareCovers_SoftwareReleases_SoftwareReleaseId');
                SET @sql = IF(@fk_exists = 0,
                    'ALTER TABLE `SoftwareCovers` ADD CONSTRAINT `FK_SoftwareCovers_SoftwareReleases_SoftwareReleaseId` FOREIGN KEY (`SoftwareReleaseId`) REFERENCES `SoftwareReleases` (`Id`) ON DELETE SET NULL',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @fk_exists = (SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers'
                    AND CONSTRAINT_NAME = 'FK_SoftwareCovers_Softwares_SoftwareId');
                SET @sql = IF(@fk_exists = 0,
                    'ALTER TABLE `SoftwareCovers` ADD CONSTRAINT `FK_SoftwareCovers_Softwares_SoftwareId` FOREIGN KEY (`SoftwareId`) REFERENCES `Softwares` (`Id`) ON DELETE CASCADE',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @fk_exists = (SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers'
                    AND CONSTRAINT_NAME = 'FK_SoftwareCovers_SoftwareReleases_SoftwareReleaseId');
                SET @sql = IF(@fk_exists > 0,
                    'ALTER TABLE `SoftwareCovers` DROP FOREIGN KEY `FK_SoftwareCovers_SoftwareReleases_SoftwareReleaseId`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @fk_exists = (SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers'
                    AND CONSTRAINT_NAME = 'FK_SoftwareCovers_Softwares_SoftwareId');
                SET @sql = IF(@fk_exists > 0,
                    'ALTER TABLE `SoftwareCovers` DROP FOREIGN KEY `FK_SoftwareCovers_Softwares_SoftwareId`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @idx_exists = (SELECT COUNT(*) FROM information_schema.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers' AND INDEX_NAME = 'IX_SoftwareCovers_GroupId');
                SET @sql = IF(@idx_exists > 0,
                    'DROP INDEX `IX_SoftwareCovers_GroupId` ON `SoftwareCovers`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @idx_exists = (SELECT COUNT(*) FROM information_schema.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers' AND INDEX_NAME = 'IX_SoftwareCovers_SoftwareId');
                SET @sql = IF(@idx_exists > 0,
                    'DROP INDEX `IX_SoftwareCovers_SoftwareId` ON `SoftwareCovers`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers' AND COLUMN_NAME = 'GroupId');
                SET @sql = IF(@col_exists > 0,
                    'ALTER TABLE `SoftwareCovers` DROP COLUMN `GroupId`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers' AND COLUMN_NAME = 'SoftwareId');
                SET @sql = IF(@col_exists > 0,
                    'ALTER TABLE `SoftwareCovers` DROP COLUMN `SoftwareId`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @col_nullable = (SELECT IS_NULLABLE FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers' AND COLUMN_NAME = 'SoftwareReleaseId');
                SET @sql = IF(@col_nullable = 'YES',
                    'ALTER TABLE `SoftwareCovers` MODIFY COLUMN `SoftwareReleaseId` bigint unsigned NOT NULL DEFAULT 0',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @fk_exists = (SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCovers'
                    AND CONSTRAINT_NAME = 'FK_SoftwareCovers_SoftwareReleases_SoftwareReleaseId');
                SET @sql = IF(@fk_exists = 0,
                    'ALTER TABLE `SoftwareCovers` ADD CONSTRAINT `FK_SoftwareCovers_SoftwareReleases_SoftwareReleaseId` FOREIGN KEY (`SoftwareReleaseId`) REFERENCES `SoftwareReleases` (`Id`) ON DELETE CASCADE',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }
    }
}

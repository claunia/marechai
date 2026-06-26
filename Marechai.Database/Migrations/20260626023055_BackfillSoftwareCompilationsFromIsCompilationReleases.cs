using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class BackfillSoftwareCompilationsFromIsCompilationReleases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MySQL/MariaDB DDL is not transactional. If a previous run timed out partway through
            // this migration, some schema/data changes may already be committed even though
            // __EFMigrationsHistory was never updated. Every step below is therefore written to be
            // safe on re-entry so production can resume from a partial application.

            // Temporary join-key column, dropped again in RemoveIsCompilationFromSoftwareReleases
            // once every step below has re-pointed everything it needs to via it.
            migrationBuilder.Sql(
                """
                SET @col_exists = (SELECT COUNT(*) FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCompilations' AND COLUMN_NAME = 'LegacyReleaseId');
                SET @sql = IF(@col_exists = 0,
                    'ALTER TABLE `SoftwareCompilations` ADD COLUMN `LegacyReleaseId` bigint unsigned NULL',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            // The backfill joins SoftwareCompilations by LegacyReleaseId several times; without an
            // index MariaDB scans the full table repeatedly and the cover update can exceed the
            // default 30 s command timeout on large datasets.
            migrationBuilder.Sql(
                """
                SET @idx_exists = (SELECT COUNT(*) FROM information_schema.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCompilations'
                      AND INDEX_NAME = 'IX_SoftwareCompilations_LegacyReleaseId');
                SET @sql = IF(@idx_exists = 0,
                    'CREATE INDEX `IX_SoftwareCompilations_LegacyReleaseId` ON `SoftwareCompilations` (`LegacyReleaseId`)',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            // 1. One SoftwareCompilations row per legacy IsCompilation=1 release (1:1 — merging
            //    same-compilation regional releases is a manual curation task, not automated here).
            migrationBuilder.Sql(
                """
                INSERT INTO SoftwareCompilations (Name, RelationshipType, LegacyReleaseId, CreatedOn, UpdatedOn)
                SELECT COALESCE(NULLIF(r.Title, ''), 'Compilation'), 0, r.Id, NOW(6), NOW(6)
                FROM SoftwareReleases r
                LEFT JOIN SoftwareCompilations comp ON comp.LegacyReleaseId = r.Id
                WHERE r.IsCompilation = 1 AND comp.Id IS NULL;
                """);

            // 2. Where the release had no Title, build a name from its included software/version
            //    junction rows, mirroring the old BuildSoftwareReleaseNewsNameAsync algorithm.
            migrationBuilder.Sql(
                """
                UPDATE SoftwareCompilations comp
                JOIN (
                    SELECT j.ReleaseId AS ReleaseId,
                           GROUP_CONCAT(j.ItemName ORDER BY j.ItemName SEPARATOR ' + ') AS BuiltName
                    FROM (
                        SELECT svbsr.ReleaseId AS ReleaseId,
                               CONVERT(CONCAT(sw.Name, ' ', sv.VersionString) USING utf8mb4) COLLATE utf8mb4_general_ci AS ItemName
                        FROM SoftwareVersionBySoftwareRelease svbsr
                        JOIN SoftwareVersions sv ON sv.Id = svbsr.SoftwareVersionId
                        JOIN Softwares sw ON sw.Id = sv.SoftwareId
                        UNION ALL
                        SELECT sbsr.ReleaseId AS ReleaseId,
                               CONVERT(sw2.Name USING utf8mb4) COLLATE utf8mb4_general_ci AS ItemName
                        FROM SoftwareBySoftwareRelease sbsr
                        JOIN Softwares sw2 ON sw2.Id = sbsr.SoftwareId
                    ) j
                    GROUP BY j.ReleaseId
                ) names ON names.ReleaseId = comp.LegacyReleaseId
                JOIN SoftwareReleases r ON r.Id = comp.LegacyReleaseId
                SET comp.Name = names.BuiltName
                WHERE r.Title IS NULL OR r.Title = '';
                """);

            // 3. Re-point each former compilation release at its new SoftwareCompilation.
            migrationBuilder.Sql(
                """
                UPDATE SoftwareReleases r
                JOIN SoftwareCompilations comp ON comp.LegacyReleaseId = r.Id
                SET r.SoftwareCompilationId = comp.Id
                WHERE r.IsCompilation = 1
                  AND (r.SoftwareCompilationId IS NULL OR r.SoftwareCompilationId <> comp.Id);
                """);

            // 4. Re-point covers that were anchored to a compilation release. Keep SoftwareReleaseId
            //    set in addition — it remains factually true which release the cover came from.
            migrationBuilder.Sql(
                """
                UPDATE SoftwareCovers sc
                JOIN SoftwareReleases r ON sc.SoftwareReleaseId = r.Id
                JOIN SoftwareCompilations comp ON comp.LegacyReleaseId = r.Id
                SET sc.SoftwareCompilationId = comp.Id
                WHERE r.IsCompilation = 1
                  AND (sc.SoftwareCompilationId IS NULL OR sc.SoftwareCompilationId <> comp.Id);
                """);

            // 5. Copy the old release-keyed junction rows into the new compilation-keyed tables.
            migrationBuilder.Sql(
                """
                INSERT INTO SoftwareBySoftwareCompilation (SoftwareCompilationId, SoftwareId)
                SELECT comp.Id, j.SoftwareId
                FROM SoftwareBySoftwareRelease j
                JOIN SoftwareCompilations comp ON comp.LegacyReleaseId = j.ReleaseId
                LEFT JOIN SoftwareBySoftwareCompilation existing
                       ON existing.SoftwareCompilationId = comp.Id AND existing.SoftwareId = j.SoftwareId
                WHERE existing.SoftwareCompilationId IS NULL;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO SoftwareVersionBySoftwareCompilation (SoftwareCompilationId, SoftwareVersionId)
                SELECT comp.Id, j.SoftwareVersionId
                FROM SoftwareVersionBySoftwareRelease j
                JOIN SoftwareCompilations comp ON comp.LegacyReleaseId = j.ReleaseId
                LEFT JOIN SoftwareVersionBySoftwareCompilation existing
                       ON existing.SoftwareCompilationId = comp.Id AND existing.SoftwareVersionId = j.SoftwareVersionId
                WHERE existing.SoftwareCompilationId IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE jbsc FROM SoftwareBySoftwareCompilation jbsc
                JOIN SoftwareCompilations comp ON comp.Id = jbsc.SoftwareCompilationId
                WHERE comp.LegacyReleaseId IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                DELETE jvbsc FROM SoftwareVersionBySoftwareCompilation jvbsc
                JOIN SoftwareCompilations comp ON comp.Id = jvbsc.SoftwareCompilationId
                WHERE comp.LegacyReleaseId IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE SoftwareCovers sc
                JOIN SoftwareCompilations comp ON comp.Id = sc.SoftwareCompilationId
                SET sc.SoftwareCompilationId = NULL
                WHERE comp.LegacyReleaseId IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE SoftwareReleases r
                JOIN SoftwareCompilations comp ON comp.Id = r.SoftwareCompilationId
                SET r.SoftwareCompilationId = NULL
                WHERE comp.LegacyReleaseId IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                DELETE FROM SoftwareCompilations WHERE LegacyReleaseId IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                SET @idx_exists = (SELECT COUNT(*) FROM information_schema.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCompilations'
                      AND INDEX_NAME = 'IX_SoftwareCompilations_LegacyReleaseId');
                SET @sql = IF(@idx_exists > 0,
                    'DROP INDEX `IX_SoftwareCompilations_LegacyReleaseId` ON `SoftwareCompilations`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.Sql(
                """
                SET @col_exists = (SELECT COUNT(*) FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCompilations' AND COLUMN_NAME = 'LegacyReleaseId');
                SET @sql = IF(@col_exists > 0,
                    'ALTER TABLE `SoftwareCompilations` DROP COLUMN `LegacyReleaseId`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);
        }
    }
}

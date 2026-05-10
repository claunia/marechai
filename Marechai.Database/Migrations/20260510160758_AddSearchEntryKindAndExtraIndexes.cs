using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchEntryKindAndExtraIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Kind",
                table: "SearchEntries",
                type: "tinyint unsigned",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchEntries_EntityType_CompanyId",
                table: "SearchEntries",
                columns: new[] { "EntityType", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchEntries_EntityType_Kind",
                table: "SearchEntries",
                columns: new[] { "EntityType", "Kind" });

            // Backfill: populate Kind for existing Software rows from Softwares.Kind.
            migrationBuilder.Sql(@"
                UPDATE `SearchEntries` se
                INNER JOIN `Softwares` sw ON sw.`Id` = se.`EntityId`
                SET se.`Kind` = sw.`Kind`
                WHERE se.`EntityType` = 12;");

            // Backfill: SoftwareCompilation rows are always Game (kind=2) per project policy.
            migrationBuilder.Sql(@"
                UPDATE `SearchEntries`
                SET `Kind` = 2
                WHERE `EntityType` = 13;");

            // Backfill HasImage for entity types whose original seed didn't include image lookups
            // (Computer/Console/Smartphone/Gpu/Processor/SoundSynth/Software/SoftwareCompilation).
            // Each block flips rows whose source has at least one image.

            migrationBuilder.Sql(@"
                UPDATE `SearchEntries` se
                SET se.`HasImage` = 1
                WHERE se.`EntityType` IN (2,3,4)
                  AND EXISTS (SELECT 1 FROM `MachinePhotos` mp WHERE mp.`MachineId` = se.`EntityId`);");

            migrationBuilder.Sql(@"
                UPDATE `SearchEntries` se
                SET se.`HasImage` = 1
                WHERE se.`EntityType` = 8
                  AND EXISTS (SELECT 1 FROM `GpuPhotos` p WHERE p.`GpuId` = se.`EntityId`);");

            migrationBuilder.Sql(@"
                UPDATE `SearchEntries` se
                SET se.`HasImage` = 1
                WHERE se.`EntityType` = 9
                  AND EXISTS (SELECT 1 FROM `ProcessorPhotos` p WHERE p.`ProcessorId` = se.`EntityId`);");

            migrationBuilder.Sql(@"
                UPDATE `SearchEntries` se
                SET se.`HasImage` = 1
                WHERE se.`EntityType` = 10
                  AND EXISTS (SELECT 1 FROM `SoundSynthPhotos` p WHERE p.`SoundSynthId` = se.`EntityId`);");

            migrationBuilder.Sql(@"
                UPDATE `SearchEntries` se
                SET se.`HasImage` = 1
                WHERE se.`EntityType` = 12
                  AND (EXISTS (SELECT 1 FROM `SoftwareScreenshots` s WHERE s.`SoftwareId` = se.`EntityId`)
                    OR EXISTS (SELECT 1 FROM `SoftwarePromoArt` p WHERE p.`SoftwareId` = se.`EntityId`)
                    OR EXISTS (SELECT 1 FROM `SoftwareCovers` c
                               INNER JOIN `SoftwareReleases` r ON r.`Id` = c.`SoftwareReleaseId`
                               WHERE r.`SoftwareId` = se.`EntityId`));");

            migrationBuilder.Sql(@"
                UPDATE `SearchEntries` se
                SET se.`HasImage` = 1
                WHERE se.`EntityType` = 13
                  AND EXISTS (SELECT 1 FROM `SoftwareCovers` c WHERE c.`SoftwareReleaseId` = se.`EntityId`);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SearchEntries_EntityType_CompanyId",
                table: "SearchEntries");

            migrationBuilder.DropIndex(
                name: "IX_SearchEntries_EntityType_Kind",
                table: "SearchEntries");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "SearchEntries");
        }
    }
}

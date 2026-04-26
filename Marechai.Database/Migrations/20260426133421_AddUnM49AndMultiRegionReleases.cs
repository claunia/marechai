using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUnM49AndMultiRegionReleases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clean up any partially-applied previous run
            migrationBuilder.Sql("DROP TABLE IF EXISTS `UnM49BySoftwareRelease`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `UnM49`;");

            // Step 1: Create UnM49 table
            migrationBuilder.CreateTable(
                name: "UnM49",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentId = table.Column<short>(type: "smallint", nullable: true),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnM49", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnM49_UnM49_ParentId",
                        column: x => x.ParentId,
                        principalTable: "UnM49",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UnM49_ParentId",
                table: "UnM49",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_UnM49_Type",
                table: "UnM49",
                column: "Type");

            // Step 2: Seed UN M.49 geographic regions (World, Continents, Sub-regions)
            migrationBuilder.Sql("""
                INSERT INTO UnM49 (Id, Name, ParentId, Type) VALUES
                -- World
                (1, 'World', NULL, 0),
                -- Continents
                (2, 'Africa', 1, 1),
                (19, 'Americas', 1, 1),
                (142, 'Asia', 1, 1),
                (150, 'Europe', 1, 1),
                (9, 'Oceania', 1, 1),
                -- Africa sub-regions
                (15, 'Northern Africa', 2, 2),
                (202, 'Sub-Saharan Africa', 2, 2),
                (14, 'Eastern Africa', 202, 2),
                (17, 'Middle Africa', 202, 2),
                (18, 'Southern Africa', 202, 2),
                (11, 'Western Africa', 202, 2),
                -- Americas sub-regions
                (419, 'Latin America and the Caribbean', 19, 2),
                (29, 'Caribbean', 419, 2),
                (13, 'Central America', 419, 2),
                (5, 'South America', 419, 2),
                (21, 'Northern America', 19, 2),
                -- Asia sub-regions
                (143, 'Central Asia', 142, 2),
                (30, 'Eastern Asia', 142, 2),
                (35, 'South-eastern Asia', 142, 2),
                (34, 'Southern Asia', 142, 2),
                (145, 'Western Asia', 142, 2),
                -- Europe sub-regions
                (151, 'Eastern Europe', 150, 2),
                (154, 'Northern Europe', 150, 2),
                (39, 'Southern Europe', 150, 2),
                (155, 'Western Europe', 150, 2),
                -- Oceania sub-regions
                (53, 'Australia and New Zealand', 9, 2),
                (54, 'Melanesia', 9, 2),
                (57, 'Micronesia', 9, 2),
                (61, 'Polynesia', 9, 2),
                -- Antarctica (directly under World)
                (10, 'Antarctica', 1, 2);
                """);

            // Step 3: Copy countries from Iso31661Numeric into UnM49 with sub-region parent assignments
            migrationBuilder.Sql("""
                INSERT INTO UnM49 (Id, Name, ParentId, Type)
                SELECT c.id, c.name,
                    CASE
                        -- Northern Africa
                        WHEN c.id IN (12,818,434,504,729,788,732) THEN 15
                        -- Eastern Africa
                        WHEN c.id IN (108,174,262,232,231,404,450,454,480,175,508,638,646,690,706,728,834,800,894,716,86) THEN 14
                        -- Middle Africa
                        WHEN c.id IN (24,120,140,148,178,180,226,266,678) THEN 17
                        -- Southern Africa
                        WHEN c.id IN (72,748,426,516,710) THEN 18
                        -- Western Africa
                        WHEN c.id IN (204,854,132,384,270,288,324,624,430,466,478,562,566,654,686,694,768) THEN 11
                        -- Caribbean
                        WHEN c.id IN (660,28,533,44,52,535,92,136,192,531,212,214,308,312,332,388,474,500,630,652,659,662,663,534,670,780,796,850) THEN 29
                        -- Central America
                        WHEN c.id IN (84,188,222,320,340,484,558,591) THEN 13
                        -- South America
                        WHEN c.id IN (32,68,76,152,170,218,238,254,328,600,604,740,858,862) THEN 5
                        -- Northern America
                        WHEN c.id IN (60,124,304,666,840,581) THEN 21
                        -- Central Asia
                        WHEN c.id IN (398,417,762,795,860) THEN 143
                        -- Eastern Asia
                        WHEN c.id IN (156,344,446,408,392,496,410) THEN 30
                        -- South-eastern Asia
                        WHEN c.id IN (96,116,360,418,458,104,608,702,764,626,704) THEN 35
                        -- Southern Asia
                        WHEN c.id IN (4,50,64,356,364,462,524,586,144) THEN 34
                        -- Western Asia
                        WHEN c.id IN (51,31,48,196,268,368,376,400,414,422,512,634,682,275,760,792,784,887) THEN 145
                        -- Eastern Europe
                        WHEN c.id IN (112,100,203,348,616,498,642,643,703,804) THEN 151
                        -- Northern Europe
                        WHEN c.id IN (248,831,832,208,233,234,246,352,372,833,428,440,578,744,752,826) THEN 154
                        -- Southern Europe
                        WHEN c.id IN (8,20,70,191,292,300,336,380,470,499,807,620,674,688,705,724) THEN 39
                        -- Western Europe
                        WHEN c.id IN (40,56,250,276,438,442,492,528,756) THEN 155
                        -- Australia and New Zealand
                        WHEN c.id IN (36,162,166,334,554,574) THEN 53
                        -- Melanesia
                        WHEN c.id IN (242,540,598,90,548) THEN 54
                        -- Micronesia
                        WHEN c.id IN (316,296,584,583,520,580,585) THEN 57
                        -- Polynesia
                        WHEN c.id IN (16,184,258,570,612,882,772,776,798,876) THEN 61
                        -- Antarctica
                        WHEN c.id = 10 THEN 1
                        -- Unmapped countries go under World
                        ELSE 1
                    END,
                    3
                FROM iso3166_1_numeric c
                WHERE c.id NOT IN (1,2,5,9,10,11,13,14,15,17,18,19,21,29,30,34,35,39,53,54,57,61,142,143,145,150,151,154,155,202,419);
                """);

            // Step 4: Create junction table
            migrationBuilder.CreateTable(
                name: "UnM49BySoftwareRelease",
                columns: table => new
                {
                    SoftwareReleaseId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    UnM49Id = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnM49BySoftwareRelease", x => new { x.SoftwareReleaseId, x.UnM49Id });
                    table.ForeignKey(
                        name: "FK_UnM49BySoftwareRelease_SoftwareReleases_SoftwareReleaseId",
                        column: x => x.SoftwareReleaseId,
                        principalTable: "SoftwareReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnM49BySoftwareRelease_UnM49_UnM49Id",
                        column: x => x.UnM49Id,
                        principalTable: "UnM49",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UnM49BySoftwareRelease_UnM49Id",
                table: "UnM49BySoftwareRelease",
                column: "UnM49Id");

            // Step 5: Migrate existing RegionId data to junction table
            migrationBuilder.Sql("""
                INSERT INTO UnM49BySoftwareRelease (SoftwareReleaseId, UnM49Id)
                SELECT Id, RegionId
                FROM SoftwareReleases
                WHERE RegionId IS NOT NULL AND RegionId != 0;
                """);

            // Step 6: Create new composite index FIRST so MySQL has an alternative
            // backing index for the SoftwareVersionId FK before we drop the old one
            migrationBuilder.CreateIndex(
                name: "IX_SoftwareReleases_SoftwareVersionId_PlatformId",
                table: "SoftwareReleases",
                columns: new[] { "SoftwareVersionId", "PlatformId" });

            // Step 7: Drop old Region FK, indexes, and column
            // Use raw SQL because the FK name in the DB may differ from what EF expects
            migrationBuilder.Sql("""
                SET @fk_name = (SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'SoftwareReleases'
                      AND COLUMN_NAME = 'RegionId'
                      AND REFERENCED_TABLE_NAME IS NOT NULL
                    LIMIT 1);
                SET @sql = IF(@fk_name IS NOT NULL,
                    CONCAT('ALTER TABLE `SoftwareReleases` DROP FOREIGN KEY `', @fk_name, '`'),
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.Sql("""
                SET @idx_exists = (SELECT COUNT(1) FROM information_schema.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'SoftwareReleases'
                      AND INDEX_NAME = 'IX_SoftwareReleases_RegionId');
                SET @sql = IF(@idx_exists > 0,
                    'ALTER TABLE `SoftwareReleases` DROP INDEX `IX_SoftwareReleases_RegionId`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.Sql("""
                SET @idx_exists = (SELECT COUNT(1) FROM information_schema.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'SoftwareReleases'
                      AND INDEX_NAME = 'IX_SoftwareReleases_SoftwareVersionId_RegionId_PlatformId');
                SET @sql = IF(@idx_exists > 0,
                    'ALTER TABLE `SoftwareReleases` DROP INDEX `IX_SoftwareReleases_SoftwareVersionId_RegionId_PlatformId`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "SoftwareReleases");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnM49BySoftwareRelease");

            migrationBuilder.DropTable(
                name: "UnM49");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareReleases_SoftwareVersionId_PlatformId",
                table: "SoftwareReleases");

            migrationBuilder.AddColumn<short>(
                name: "RegionId",
                table: "SoftwareReleases",
                type: "smallint(3)",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareReleases_RegionId",
                table: "SoftwareReleases",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareReleases_SoftwareVersionId_RegionId_PlatformId",
                table: "SoftwareReleases",
                columns: new[] { "SoftwareVersionId", "RegionId", "PlatformId" });

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareReleases_iso3166_1_numeric_RegionId",
                table: "SoftwareReleases",
                column: "RegionId",
                principalTable: "iso3166_1_numeric",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

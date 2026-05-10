using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EntityType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    EntityId = table.Column<long>(type: "bigint", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AltName = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedName = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Year = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<short>(type: "smallint", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    HasImage = table.Column<bool>(type: "bit(1)", nullable: false),
                    Soundex = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchEntries", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SearchEntries_EntityType_CountryId",
                table: "SearchEntries",
                columns: new[] { "EntityType", "CountryId" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchEntries_EntityType_EntityId",
                table: "SearchEntries",
                columns: new[] { "EntityType", "EntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchEntries_EntityType_Year",
                table: "SearchEntries",
                columns: new[] { "EntityType", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchEntries_Soundex",
                table: "SearchEntries",
                column: "Soundex");

            // FULLTEXT index on NormalizedName using MariaDB's default parser.
            // The ngram parser is a MySQL 5.7+ feature not available in MariaDB; we use the
            // default parser and rely on BOOLEAN MODE prefix wildcards (`query*`) at query time
            // for partial-token matching, layered with prefix LIKE for sub-token autocomplete.
            // ft_min_word_len defaults to 3 for InnoDB which suits our 3-char minimum query.
            migrationBuilder.Sql(
                @"ALTER TABLE `SearchEntries`
                  ADD FULLTEXT INDEX `FT_SearchEntries_NormalizedName` (`NormalizedName`);");

            // Seed: populate from all 13 source tables in a single transaction.
            // Each block is INSERT IGNORE so re-running on a partially-populated DB is harmless.
            // Normalization mirrors SearchIndexUpdater.Normalize: lowercased + non-alphanumeric → space + collapsed.
            // We use LOWER + REGEXP_REPLACE; full diacritics-strip happens on subsequent live updates via the interceptor.

            // 1) Companies → SearchEntries.EntityType=Company(1)
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `SearchEntries`
                    (`EntityType`,`EntityId`,`DisplayName`,`AltName`,`NormalizedName`,`Year`,`CountryId`,`CompanyId`,`HasImage`,`Soundex`)
                SELECT 1, c.`id`, LEFT(c.`name`, 512), LEFT(c.`LegalName`, 512),
                       LEFT(TRIM(REGEXP_REPLACE(LOWER(CONCAT_WS(' ', c.`name`, c.`LegalName`)), '[^a-z0-9]+', ' ')), 1024),
                       YEAR(c.`founded`), c.`country`, NULL, FALSE,
                       LEFT(SOUNDEX(c.`name`), 10)
                FROM `companies` c WHERE c.`name` IS NOT NULL AND c.`name` <> '';");

            // 2/3/4) Machines → Computer(2)/Console(3)/Smartphone(4) based on Type
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `SearchEntries`
                    (`EntityType`,`EntityId`,`DisplayName`,`AltName`,`NormalizedName`,`Year`,`CountryId`,`CompanyId`,`HasImage`,`Soundex`)
                SELECT
                    CASE m.`type` WHEN 2 THEN 3 WHEN 3 THEN 4 ELSE 2 END,
                    m.`id`, LEFT(m.`name`, 512), LEFT(m.`model`, 512),
                    LEFT(TRIM(REGEXP_REPLACE(LOWER(CONCAT_WS(' ', m.`name`, m.`model`)), '[^a-z0-9]+', ' ')), 1024),
                    YEAR(m.`introduced`), NULL, m.`company`, FALSE,
                    LEFT(SOUNDEX(m.`name`), 10)
                FROM `machines` m WHERE m.`name` IS NOT NULL AND m.`name` <> '';");

            // 5) Books → 5
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `SearchEntries`
                    (`EntityType`,`EntityId`,`DisplayName`,`AltName`,`NormalizedName`,`Year`,`CountryId`,`CompanyId`,`HasImage`,`Soundex`)
                SELECT 5, b.`Id`, LEFT(b.`Title`, 512), LEFT(b.`NativeTitle`, 512),
                       LEFT(TRIM(REGEXP_REPLACE(LOWER(CONCAT_WS(' ', b.`Title`, b.`NativeTitle`)), '[^a-z0-9]+', ' ')), 1024),
                       YEAR(b.`Published`), b.`CountryId`, NULL,
                       (b.`CoverGuid` IS NOT NULL),
                       LEFT(SOUNDEX(b.`Title`), 10)
                FROM `Books` b WHERE b.`Title` IS NOT NULL AND b.`Title` <> '';");

            // 6) Documents → 6
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `SearchEntries`
                    (`EntityType`,`EntityId`,`DisplayName`,`AltName`,`NormalizedName`,`Year`,`CountryId`,`CompanyId`,`HasImage`,`Soundex`)
                SELECT 6, d.`Id`, LEFT(d.`Title`, 512), LEFT(d.`NativeTitle`, 512),
                       LEFT(TRIM(REGEXP_REPLACE(LOWER(CONCAT_WS(' ', d.`Title`, d.`NativeTitle`)), '[^a-z0-9]+', ' ')), 1024),
                       YEAR(d.`Published`), d.`CountryId`, NULL, FALSE,
                       LEFT(SOUNDEX(d.`Title`), 10)
                FROM `Documents` d WHERE d.`Title` IS NOT NULL AND d.`Title` <> '';");

            // 7) Magazines → 7 (year preferred from FirstPublication)
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `SearchEntries`
                    (`EntityType`,`EntityId`,`DisplayName`,`AltName`,`NormalizedName`,`Year`,`CountryId`,`CompanyId`,`HasImage`,`Soundex`)
                SELECT 7, mg.`Id`, LEFT(mg.`Title`, 512), LEFT(mg.`NativeTitle`, 512),
                       LEFT(TRIM(REGEXP_REPLACE(LOWER(CONCAT_WS(' ', mg.`Title`, mg.`NativeTitle`)), '[^a-z0-9]+', ' ')), 1024),
                       YEAR(COALESCE(mg.`FirstPublication`, mg.`Published`)), mg.`CountryId`, NULL, FALSE,
                       LEFT(SOUNDEX(mg.`Title`), 10)
                FROM `Magazines` mg WHERE mg.`Title` IS NOT NULL AND mg.`Title` <> '';");

            // 8) Gpus → 8
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `SearchEntries`
                    (`EntityType`,`EntityId`,`DisplayName`,`AltName`,`NormalizedName`,`Year`,`CountryId`,`CompanyId`,`HasImage`,`Soundex`)
                SELECT 8, g.`id`, LEFT(g.`name`, 512), LEFT(g.`model_code`, 512),
                       LEFT(TRIM(REGEXP_REPLACE(LOWER(CONCAT_WS(' ', g.`name`, g.`model_code`)), '[^a-z0-9]+', ' ')), 1024),
                       YEAR(g.`introduced`), NULL, g.`company`, FALSE,
                       LEFT(SOUNDEX(g.`name`), 10)
                FROM `gpus` g WHERE g.`name` IS NOT NULL AND g.`name` <> '';");

            // 9) Processors → 9
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `SearchEntries`
                    (`EntityType`,`EntityId`,`DisplayName`,`AltName`,`NormalizedName`,`Year`,`CountryId`,`CompanyId`,`HasImage`,`Soundex`)
                SELECT 9, p.`id`, LEFT(p.`name`, 512), LEFT(p.`model_code`, 512),
                       LEFT(TRIM(REGEXP_REPLACE(LOWER(CONCAT_WS(' ', p.`name`, p.`model_code`)), '[^a-z0-9]+', ' ')), 1024),
                       YEAR(p.`introduced`), NULL, p.`company`, FALSE,
                       LEFT(SOUNDEX(p.`name`), 10)
                FROM `processors` p WHERE p.`name` IS NOT NULL AND p.`name` <> '';");

            // 10) SoundSynths → 10
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `SearchEntries`
                    (`EntityType`,`EntityId`,`DisplayName`,`AltName`,`NormalizedName`,`Year`,`CountryId`,`CompanyId`,`HasImage`,`Soundex`)
                SELECT 10, s.`id`, LEFT(s.`name`, 512), LEFT(s.`model_code`, 512),
                       LEFT(TRIM(REGEXP_REPLACE(LOWER(CONCAT_WS(' ', s.`name`, s.`model_code`)), '[^a-z0-9]+', ' ')), 1024),
                       YEAR(s.`introduced`), NULL, s.`company`, FALSE,
                       LEFT(SOUNDEX(s.`name`), 10)
                FROM `sound_synths` s WHERE s.`name` IS NOT NULL AND s.`name` <> '';");

            // 11) People → 11 (display = Name + ' ' + Surname; alt = Alias)
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `SearchEntries`
                    (`EntityType`,`EntityId`,`DisplayName`,`AltName`,`NormalizedName`,`Year`,`CountryId`,`CompanyId`,`HasImage`,`Soundex`)
                SELECT 11, pe.`Id`, LEFT(TRIM(CONCAT_WS(' ', pe.`Name`, pe.`Surname`)), 512), LEFT(pe.`Alias`, 512),
                       LEFT(TRIM(REGEXP_REPLACE(LOWER(CONCAT_WS(' ', pe.`Name`, pe.`Surname`, pe.`Alias`)), '[^a-z0-9]+', ' ')), 1024),
                       YEAR(pe.`BirthDate`), pe.`CountryOfBirthId`, NULL,
                       (pe.`Photo` IS NOT NULL AND pe.`Photo` <> '00000000-0000-0000-0000-000000000000'),
                       LEFT(SOUNDEX(pe.`Surname`), 10)
                FROM `People` pe WHERE pe.`Name` IS NOT NULL OR pe.`Surname` IS NOT NULL;");

            // 12) Software → 12
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `SearchEntries`
                    (`EntityType`,`EntityId`,`DisplayName`,`AltName`,`NormalizedName`,`Year`,`CountryId`,`CompanyId`,`HasImage`,`Soundex`)
                SELECT 12, sw.`Id`, LEFT(sw.`Name`, 512), NULL,
                       LEFT(TRIM(REGEXP_REPLACE(LOWER(sw.`Name`), '[^a-z0-9]+', ' ')), 1024),
                       NULL, NULL, NULL, FALSE,
                       LEFT(SOUNDEX(sw.`Name`), 10)
                FROM `Softwares` sw WHERE sw.`Name` IS NOT NULL AND sw.`Name` <> '';");

            // 13) SoftwareReleases (compilations only) → 13
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `SearchEntries`
                    (`EntityType`,`EntityId`,`DisplayName`,`AltName`,`NormalizedName`,`Year`,`CountryId`,`CompanyId`,`HasImage`,`Soundex`)
                SELECT 13, sr.`Id`, LEFT(sr.`Title`, 512), NULL,
                       LEFT(TRIM(REGEXP_REPLACE(LOWER(sr.`Title`), '[^a-z0-9]+', ' ')), 1024),
                       YEAR(sr.`ReleaseDate`), NULL, sr.`PublisherId`, FALSE,
                       LEFT(SOUNDEX(sr.`Title`), 10)
                FROM `SoftwareReleases` sr
                WHERE sr.`IsCompilation` = TRUE AND sr.`Title` IS NOT NULL AND sr.`Title` <> '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchEntries");
        }
    }
}

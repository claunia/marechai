using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwareExternalIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "IgdbGames",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ExternalSites",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UrlTemplate = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalSites", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareExternalIds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ExternalSiteId = table.Column<long>(type: "bigint", nullable: false),
                    ExternalId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareExternalIds", x => x.Id);
                    table.ForeignKey(
                        name: "fk_software_external_ids_external_site",
                        column: x => x.ExternalSiteId,
                        principalTable: "ExternalSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_software_external_ids_software",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_external_sites_name",
                table: "ExternalSites",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_software_external_ids_site_external_id",
                table: "SoftwareExternalIds",
                columns: new[] { "ExternalSiteId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_software_external_ids_software",
                table: "SoftwareExternalIds",
                column: "SoftwareId");

            migrationBuilder.Sql(
                """
                INSERT INTO `ExternalSites` (`Name`, `UrlTemplate`, `CreatedOn`, `UpdatedOn`)
                VALUES
                    ('MobyGames', 'https://www.mobygames.com/game/{id}', UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
                    ('IGDB', 'https://www.igdb.com/games/{id}', UTC_TIMESTAMP(6), UTC_TIMESTAMP(6));
                """);

            // MobyGames: prefer the numeric id (always resolvable to a URL on its own); fall back to
            // the slug when the numeric id hasn't been resolved yet for that row.
            migrationBuilder.Sql(
                """
                INSERT IGNORE INTO `SoftwareExternalIds` (`SoftwareId`, `ExternalSiteId`, `ExternalId`, `CreatedOn`, `UpdatedOn`)
                SELECT `SoftwareId`, (SELECT `Id` FROM `ExternalSites` WHERE `Name` = 'MobyGames'),
                       COALESCE(CAST(`MobyNumericId` AS CHAR), `MobyGameId`),
                       UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
                FROM `MobyGamesImportStates`
                WHERE `SoftwareId` IS NOT NULL;
                """);

            // IGDB: prefer the slug (required for a working igdb.com URL); fall back to the numeric
            // id for rows mirrored before the slug was tracked (backfilled separately afterwards).
            migrationBuilder.Sql(
                """
                INSERT IGNORE INTO `SoftwareExternalIds` (`SoftwareId`, `ExternalSiteId`, `ExternalId`, `CreatedOn`, `UpdatedOn`)
                SELECT `SoftwareId`, (SELECT `Id` FROM `ExternalSites` WHERE `Name` = 'IGDB'),
                       COALESCE(NULLIF(`Slug`, ''), CAST(`IgdbId` AS CHAR)),
                       UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
                FROM `IgdbGames`
                WHERE `SoftwareId` IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SoftwareExternalIds");

            migrationBuilder.DropTable(
                name: "ExternalSites");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "IgdbGames");
        }
    }
}

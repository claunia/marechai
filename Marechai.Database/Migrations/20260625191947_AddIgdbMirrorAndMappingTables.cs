using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddIgdbMirrorAndMappingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IgdbCompanies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IgdbId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchStatus = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MatchType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchScore = table.Column<double>(type: "double", nullable: true),
                    MatchedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CandidatesJson = table.Column<string>(type: "varchar(4096)", maxLength: 4096, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorMessage = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BatchNumber = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgdbCompanies", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IgdbExternalGames",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IgdbId = table.Column<long>(type: "bigint", nullable: false),
                    GameIgdbId = table.Column<long>(type: "bigint", nullable: false),
                    Uid = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgdbExternalGames", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IgdbGames",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IgdbId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GameTypeId = table.Column<int>(type: "int", nullable: true),
                    ParentGameId = table.Column<long>(type: "bigint", nullable: true),
                    VersionParentId = table.Column<long>(type: "bigint", nullable: true),
                    PlatformIdsJson = table.Column<string>(type: "varchar(4096)", maxLength: 4096, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchStatus = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MatchType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchScore = table.Column<double>(type: "double", nullable: true),
                    PlatformOverlapScore = table.Column<double>(type: "double", nullable: true),
                    CandidatesJson = table.Column<string>(type: "varchar(4096)", maxLength: 4096, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BatchNumber = table.Column<int>(type: "int", nullable: false),
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgdbGames", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IgdbGameTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgdbGameTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IgdbInvolvedCompanies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IgdbId = table.Column<long>(type: "bigint", nullable: false),
                    GameIgdbId = table.Column<long>(type: "bigint", nullable: false),
                    CompanyIgdbId = table.Column<long>(type: "bigint", nullable: false),
                    Developer = table.Column<bool>(type: "bit(1)", nullable: false),
                    Publisher = table.Column<bool>(type: "bit(1)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgdbInvolvedCompanies", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IgdbPlatforms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchStatus = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MatchType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SoftwarePlatformId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgdbPlatforms", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbCompanies_CompanyId",
                table: "IgdbCompanies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbCompanies_IgdbId",
                table: "IgdbCompanies",
                column: "IgdbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IgdbCompanies_MatchStatus",
                table: "IgdbCompanies",
                column: "MatchStatus");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbCompanies_Name",
                table: "IgdbCompanies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbExternalGames_GameIgdbId",
                table: "IgdbExternalGames",
                column: "GameIgdbId");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbExternalGames_IgdbId",
                table: "IgdbExternalGames",
                column: "IgdbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IgdbExternalGames_Uid",
                table: "IgdbExternalGames",
                column: "Uid");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbGames_GameTypeId",
                table: "IgdbGames",
                column: "GameTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbGames_IgdbId",
                table: "IgdbGames",
                column: "IgdbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IgdbGames_MatchStatus",
                table: "IgdbGames",
                column: "MatchStatus");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbGames_Name",
                table: "IgdbGames",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbGames_ParentGameId",
                table: "IgdbGames",
                column: "ParentGameId");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbGames_SoftwareId",
                table: "IgdbGames",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbGames_VersionParentId",
                table: "IgdbGames",
                column: "VersionParentId");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbInvolvedCompanies_CompanyIgdbId",
                table: "IgdbInvolvedCompanies",
                column: "CompanyIgdbId");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbInvolvedCompanies_GameIgdbId",
                table: "IgdbInvolvedCompanies",
                column: "GameIgdbId");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbInvolvedCompanies_IgdbId",
                table: "IgdbInvolvedCompanies",
                column: "IgdbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IgdbPlatforms_MatchStatus",
                table: "IgdbPlatforms",
                column: "MatchStatus");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbPlatforms_SoftwarePlatformId",
                table: "IgdbPlatforms",
                column: "SoftwarePlatformId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IgdbCompanies");

            migrationBuilder.DropTable(
                name: "IgdbExternalGames");

            migrationBuilder.DropTable(
                name: "IgdbGames");

            migrationBuilder.DropTable(
                name: "IgdbGameTypes");

            migrationBuilder.DropTable(
                name: "IgdbInvolvedCompanies");

            migrationBuilder.DropTable(
                name: "IgdbPlatforms");
        }
    }
}

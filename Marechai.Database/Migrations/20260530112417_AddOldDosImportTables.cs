using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddOldDosImportTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OldDosCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    RussianName = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnglishName = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Path = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastCrawledOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OldDosCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OldDosCategories_OldDosCategories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "OldDosCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OldDosOsPlatformMaps",
                columns: table => new
                {
                    OsName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwarePlatformId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    IgnoreOnPromote = table.Column<bool>(type: "bit(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OldDosOsPlatformMaps", x => x.OsName);
                    table.ForeignKey(
                        name: "FK_OldDosOsPlatformMaps_SoftwarePlatforms_SoftwarePlatformId",
                        column: x => x.SoftwarePlatformId,
                        principalTable: "SoftwarePlatforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OldDosSoftwares",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    SourceUrl = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Name = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeveloperName = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OsName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RussianDescription = table.Column<string>(type: "longtext", maxLength: 262144, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RussianCategoryPath = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldDosCategoryId = table.Column<int>(type: "int", nullable: true),
                    CrawledOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastError = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnglishDescriptionLiteral = table.Column<string>(type: "longtext", maxLength: 262144, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnglishDescriptionMuseum = table.Column<string>(type: "longtext", maxLength: 262144, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MuseumDescriptionPromptVersion = table.Column<int>(type: "int", nullable: false),
                    SuggestedGenreIdsJson = table.Column<string>(type: "varchar(4096)", maxLength: 4096, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewedBy = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PromotedSoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OldDosSoftwares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OldDosSoftwares_OldDosCategories_OldDosCategoryId",
                        column: x => x.OldDosCategoryId,
                        principalTable: "OldDosCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OldDosVersions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OldDosSoftwareId = table.Column<long>(type: "bigint", nullable: false),
                    VersionString = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReleaseDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReleaseDatePrecision = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    OsHint = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DownloadUrl = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileName = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsEnabledByDefault = table.Column<bool>(type: "bit(1)", nullable: false),
                    PromotedSoftwareVersionId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OldDosVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OldDosVersions_OldDosSoftwares_OldDosSoftwareId",
                        column: x => x.OldDosSoftwareId,
                        principalTable: "OldDosSoftwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OldDosCategories_ParentId",
                table: "OldDosCategories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OldDosOsPlatformMaps_SoftwarePlatformId",
                table: "OldDosOsPlatformMaps",
                column: "SoftwarePlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_OldDosSoftwares_OldDosCategoryId",
                table: "OldDosSoftwares",
                column: "OldDosCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OldDosSoftwares_SourceId",
                table: "OldDosSoftwares",
                column: "SourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OldDosSoftwares_SourceUrl",
                table: "OldDosSoftwares",
                column: "SourceUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OldDosSoftwares_Status",
                table: "OldDosSoftwares",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OldDosVersions_OldDosSoftwareId",
                table: "OldDosVersions",
                column: "OldDosSoftwareId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OldDosOsPlatformMaps");

            migrationBuilder.DropTable(
                name: "OldDosVersions");

            migrationBuilder.DropTable(
                name: "OldDosSoftwares");

            migrationBuilder.DropTable(
                name: "OldDosCategories");
        }
    }
}

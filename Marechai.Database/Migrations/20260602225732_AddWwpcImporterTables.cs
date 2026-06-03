using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddWwpcImporterTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WwpcCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    LastCrawledOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WwpcCategories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WwpcSoftwares",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SourceUrl = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Slug = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ProductType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Name = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VendorName = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VendorUrl = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawCategoriesCsv = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlatformsCsv = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReleaseDateText = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserInterface = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawDescription = table.Column<string>(type: "longtext", maxLength: 262144, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnglishDescriptionMuseum = table.Column<string>(type: "longtext", maxLength: 262144, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MuseumDescriptionPromptVersion = table.Column<int>(type: "int", nullable: false),
                    SuggestedGenreIdsJson = table.Column<string>(type: "varchar(4096)", maxLength: 4096, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SuggestedVendorCompanyId = table.Column<int>(type: "int", nullable: true),
                    WwpcCategoryId = table.Column<int>(type: "int", nullable: true),
                    CrawledOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastError = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
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
                    table.PrimaryKey("PK_WwpcSoftwares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WwpcSoftwares_WwpcCategories_WwpcCategoryId",
                        column: x => x.WwpcCategoryId,
                        principalTable: "WwpcCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WwpcScreenshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WwpcSoftwareId = table.Column<long>(type: "bigint", nullable: false),
                    MajorRelease = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceUrl = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrl = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Caption = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SuggestedSoftwarePlatformId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    IsEnabledByDefault = table.Column<bool>(type: "bit(1)", nullable: false),
                    CrawledOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PromotedSoftwareScreenshotId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WwpcScreenshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WwpcScreenshots_WwpcSoftwares_WwpcSoftwareId",
                        column: x => x.WwpcSoftwareId,
                        principalTable: "WwpcSoftwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WwpcVersions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WwpcSoftwareId = table.Column<long>(type: "bigint", nullable: false),
                    MajorRelease = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MajorReleaseUrl = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VersionString = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Language = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Architecture = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MediaKind = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SizeText = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DownloadUrl = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
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
                    table.PrimaryKey("PK_WwpcVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WwpcVersions_WwpcSoftwares_WwpcSoftwareId",
                        column: x => x.WwpcSoftwareId,
                        principalTable: "WwpcSoftwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WwpcCategories_ProductType_Name",
                table: "WwpcCategories",
                columns: new[] { "ProductType", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WwpcScreenshots_SourceUrl",
                table: "WwpcScreenshots",
                column: "SourceUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WwpcScreenshots_WwpcSoftwareId",
                table: "WwpcScreenshots",
                column: "WwpcSoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_WwpcSoftwares_ProductType",
                table: "WwpcSoftwares",
                column: "ProductType");

            migrationBuilder.CreateIndex(
                name: "IX_WwpcSoftwares_Slug",
                table: "WwpcSoftwares",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_WwpcSoftwares_SourceUrl",
                table: "WwpcSoftwares",
                column: "SourceUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WwpcSoftwares_Status",
                table: "WwpcSoftwares",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WwpcSoftwares_SuggestedVendorCompanyId",
                table: "WwpcSoftwares",
                column: "SuggestedVendorCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_WwpcSoftwares_WwpcCategoryId",
                table: "WwpcSoftwares",
                column: "WwpcCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_WwpcVersions_WwpcSoftwareId",
                table: "WwpcVersions",
                column: "WwpcSoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_WwpcVersions_WwpcSoftwareId_MajorRelease_VersionString",
                table: "WwpcVersions",
                columns: new[] { "WwpcSoftwareId", "MajorRelease", "VersionString" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WwpcScreenshots");

            migrationBuilder.DropTable(
                name: "WwpcVersions");

            migrationBuilder.DropTable(
                name: "WwpcSoftwares");

            migrationBuilder.DropTable(
                name: "WwpcCategories");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwareCriticReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MobyNumericId",
                table: "MobyGamesImportStates",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MobyGamesReviewImportStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MobyGameId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProcessedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReviewsImported = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobyGamesReviewImportStates", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareCriticReviews",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    MagazineId = table.Column<long>(type: "bigint", nullable: false),
                    PlatformId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    NormalizedScore = table.Column<int>(type: "int", nullable: true),
                    OriginalScore = table.Column<float>(type: "float", nullable: true),
                    OriginalScoreMaximum = table.Column<float>(type: "float", nullable: true),
                    ReviewText = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReviewDatePrecision = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReviewUrl = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareCriticReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareCriticReviews_Magazines_MagazineId",
                        column: x => x.MagazineId,
                        principalTable: "Magazines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SoftwareCriticReviews_SoftwarePlatforms_PlatformId",
                        column: x => x.PlatformId,
                        principalTable: "SoftwarePlatforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SoftwareCriticReviews_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MobyGamesReviewImportStates_MobyGameId",
                table: "MobyGamesReviewImportStates",
                column: "MobyGameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MobyGamesReviewImportStates_Status",
                table: "MobyGamesReviewImportStates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareCriticReviews_MagazineId",
                table: "SoftwareCriticReviews",
                column: "MagazineId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareCriticReviews_PlatformId",
                table: "SoftwareCriticReviews",
                column: "PlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareCriticReviews_SoftwareId",
                table: "SoftwareCriticReviews",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareCriticReviews_SoftwareId_MagazineId_PlatformId",
                table: "SoftwareCriticReviews",
                columns: new[] { "SoftwareId", "MagazineId", "PlatformId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MobyGamesReviewImportStates");

            migrationBuilder.DropTable(
                name: "SoftwareCriticReviews");

            migrationBuilder.DropColumn(
                name: "MobyNumericId",
                table: "MobyGamesImportStates");
        }
    }
}

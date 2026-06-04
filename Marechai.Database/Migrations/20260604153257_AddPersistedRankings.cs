using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPersistedRankings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RankingDefinitions",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Dimension = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DimensionId = table.Column<long>(type: "bigint", nullable: true),
                    EntryCount = table.Column<int>(type: "int", nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingDefinitions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareScores",
                columns: table => new
                {
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Score = table.Column<double>(type: "double", nullable: false),
                    GlobalRank = table.Column<int>(type: "int", nullable: false),
                    CriticAverage = table.Column<double>(type: "double", nullable: true),
                    UserStarAverage = table.Column<double>(type: "double", nullable: true),
                    CriticReviewCount = table.Column<int>(type: "int", nullable: false),
                    UserRatingCount = table.Column<int>(type: "int", nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareScores", x => x.SoftwareId);
                    table.ForeignKey(
                        name: "FK_SoftwareScores_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RankingEntries",
                columns: table => new
                {
                    RankingDefinitionId = table.Column<uint>(type: "int unsigned", nullable: false),
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingEntries", x => new { x.RankingDefinitionId, x.SoftwareId });
                    table.ForeignKey(
                        name: "FK_RankingEntries_RankingDefinitions_RankingDefinitionId",
                        column: x => x.RankingDefinitionId,
                        principalTable: "RankingDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RankingEntries_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RankingDefinitions_Dimension_DimensionId",
                table: "RankingDefinitions",
                columns: new[] { "Dimension", "DimensionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_ranking_entries_definition_rank",
                table: "RankingEntries",
                columns: new[] { "RankingDefinitionId", "Rank" });

            migrationBuilder.CreateIndex(
                name: "idx_ranking_entries_software",
                table: "RankingEntries",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "idx_software_scores_global_rank",
                table: "SoftwareScores",
                column: "GlobalRank");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RankingEntries");

            migrationBuilder.DropTable(
                name: "SoftwareScores");

            migrationBuilder.DropTable(
                name: "RankingDefinitions");
        }
    }
}

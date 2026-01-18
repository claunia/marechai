using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddGpuBySoftwareRelease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MinimumGpuBySoftwareRelease",
                columns: table => new
                {
                    ReleaseId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    GpuId = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinimumGpuBySoftwareRelease", x => new { x.ReleaseId, x.GpuId });
                    table.ForeignKey(
                        name: "FK_MinimumGpuBySoftwareRelease_SoftwareReleases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "SoftwareReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MinimumGpuBySoftwareRelease_gpus_GpuId",
                        column: x => x.GpuId,
                        principalTable: "gpus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RecommendedGpuBySoftwareRelease",
                columns: table => new
                {
                    ReleaseId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    GpuId = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendedGpuBySoftwareRelease", x => new { x.ReleaseId, x.GpuId });
                    table.ForeignKey(
                        name: "FK_RecommendedGpuBySoftwareRelease_SoftwareReleases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "SoftwareReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecommendedGpuBySoftwareRelease_gpus_GpuId",
                        column: x => x.GpuId,
                        principalTable: "gpus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MinimumGpuBySoftwareRelease_GpuId",
                table: "MinimumGpuBySoftwareRelease",
                column: "GpuId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedGpuBySoftwareRelease_GpuId",
                table: "RecommendedGpuBySoftwareRelease",
                column: "GpuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MinimumGpuBySoftwareRelease");

            migrationBuilder.DropTable(
                name: "RecommendedGpuBySoftwareRelease");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsCompilationFromSoftwareReleases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safety net: any release that was flagged IsCompilation=1 but never picked up a
            // SoftwareCompilationId in the backfill migration (e.g. rows inserted between the two
            // migrations) has no owner at all once IsCompilation is dropped below — remove it
            // outright rather than leave a dangling, unreachable release row.
            migrationBuilder.Sql(
                """
                DELETE FROM SoftwareReleases WHERE IsCompilation = 1 AND SoftwareCompilationId IS NULL;
                """);

            migrationBuilder.DropColumn(
                name: "LegacyReleaseId",
                table: "SoftwareCompilations");

            migrationBuilder.DropTable(
                name: "SoftwareBySoftwareRelease");

            migrationBuilder.DropTable(
                name: "SoftwareVersionBySoftwareRelease");

            migrationBuilder.DropColumn(
                name: "IsCompilation",
                table: "SoftwareReleases");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "LegacyReleaseId",
                table: "SoftwareCompilations",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompilation",
                table: "SoftwareReleases",
                type: "bit(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SoftwareBySoftwareRelease",
                columns: table => new
                {
                    ReleaseId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareBySoftwareRelease", x => new { x.ReleaseId, x.SoftwareId });
                    table.ForeignKey(
                        name: "FK_SoftwareBySoftwareRelease_SoftwareReleases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "SoftwareReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareBySoftwareRelease_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareVersionBySoftwareRelease",
                columns: table => new
                {
                    ReleaseId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    SoftwareVersionId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareVersionBySoftwareRelease", x => new { x.ReleaseId, x.SoftwareVersionId });
                    table.ForeignKey(
                        name: "FK_SoftwareVersionBySoftwareRelease_SoftwareReleases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "SoftwareReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareVersionBySoftwareRelease_SoftwareVersions_SoftwareVe~",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareBySoftwareRelease_SoftwareId",
                table: "SoftwareBySoftwareRelease",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVersionBySoftwareRelease_SoftwareVersionId",
                table: "SoftwareVersionBySoftwareRelease",
                column: "SoftwareVersionId");
        }
    }
}

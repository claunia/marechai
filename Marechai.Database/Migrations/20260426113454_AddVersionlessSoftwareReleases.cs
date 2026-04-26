using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionlessSoftwareReleases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompilation",
                table: "SoftwareReleases",
                type: "bit(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<ulong>(
                name: "SoftwareId",
                table: "SoftwareReleases",
                type: "bigint unsigned",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareReleases_SoftwareId",
                table: "SoftwareReleases",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareBySoftwareRelease_SoftwareId",
                table: "SoftwareBySoftwareRelease",
                column: "SoftwareId");

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareReleases_Softwares_SoftwareId",
                table: "SoftwareReleases",
                column: "SoftwareId",
                principalTable: "Softwares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Data migration: populate SoftwareId for existing versioned releases
            migrationBuilder.Sql("""
                UPDATE SoftwareReleases sr
                INNER JOIN SoftwareVersions sv ON sr.SoftwareVersionId = sv.Id
                SET sr.SoftwareId = sv.SoftwareId
                WHERE sr.SoftwareVersionId IS NOT NULL;
                """);

            // Data migration: set IsCompilation flag for existing compilation releases
            migrationBuilder.Sql("""
                UPDATE SoftwareReleases
                SET IsCompilation = 1
                WHERE SoftwareVersionId IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareReleases_Softwares_SoftwareId",
                table: "SoftwareReleases");

            migrationBuilder.DropTable(
                name: "SoftwareBySoftwareRelease");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareReleases_SoftwareId",
                table: "SoftwareReleases");

            migrationBuilder.DropColumn(
                name: "IsCompilation",
                table: "SoftwareReleases");

            migrationBuilder.DropColumn(
                name: "SoftwareId",
                table: "SoftwareReleases");
        }
    }
}

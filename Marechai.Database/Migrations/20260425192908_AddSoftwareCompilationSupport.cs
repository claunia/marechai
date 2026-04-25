using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwareCompilationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareReleases_SoftwareVersions_SoftwareVersionId",
                table: "SoftwareReleases");

            migrationBuilder.AlterColumn<ulong>(
                name: "SoftwareVersionId",
                table: "SoftwareReleases",
                type: "bigint unsigned",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "SoftwareReleases",
                type: "longtext",
                nullable: true)
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
                name: "IX_SoftwareVersionBySoftwareRelease_SoftwareVersionId",
                table: "SoftwareVersionBySoftwareRelease",
                column: "SoftwareVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareReleases_SoftwareVersions_SoftwareVersionId",
                table: "SoftwareReleases",
                column: "SoftwareVersionId",
                principalTable: "SoftwareVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareReleases_SoftwareVersions_SoftwareVersionId",
                table: "SoftwareReleases");

            migrationBuilder.DropTable(
                name: "SoftwareVersionBySoftwareRelease");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "SoftwareReleases");

            migrationBuilder.AlterColumn<ulong>(
                name: "SoftwareVersionId",
                table: "SoftwareReleases",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareReleases_SoftwareVersions_SoftwareVersionId",
                table: "SoftwareReleases",
                column: "SoftwareVersionId",
                principalTable: "SoftwareVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

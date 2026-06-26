using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwareCompilations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Purely additive: the legacy IsCompilation column and SoftwareBySoftwareRelease /
            // SoftwareVersionBySoftwareRelease junction tables are left in place here so the
            // follow-up data-backfill migration can still read them. They are dropped in
            // RemoveIsCompilationFromSoftwareReleases, after the backfill is verified.
            migrationBuilder.AddColumn<ulong>(
                name: "SoftwareCompilationId",
                table: "SoftwareReleases",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "SoftwareCompilationId",
                table: "SoftwareCovers",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "SoftwareCompilationId",
                table: "MobyGamesImportStates",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SoftwareCompilations",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    MachineId = table.Column<int>(type: "int(11)", nullable: true),
                    PredecessorId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    RelationshipType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareCompilations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareCompilations_SoftwareCompilations_PredecessorId",
                        column: x => x.PredecessorId,
                        principalTable: "SoftwareCompilations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SoftwareCompilations_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SoftwareCompilations_machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "machines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareBySoftwareCompilation",
                columns: table => new
                {
                    SoftwareCompilationId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareBySoftwareCompilation", x => new { x.SoftwareCompilationId, x.SoftwareId });
                    table.ForeignKey(
                        name: "FK_SoftwareBySoftwareCompilation_SoftwareCompilations_SoftwareC~",
                        column: x => x.SoftwareCompilationId,
                        principalTable: "SoftwareCompilations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareBySoftwareCompilation_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareCompilationBySoftwareCompilation",
                columns: table => new
                {
                    ParentCompilationId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChildCompilationId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareCompilationBySoftwareCompilation", x => new { x.ParentCompilationId, x.ChildCompilationId });
                    table.ForeignKey(
                        name: "FK_SoftwareCompilationBySoftwareCompilation_SoftwareCompilation~",
                        column: x => x.ChildCompilationId,
                        principalTable: "SoftwareCompilations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SoftwareCompilationBySoftwareCompilation_SoftwareCompilatio~1",
                        column: x => x.ParentCompilationId,
                        principalTable: "SoftwareCompilations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareVersionBySoftwareCompilation",
                columns: table => new
                {
                    SoftwareCompilationId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    SoftwareVersionId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareVersionBySoftwareCompilation", x => new { x.SoftwareCompilationId, x.SoftwareVersionId });
                    table.ForeignKey(
                        name: "FK_SoftwareVersionBySoftwareCompilation_SoftwareCompilations_So~",
                        column: x => x.SoftwareCompilationId,
                        principalTable: "SoftwareCompilations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareVersionBySoftwareCompilation_SoftwareVersions_Softwa~",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareReleases_SoftwareCompilationId",
                table: "SoftwareReleases",
                column: "SoftwareCompilationId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareCovers_SoftwareCompilationId",
                table: "SoftwareCovers",
                column: "SoftwareCompilationId");

            migrationBuilder.CreateIndex(
                name: "IX_MobyGamesImportStates_SoftwareCompilationId",
                table: "MobyGamesImportStates",
                column: "SoftwareCompilationId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareBySoftwareCompilation_SoftwareId",
                table: "SoftwareBySoftwareCompilation",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareCompilationBySoftwareCompilation_ChildCompilationId",
                table: "SoftwareCompilationBySoftwareCompilation",
                column: "ChildCompilationId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareCompilations_MachineId",
                table: "SoftwareCompilations",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareCompilations_Name",
                table: "SoftwareCompilations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareCompilations_PredecessorId",
                table: "SoftwareCompilations",
                column: "PredecessorId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareCompilations_SoftwareId",
                table: "SoftwareCompilations",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVersionBySoftwareCompilation_SoftwareVersionId",
                table: "SoftwareVersionBySoftwareCompilation",
                column: "SoftwareVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MobyGamesImportStates_SoftwareCompilations_SoftwareCompilati~",
                table: "MobyGamesImportStates",
                column: "SoftwareCompilationId",
                principalTable: "SoftwareCompilations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareCovers_SoftwareCompilations_SoftwareCompilationId",
                table: "SoftwareCovers",
                column: "SoftwareCompilationId",
                principalTable: "SoftwareCompilations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareReleases_SoftwareCompilations_SoftwareCompilationId",
                table: "SoftwareReleases",
                column: "SoftwareCompilationId",
                principalTable: "SoftwareCompilations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MobyGamesImportStates_SoftwareCompilations_SoftwareCompilati~",
                table: "MobyGamesImportStates");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareCovers_SoftwareCompilations_SoftwareCompilationId",
                table: "SoftwareCovers");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareReleases_SoftwareCompilations_SoftwareCompilationId",
                table: "SoftwareReleases");

            migrationBuilder.DropTable(
                name: "SoftwareBySoftwareCompilation");

            migrationBuilder.DropTable(
                name: "SoftwareCompilationBySoftwareCompilation");

            migrationBuilder.DropTable(
                name: "SoftwareVersionBySoftwareCompilation");

            migrationBuilder.DropTable(
                name: "SoftwareCompilations");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareReleases_SoftwareCompilationId",
                table: "SoftwareReleases");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareCovers_SoftwareCompilationId",
                table: "SoftwareCovers");

            migrationBuilder.DropIndex(
                name: "IX_MobyGamesImportStates_SoftwareCompilationId",
                table: "MobyGamesImportStates");

            migrationBuilder.DropColumn(
                name: "SoftwareCompilationId",
                table: "SoftwareReleases");

            migrationBuilder.DropColumn(
                name: "SoftwareCompilationId",
                table: "SoftwareCovers");

            migrationBuilder.DropColumn(
                name: "SoftwareCompilationId",
                table: "MobyGamesImportStates");
        }
    }
}

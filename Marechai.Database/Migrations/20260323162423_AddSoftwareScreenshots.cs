using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwareScreenshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SoftwareScreenshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    SoftwarePlatformId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    SoftwareVersionId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    Caption = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalExtension = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareScreenshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareScreenshots_SoftwarePlatforms_SoftwarePlatformId",
                        column: x => x.SoftwarePlatformId,
                        principalTable: "SoftwarePlatforms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SoftwareScreenshots_SoftwareVersions_SoftwareVersionId",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SoftwareScreenshots_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareScreenshots_SoftwareId",
                table: "SoftwareScreenshots",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareScreenshots_SoftwarePlatformId",
                table: "SoftwareScreenshots",
                column: "SoftwarePlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareScreenshots_SoftwareVersionId",
                table: "SoftwareScreenshots",
                column: "SoftwareVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SoftwareScreenshots");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMobyGamesImportModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MobyGamesImportStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MobyGameId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProcessedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    BatchNumber = table.Column<int>(type: "int", nullable: false),
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobyGamesImportStates", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MobyGamesRejections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MobyGameId = table.Column<int>(type: "int", nullable: false),
                    GameName = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reason = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RejectedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReviewedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReviewAction = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobyGamesRejections", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PeopleBySoftware",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeopleBySoftware", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeopleBySoftware_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeopleBySoftware_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareAttributes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SoftwareReleaseId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Category = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Key = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareAttributes_SoftwareReleases_SoftwareReleaseId",
                        column: x => x.SoftwareReleaseId,
                        principalTable: "SoftwareReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareGenres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareGenres", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GenresBySoftware",
                columns: table => new
                {
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenresBySoftware", x => new { x.SoftwareId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_GenresBySoftware_SoftwareGenres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "SoftwareGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GenresBySoftware_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GenresBySoftware_GenreId",
                table: "GenresBySoftware",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_MobyGamesImportStates_MobyGameId",
                table: "MobyGamesImportStates",
                column: "MobyGameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MobyGamesImportStates_Status",
                table: "MobyGamesImportStates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MobyGamesRejections_MobyGameId",
                table: "MobyGamesRejections",
                column: "MobyGameId");

            migrationBuilder.CreateIndex(
                name: "IX_MobyGamesRejections_ReviewAction",
                table: "MobyGamesRejections",
                column: "ReviewAction");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleBySoftware_PersonId",
                table: "PeopleBySoftware",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleBySoftware_SoftwareId_PersonId_Role",
                table: "PeopleBySoftware",
                columns: new[] { "SoftwareId", "PersonId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareAttributes_SoftwareReleaseId_Category_Key",
                table: "SoftwareAttributes",
                columns: new[] { "SoftwareReleaseId", "Category", "Key" });

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareGenres_Name_Type",
                table: "SoftwareGenres",
                columns: new[] { "Name", "Type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenresBySoftware");

            migrationBuilder.DropTable(
                name: "MobyGamesImportStates");

            migrationBuilder.DropTable(
                name: "MobyGamesRejections");

            migrationBuilder.DropTable(
                name: "PeopleBySoftware");

            migrationBuilder.DropTable(
                name: "SoftwareAttributes");

            migrationBuilder.DropTable(
                name: "SoftwareGenres");
        }
    }
}

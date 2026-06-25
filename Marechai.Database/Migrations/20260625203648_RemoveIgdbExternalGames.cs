using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIgdbExternalGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IgdbExternalGames");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IgdbExternalGames",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GameIgdbId = table.Column<long>(type: "bigint", nullable: false),
                    IgdbId = table.Column<long>(type: "bigint", nullable: false),
                    Uid = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgdbExternalGames", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbExternalGames_GameIgdbId",
                table: "IgdbExternalGames",
                column: "GameIgdbId");

            migrationBuilder.CreateIndex(
                name: "IX_IgdbExternalGames_IgdbId",
                table: "IgdbExternalGames",
                column: "IgdbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IgdbExternalGames_Uid",
                table: "IgdbExternalGames",
                column: "Uid");
        }
    }
}

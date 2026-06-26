using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddIgdbGameEnrichmentTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EnrichedOn",
                table: "IgdbGames",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnrichmentApplied",
                table: "IgdbGames",
                type: "bit(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ScreenshotImageIdsJson",
                table: "IgdbGames",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnrichedOn",
                table: "IgdbGames");

            migrationBuilder.DropColumn(
                name: "EnrichmentApplied",
                table: "IgdbGames");

            migrationBuilder.DropColumn(
                name: "ScreenshotImageIdsJson",
                table: "IgdbGames");
        }
    }
}

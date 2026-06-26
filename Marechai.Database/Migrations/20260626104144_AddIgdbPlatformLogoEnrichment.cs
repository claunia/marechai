using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddIgdbPlatformLogoEnrichment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EnrichedOn",
                table: "IgdbPlatforms",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnrichmentApplied",
                table: "IgdbPlatforms",
                type: "bit(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LogoImageId",
                table: "IgdbPlatforms",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnrichedOn",
                table: "IgdbPlatforms");

            migrationBuilder.DropColumn(
                name: "EnrichmentApplied",
                table: "IgdbPlatforms");

            migrationBuilder.DropColumn(
                name: "LogoImageId",
                table: "IgdbPlatforms");
        }
    }
}

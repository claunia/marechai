using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class GrayscaleResolutions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.AddColumn<bool>("Grayscale", "resolutions", nullable: false, defaultValue: false);

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropColumn("Grayscale", "resolutions");
    }
}
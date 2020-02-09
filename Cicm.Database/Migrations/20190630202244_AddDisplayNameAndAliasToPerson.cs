using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class AddDisplayNameAndAliasToPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>("Alias", "People", nullable: true);

            migrationBuilder.AddColumn<string>("DisplayName", "People", nullable: true);

            migrationBuilder.CreateIndex("IX_People_Alias", "People", "Alias");

            migrationBuilder.CreateIndex("IX_People_DisplayName", "People", "DisplayName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_People_Alias", "People");

            migrationBuilder.DropIndex("IX_People_DisplayName", "People");

            migrationBuilder.DropColumn("Alias", "People");

            migrationBuilder.DropColumn("DisplayName", "People");
        }
    }
}
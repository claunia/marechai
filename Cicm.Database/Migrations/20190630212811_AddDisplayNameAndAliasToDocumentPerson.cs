using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class AddDisplayNameAndAliasToDocumentPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>("Alias", "DocumentPeople", nullable: true);

            migrationBuilder.AddColumn<string>("DisplayName", "DocumentPeople", nullable: true);

            migrationBuilder.CreateIndex("IX_DocumentPeople_Alias", "DocumentPeople", "Alias");

            migrationBuilder.CreateIndex("IX_DocumentPeople_DisplayName", "DocumentPeople", "DisplayName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_DocumentPeople_Alias", "DocumentPeople");

            migrationBuilder.DropIndex("IX_DocumentPeople_DisplayName", "DocumentPeople");

            migrationBuilder.DropColumn("Alias", "DocumentPeople");

            migrationBuilder.DropColumn("DisplayName", "DocumentPeople");
        }
    }
}
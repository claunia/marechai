using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddDocumentPeople : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>("DocumentPersonId", "People", nullable: true);

            migrationBuilder.CreateTable("DocumentPeople", table => new
            {
                Id = table.Column<int>().
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name     = table.Column<string>(), Surname = table.Column<string>(),
                PersonId = table.Column<int>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_DocumentPeople", x => x.Id);

                table.ForeignKey("FK_DocumentPeople_People_PersonId", x => x.PersonId, "People", "Id",
                                 onDelete: ReferentialAction.SetNull);
            });

            migrationBuilder.CreateIndex("IX_DocumentPeople_Name", "DocumentPeople", "Name");

            migrationBuilder.CreateIndex("IX_DocumentPeople_PersonId", "DocumentPeople", "PersonId", unique: true);

            migrationBuilder.CreateIndex("IX_DocumentPeople_Surname", "DocumentPeople", "Surname");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("DocumentPeople");

            migrationBuilder.DropColumn("DocumentPersonId", "People");
        }
    }
}
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddPeopleByMagazine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("PeopleByMagazines", table => new
            {
                Id = table.Column<long>().
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                PersonId = table.Column<int>(), MagazineId = table.Column<long>(),
                RoleId   = table.Column<string>("char(3)")
            }, constraints: table =>
            {
                table.PrimaryKey("PK_PeopleByMagazines", x => x.Id);

                table.ForeignKey("FK_PeopleByMagazines_MagazineIssues_MagazineId", x => x.MagazineId, "MagazineIssues",
                                 "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_PeopleByMagazines_DocumentPeople_PersonId", x => x.PersonId, "DocumentPeople",
                                 "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_PeopleByMagazines_DocumentRoles_RoleId", x => x.RoleId, "DocumentRoles", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_PeopleByMagazines_MagazineId", "PeopleByMagazines", "MagazineId");

            migrationBuilder.CreateIndex("IX_PeopleByMagazines_PersonId", "PeopleByMagazines", "PersonId");

            migrationBuilder.CreateIndex("IX_PeopleByMagazines_RoleId", "PeopleByMagazines", "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable("PeopleByMagazines");
    }
}
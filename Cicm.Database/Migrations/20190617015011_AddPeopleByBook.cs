using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddPeopleByBook : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("PeopleByBooks",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             PersonId = table.Column<int>(),
                                             BookId   = table.Column<long>(),
                                             RoleId   = table.Column<string>("char(3)")
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_PeopleByBooks", x => x.Id);
                                             table.ForeignKey("FK_PeopleByBooks_Books_BookId", x => x.BookId, "Books",
                                                              "Id", onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_PeopleByBooks_DocumentPeople_PersonId",
                                                              x => x.PersonId, "DocumentPeople", "Id",
                                                              onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_PeopleByBooks_DocumentRoles_RoleId", x => x.RoleId,
                                                              "DocumentRoles", "Id",
                                                              onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateIndex("IX_PeopleByBooks_BookId", "PeopleByBooks", "BookId");

            migrationBuilder.CreateIndex("IX_PeopleByBooks_PersonId", "PeopleByBooks", "PersonId");

            migrationBuilder.CreateIndex("IX_PeopleByBooks_RoleId", "PeopleByBooks", "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("PeopleByBooks");
        }
    }
}
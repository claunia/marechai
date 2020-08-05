using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddPeopleByDocument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("PeopleByDocuments", table => new
            {
                Id = table.Column<long>().
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                PersonId   = table.Column<int>(),
                DocumentId = table.Column<long>(),
                RoleId     = table.Column<string>("char(3)")
            }, constraints: table =>
            {
                table.PrimaryKey("PK_PeopleByDocuments", x => x.Id);

                table.ForeignKey("FK_PeopleByDocuments_Documents_DocumentId", x => x.DocumentId, "Documents", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_PeopleByDocuments_DocumentPeople_PersonId", x => x.PersonId, "DocumentPeople",
                                 "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_PeopleByDocuments_DocumentRoles_RoleId", x => x.RoleId, "DocumentRoles", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_PeopleByDocuments_DocumentId", "PeopleByDocuments", "DocumentId");

            migrationBuilder.CreateIndex("IX_PeopleByDocuments_PersonId", "PeopleByDocuments", "PersonId");

            migrationBuilder.CreateIndex("IX_PeopleByDocuments_RoleId", "PeopleByDocuments", "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable("PeopleByDocuments");
    }
}
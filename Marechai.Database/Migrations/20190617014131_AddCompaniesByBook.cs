using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddCompaniesByBook : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("CompaniesByBooks", table => new
            {
                Id = table.Column<long>().
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CompanyId = table.Column<int>(),
                BookId    = table.Column<long>(),
                RoleId    = table.Column<string>("char(3)")
            }, constraints: table =>
            {
                table.PrimaryKey("PK_CompaniesByBooks", x => x.Id);

                table.ForeignKey("FK_CompaniesByBooks_Books_BookId", x => x.BookId, "Books", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_CompaniesByBooks_DocumentCompanies_CompanyId", x => x.CompanyId,
                                 "DocumentCompanies", "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_CompaniesByBooks_DocumentRoles_RoleId", x => x.RoleId, "DocumentRoles", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_CompaniesByBooks_BookId", "CompaniesByBooks", "BookId");

            migrationBuilder.CreateIndex("IX_CompaniesByBooks_CompanyId", "CompaniesByBooks", "CompanyId");

            migrationBuilder.CreateIndex("IX_CompaniesByBooks_RoleId", "CompaniesByBooks", "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable("CompaniesByBooks");
    }
}
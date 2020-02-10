using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddCompaniesByMagazine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("CompaniesByMagazines", table => new
            {
                Id = table.Column<long>().
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CompanyId = table.Column<int>(), MagazineId = table.Column<long>(),
                RoleId    = table.Column<string>("char(3)")
            }, constraints: table =>
            {
                table.PrimaryKey("PK_CompaniesByMagazines", x => x.Id);

                table.ForeignKey("FK_CompaniesByMagazines_DocumentCompanies_CompanyId", x => x.CompanyId,
                                 "DocumentCompanies", "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_CompaniesByMagazines_Magazines_MagazineId", x => x.MagazineId, "Magazines", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_CompaniesByMagazines_DocumentRoles_RoleId", x => x.RoleId, "DocumentRoles", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_CompaniesByMagazines_CompanyId", "CompaniesByMagazines", "CompanyId");

            migrationBuilder.CreateIndex("IX_CompaniesByMagazines_MagazineId", "CompaniesByMagazines", "MagazineId");

            migrationBuilder.CreateIndex("IX_CompaniesByMagazines_RoleId", "CompaniesByMagazines", "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable("CompaniesByMagazines");
    }
}
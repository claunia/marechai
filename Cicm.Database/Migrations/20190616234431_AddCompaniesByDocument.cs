using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class AddCompaniesByDocument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("CompaniesByDocuments",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             CompanyId  = table.Column<int>(),
                                             DocumentId = table.Column<long>(),
                                             RoleId     = table.Column<string>("char(3)")
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_CompaniesByDocuments", x => x.Id);
                                             table.ForeignKey("FK_CompaniesByDocuments_DocumentCompanies_CompanyId",
                                                              x => x.CompanyId, "DocumentCompanies", "Id",
                                                              onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_CompaniesByDocuments_Documents_DocumentId",
                                                              x => x.DocumentId, "Documents", "Id",
                                                              onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_CompaniesByDocuments_DocumentRoles_RoleId",
                                                              x => x.RoleId, "DocumentRoles", "Id",
                                                              onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateIndex("IX_CompaniesByDocuments_CompanyId", "CompaniesByDocuments", "CompanyId");

            migrationBuilder.CreateIndex("IX_CompaniesByDocuments_DocumentId", "CompaniesByDocuments", "DocumentId");

            migrationBuilder.CreateIndex("IX_CompaniesByDocuments_RoleId", "CompaniesByDocuments", "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("CompaniesByDocuments");
        }
    }
}
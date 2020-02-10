using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddDocumentCompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>("DocumentCompanyId", "companies", nullable: true);

            migrationBuilder.CreateTable("DocumentCompanies",
                                         table => new
                                         {
                                             Id = table.Column<int>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             Name      = table.Column<string>(),
                                             CompanyId = table.Column<int>(nullable: true)
                                         },
                                         constraints: table =>
                                         {
                                             table.PrimaryKey("PK_DocumentCompanies", x => x.Id);
                                         });

            migrationBuilder.CreateIndex("IX_companies_DocumentCompanyId", "companies", "DocumentCompanyId",
                                         unique: true);

            migrationBuilder.CreateIndex("IX_DocumentCompanies_CompanyId", "DocumentCompanies", "CompanyId",
                                         unique: true);

            migrationBuilder.CreateIndex("IX_DocumentCompanies_Name", "DocumentCompanies", "Name");

            migrationBuilder.AddForeignKey("FK_companies_DocumentCompanies_DocumentCompanyId", "companies",
                                           "DocumentCompanyId", "DocumentCompanies", principalColumn: "Id",
                                           onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_companies_DocumentCompanies_DocumentCompanyId", "companies");

            migrationBuilder.DropTable("DocumentCompanies");

            migrationBuilder.DropIndex("IX_companies_DocumentCompanyId", "companies");

            migrationBuilder.DropColumn("DocumentCompanyId", "companies");
        }
    }
}
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class FixDocumentCompanyOneToOne : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_companies_DocumentCompanies_DocumentCompanyId", "companies");

            migrationBuilder.DropIndex("IX_companies_DocumentCompanyId", "companies");

            migrationBuilder.AlterColumn<int>("CompanyId", "DocumentCompanies", nullable: true, oldClrType: typeof(int),
                                              oldNullable: true);

            migrationBuilder.AddForeignKey("FK_DocumentCompanies_companies_CompanyId", "DocumentCompanies", "CompanyId",
                                           "companies", principalColumn: "id", onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_DocumentCompanies_companies_CompanyId", "DocumentCompanies");

            migrationBuilder.AlterColumn<int>("CompanyId", "DocumentCompanies", nullable: true, oldClrType: typeof(int),
                                              oldNullable: true);

            migrationBuilder.CreateIndex("IX_companies_DocumentCompanyId", "companies", "DocumentCompanyId",
                                         unique: true);

            migrationBuilder.AddForeignKey("FK_companies_DocumentCompanies_DocumentCompanyId", "companies",
                                           "DocumentCompanyId", "DocumentCompanies", principalColumn: "Id",
                                           onDelete: ReferentialAction.SetNull);
        }
    }
}
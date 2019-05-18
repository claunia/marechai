using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class AddCompanyDescriptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("CompanyDescriptions",
                                         table => new
                                         {
                                             Id = table.Column<int>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             CompanyId = table.Column<int>(),
                                             Text      = table.Column<string>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_CompanyDescriptions", x => x.Id);
                                             table.ForeignKey("FK_CompanyDescriptions_companies_CompanyId",
                                                              x => x.CompanyId, "companies", "id",
                                                              onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateIndex("IX_CompanyDescriptions_CompanyId", "CompanyDescriptions", "CompanyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("CompanyDescriptions");
        }
    }
}
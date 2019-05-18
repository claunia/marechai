using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class AddCompanyDescriptionIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>("Text", "CompanyDescriptions", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.CreateIndex("IX_CompanyDescriptions_Text", "CompanyDescriptions", "Text")
                            .Annotation("MySql:FullTextIndex", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_CompanyDescriptions_Text", "CompanyDescriptions");

            migrationBuilder.AlterColumn<string>("Text", "CompanyDescriptions", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);
        }
    }
}
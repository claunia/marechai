using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AnnotateCompanyDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>("Text", "CompanyDescriptions", maxLength: 262144, nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>("Text", "CompanyDescriptions", nullable: true,
                                                 oldClrType: typeof(string), oldMaxLength: 262144, oldNullable: true);
        }
    }
}
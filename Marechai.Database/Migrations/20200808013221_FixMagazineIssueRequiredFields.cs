using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class FixMagazineIssueRequiredFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.AlterColumn<short>("Pages", "MagazineIssues", nullable: true, oldClrType: typeof(short),
                                                oldType: "smallint");

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.AlterColumn<short>("Pages", "MagazineIssues", "smallint", nullable: false,
                                                oldClrType: typeof(short), oldNullable: true);
    }
}
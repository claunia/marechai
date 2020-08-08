using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class FixMediaRequiredFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.AlterColumn<string>("Title", "Media", nullable: false, oldClrType: typeof(string),
                                                 oldType: "varchar(255) CHARACTER SET utf8mb4", oldNullable: true);

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.AlterColumn<string>("Title", "Media", "varchar(255) CHARACTER SET utf8mb4", nullable: true,
                                                 oldClrType: typeof(string));
    }
}
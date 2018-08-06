using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class RenameModelFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("idx_company_id", "company_descriptions");

            migrationBuilder.CreateIndex("idx_company_id", "company_descriptions", "company_id", unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("idx_company_id", "company_descriptions");

            migrationBuilder.CreateIndex("idx_company_id", "company_descriptions", "company_id");
        }
    }
}
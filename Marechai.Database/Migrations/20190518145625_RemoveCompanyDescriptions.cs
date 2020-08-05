using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class RemoveCompanyDescriptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable("company_descriptions");

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("company_descriptions", table => new
            {
                id = table.Column<int>("int(11)").
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                company_id = table.Column<int>("int(11)"),
                text       = table.Column<string>("text", nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_company_descriptions", x => x.id);
                table.ForeignKey("fk_company_id", x => x.id, "companies", "id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("idx_company_id", "company_descriptions", "company_id", unique: true);

            migrationBuilder.CreateIndex("idx_text", "company_descriptions", "text");
        }
    }
}
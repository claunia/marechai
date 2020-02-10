using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddIso639 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("ISO_639-3", table => new
            {
                Id      = table.Column<string>("char(3)"), Part2B = table.Column<string>("char(3)", nullable: true),
                Part2T  = table.Column<string>("char(3)", nullable: true),
                Part1   = table.Column<string>("char(2)", nullable: true), Scope = table.Column<string>("char(1)"),
                Type    = table.Column<string>("char(1)"), Ref_Name              = table.Column<string>("varchar(150)"),
                Comment = table.Column<string>("varchar(150)", nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_ISO_639-3", x => x.Id);
            });

            migrationBuilder.CreateIndex("IX_ISO_639-3_Comment", "ISO_639-3", "Comment");

            migrationBuilder.CreateIndex("IX_ISO_639-3_Part1", "ISO_639-3", "Part1");

            migrationBuilder.CreateIndex("IX_ISO_639-3_Part2B", "ISO_639-3", "Part2B");

            migrationBuilder.CreateIndex("IX_ISO_639-3_Part2T", "ISO_639-3", "Part2T");

            migrationBuilder.CreateIndex("IX_ISO_639-3_Ref_Name", "ISO_639-3", "Ref_Name");

            migrationBuilder.CreateIndex("IX_ISO_639-3_Scope", "ISO_639-3", "Scope");

            migrationBuilder.CreateIndex("IX_ISO_639-3_Type", "ISO_639-3", "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("ISO_639-3");
    }
}
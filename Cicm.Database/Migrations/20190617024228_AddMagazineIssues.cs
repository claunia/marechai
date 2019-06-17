using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class AddMagazineIssues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("MagazineIssues",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             MagazineId    = table.Column<long>(),
                                             Caption       = table.Column<string>(),
                                             NativeCaption = table.Column<string>(nullable: true),
                                             Published     = table.Column<DateTime>(nullable: true),
                                             ProductCode   = table.Column<string>(maxLength: 18, nullable: true),
                                             Pages         = table.Column<short>()
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_MagazineIssues", x => x.Id);
                                             table.ForeignKey("FK_MagazineIssues_Magazines_MagazineId",
                                                              x => x.MagazineId, "Magazines", "Id",
                                                              onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateIndex("IX_MagazineIssues_Caption", "MagazineIssues", "Caption");

            migrationBuilder.CreateIndex("IX_MagazineIssues_MagazineId", "MagazineIssues", "MagazineId");

            migrationBuilder.CreateIndex("IX_MagazineIssues_NativeCaption", "MagazineIssues", "NativeCaption");

            migrationBuilder.CreateIndex("IX_MagazineIssues_Pages", "MagazineIssues", "Pages");

            migrationBuilder.CreateIndex("IX_MagazineIssues_ProductCode", "MagazineIssues", "ProductCode");

            migrationBuilder.CreateIndex("IX_MagazineIssues_Published", "MagazineIssues", "Published");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("MagazineIssues");
        }
    }
}
using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class AddMagazines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Magazines",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             Title            = table.Column<string>(),
                                             NativeTitle      = table.Column<string>(nullable: true),
                                             Published        = table.Column<DateTime>(nullable: true),
                                             CountryId        = table.Column<short>(nullable: true),
                                             Synopsis         = table.Column<string>(maxLength: 262144, nullable: true),
                                             Issn             = table.Column<string>(maxLength: 8,      nullable: true),
                                             FirstPublication = table.Column<DateTime>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_Magazines", x => x.Id);
                                             table.ForeignKey("FK_Magazines_iso3166_1_numeric_CountryId",
                                                              x => x.CountryId, "iso3166_1_numeric", "id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateIndex("IX_Magazines_CountryId", "Magazines", "CountryId");

            migrationBuilder.CreateIndex("IX_Magazines_FirstPublication", "Magazines", "FirstPublication");

            migrationBuilder.CreateIndex("IX_Magazines_Issn", "Magazines", "Issn");

            migrationBuilder.CreateIndex("IX_Magazines_NativeTitle", "Magazines", "NativeTitle");

            migrationBuilder.CreateIndex("IX_Magazines_Published", "Magazines", "Published");

            migrationBuilder.CreateIndex("IX_Magazines_Synopsis", "Magazines", "Synopsis")
                            .Annotation("MySql:FullTextIndex", true);

            migrationBuilder.CreateIndex("IX_Magazines_Title", "Magazines", "Title");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Magazines");
        }
    }
}
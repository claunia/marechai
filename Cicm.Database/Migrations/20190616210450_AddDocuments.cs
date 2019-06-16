using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class AddDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Documents",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             Title       = table.Column<string>(),
                                             NativeTitle = table.Column<string>(nullable: true),
                                             Published   = table.Column<DateTime>(nullable: true),
                                             CountryId   = table.Column<short>(nullable: true),
                                             Synopsis    = table.Column<string>(maxLength: 262144, nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_Documents", x => x.Id);
                                             table.ForeignKey("FK_Documents_iso3166_1_numeric_CountryId",
                                                              x => x.CountryId, "iso3166_1_numeric", "id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateIndex("IX_Documents_CountryId", "Documents", "CountryId");

            migrationBuilder.CreateIndex("IX_Documents_NativeTitle", "Documents", "NativeTitle");

            migrationBuilder.CreateIndex("IX_Documents_Published", "Documents", "Published");

            migrationBuilder.CreateIndex("IX_Documents_Synopsis", "Documents", "Synopsis")
                            .Annotation("MySql:FullTextIndex", true);

            migrationBuilder.CreateIndex("IX_Documents_Title", "Documents", "Title");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Documents");
        }
    }
}
using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddBooks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Books", table => new
            {
                Id = table.Column<long>().
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Title      = table.Column<string>(), NativeTitle               = table.Column<string>(nullable: true),
                Published  = table.Column<DateTime>(nullable: true), CountryId = table.Column<short>(nullable: true),
                Synopsis   = table.Column<string>(maxLength: 262144, nullable: true),
                Isbn       = table.Column<string>(maxLength: 13, nullable: true),
                Pages      = table.Column<short>(nullable: true),
                Edition    = table.Column<int>(nullable: true),
                PreviousId = table.Column<long>(nullable: true),
                SourceId   = table.Column<long>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Books", x => x.Id);

                table.ForeignKey("FK_Books_iso3166_1_numeric_CountryId", x => x.CountryId, "iso3166_1_numeric", "id",
                                 onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_Books_Books_PreviousId", x => x.PreviousId, "Books", "Id",
                                 onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_Books_Books_SourceId", x => x.SourceId, "Books", "Id",
                                 onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateIndex("IX_Books_CountryId", "Books", "CountryId");

            migrationBuilder.CreateIndex("IX_Books_Edition", "Books", "Edition");

            migrationBuilder.CreateIndex("IX_Books_Isbn", "Books", "Isbn");

            migrationBuilder.CreateIndex("IX_Books_NativeTitle", "Books", "NativeTitle");

            migrationBuilder.CreateIndex("IX_Books_Pages", "Books", "Pages");

            migrationBuilder.CreateIndex("IX_Books_PreviousId", "Books", "PreviousId", unique: true);

            migrationBuilder.CreateIndex("IX_Books_Published", "Books", "Published");

            migrationBuilder.CreateIndex("IX_Books_SourceId", "Books", "SourceId");

            migrationBuilder.CreateIndex("IX_Books_Synopsis", "Books", "Synopsis").
                             Annotation("MySql:FullTextIndex", true);

            migrationBuilder.CreateIndex("IX_Books_Title", "Books", "Title");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("Books");
    }
}
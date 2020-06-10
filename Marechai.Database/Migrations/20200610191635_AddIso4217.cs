using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddIso4217 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Iso4217", table => new
            {
                Code       = table.Column<string>(maxLength: 3, nullable: false),
                Numeric    = table.Column<short>("smallint(3)", nullable: false),
                MinorUnits = table.Column<byte>(nullable: true),
                Name       = table.Column<string>(maxLength: 150, nullable: false),
                Withdrawn  = table.Column<DateTime>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Iso4217", x => x.Code);
            });

            migrationBuilder.CreateIndex("IX_Iso4217_Numeric", "Iso4217", "Numeric");

            migrationBuilder.CreateIndex("IX_Iso4217_Withdrawn", "Iso4217", "Withdrawn");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("Iso4217");
    }
}
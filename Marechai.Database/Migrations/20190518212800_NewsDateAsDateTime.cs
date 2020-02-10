using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class NewsDateAsDateTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>("date", "news", "datetime", nullable: false,
                                                   oldClrType: typeof(string), oldType: "char(20)",
                                                   oldDefaultValueSql: "''");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>("date", "news", "char(20)", nullable: false, defaultValueSql: "''",
                                                 oldClrType: typeof(DateTime), oldType: "datetime");
        }
    }
}
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsNameAndLongAddedId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "added_id",
                table: "news",
                type: "bigint(20)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "news",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name",
                table: "news");

            migrationBuilder.AlterColumn<int>(
                name: "added_id",
                table: "news",
                type: "int(11)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(long),
                oldType: "bigint(20)",
                oldDefaultValueSql: "'0'");
        }
    }
}

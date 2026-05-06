using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceSoftwareBoolsWithKindEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add the new Kind column (default 0 = Software)
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "Softwares",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 2. Migrate data: Game wins over OperatingSystem if both are true
            migrationBuilder.Sql("UPDATE `Softwares` SET `Kind` = 1 WHERE `IsOperatingSystem` = 1;");
            migrationBuilder.Sql("UPDATE `Softwares` SET `Kind` = 2 WHERE `IsGame` = 1;");

            // 3. Drop old boolean columns
            migrationBuilder.DropColumn(
                name: "IsGame",
                table: "Softwares");

            migrationBuilder.DropColumn(
                name: "IsOperatingSystem",
                table: "Softwares");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Re-add boolean columns
            migrationBuilder.AddColumn<bool>(
                name: "IsGame",
                table: "Softwares",
                type: "bit(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOperatingSystem",
                table: "Softwares",
                type: "bit(1)",
                nullable: false,
                defaultValue: false);

            // 2. Migrate data back
            migrationBuilder.Sql("UPDATE `Softwares` SET `IsOperatingSystem` = 1 WHERE `Kind` = 1;");
            migrationBuilder.Sql("UPDATE `Softwares` SET `IsGame` = 1 WHERE `Kind` = 2;");

            // 3. Drop Kind column
            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Softwares");
        }
    }
}

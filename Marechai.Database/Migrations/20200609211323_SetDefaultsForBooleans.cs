using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class SetDefaultsForBooleans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>("SerialNumberVisible", "OwnedMachines", nullable: false,
                                               defaultValue: true, oldClrType: typeof(bool), oldType: "bit(1)",
                                               oldDefaultValue: 1ul);

            migrationBuilder.AlterColumn<bool>("Enabled", "DocumentRoles", nullable: false, defaultValue: true,
                                               oldClrType: typeof(bool), oldType: "bit(1)", oldDefaultValue: 1ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>("SerialNumberVisible", "OwnedMachines", "bit(1)", nullable: false,
                                               defaultValue: 1ul, oldClrType: typeof(bool), oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>("Enabled", "DocumentRoles", "bit(1)", nullable: false, defaultValue: 1ul,
                                               oldClrType: typeof(bool), oldDefaultValue: true);
        }
    }
}
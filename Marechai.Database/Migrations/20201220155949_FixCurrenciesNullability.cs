using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class FixCurrenciesNullability : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>("SourceCode", "CurrenciesPegging", "varchar(3) CHARACTER SET utf8mb4",
                                                 nullable: false, defaultValue: "", oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4", oldNullable: true);

            migrationBuilder.AlterColumn<string>("DestinationCode", "CurrenciesPegging",
                                                 "varchar(3) CHARACTER SET utf8mb4", nullable: false, defaultValue: "",
                                                 oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4", oldNullable: true);

            migrationBuilder.AlterColumn<string>("CurrencyCode", "CurrenciesInflation",
                                                 "varchar(3) CHARACTER SET utf8mb4", nullable: false, defaultValue: "",
                                                 oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4", oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>("SourceCode", "CurrenciesPegging", "varchar(3) CHARACTER SET utf8mb4",
                                                 nullable: true, oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4");

            migrationBuilder.AlterColumn<string>("DestinationCode", "CurrenciesPegging",
                                                 "varchar(3) CHARACTER SET utf8mb4", nullable: true,
                                                 oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4");

            migrationBuilder.AlterColumn<string>("CurrencyCode", "CurrenciesInflation",
                                                 "varchar(3) CHARACTER SET utf8mb4", nullable: true,
                                                 oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4");
        }
    }
}
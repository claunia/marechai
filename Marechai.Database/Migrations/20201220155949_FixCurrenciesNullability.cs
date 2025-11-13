using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class FixCurrenciesNullability : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MySQL/MariaDB: Disable foreign key checks to allow column modifications
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS=0");

            // Ensure the Iso4217 table has the proper charset
            migrationBuilder.Sql("ALTER TABLE `Iso4217` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci");

            // Modify columns to NOT NULL with defaults
            migrationBuilder.AlterColumn<string>("SourceCode", "CurrenciesPegging", "varchar(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci",
                                                 nullable: false, defaultValue: "", oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4", oldNullable: true);

            migrationBuilder.AlterColumn<string>("DestinationCode", "CurrenciesPegging",
                                                 "varchar(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci", nullable: false, defaultValue: "",
                                                 oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4", oldNullable: true);

            migrationBuilder.AlterColumn<string>("CurrencyCode", "CurrenciesInflation",
                                                 "varchar(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci", nullable: false, defaultValue: "",
                                                 oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4", oldNullable: true);

            // Recreate foreign key constraints now that columns are NOT NULL
            migrationBuilder.AddForeignKey(
                name: "FK_CurrenciesPegging_Iso4217_SourceCode",
                table: "CurrenciesPegging",
                column: "SourceCode",
                principalTable: "Iso4217",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CurrenciesPegging_Iso4217_DestinationCode",
                table: "CurrenciesPegging",
                column: "DestinationCode",
                principalTable: "Iso4217",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CurrenciesInflation_Iso4217_CurrencyCode",
                table: "CurrenciesInflation",
                column: "CurrencyCode",
                principalTable: "Iso4217",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            // Re-enable foreign key checks
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS=1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // MySQL/MariaDB: Disable foreign key checks to allow column modifications
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS=0");

            // Drop foreign key constraints before reverting columns to nullable
            migrationBuilder.DropForeignKey(
                name: "FK_CurrenciesPegging_Iso4217_SourceCode",
                table: "CurrenciesPegging");

            migrationBuilder.DropForeignKey(
                name: "FK_CurrenciesPegging_Iso4217_DestinationCode",
                table: "CurrenciesPegging");

            migrationBuilder.DropForeignKey(
                name: "FK_CurrenciesInflation_Iso4217_CurrencyCode",
                table: "CurrenciesInflation");

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

            // Re-enable foreign key checks
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS=1");
        }
    }
}
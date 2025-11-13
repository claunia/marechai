using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class FixDumpRequiredFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>("MediaId", "Dumps", nullable: false, oldClrType: typeof(ulong),
                                                oldType: "bigint unsigned", oldNullable: true);

            migrationBuilder.AlterColumn<ulong>("MediaDumpId", "Dumps", nullable: false, oldClrType: typeof(ulong),
                                                oldType: "bigint unsigned", oldNullable: true);

            migrationBuilder.AlterColumn<string>("Dumper", "Dumps", nullable: false, oldClrType: typeof(string),
                                                 oldType: "varchar(255) CHARACTER SET utf8mb4", oldNullable: true);

            // MySQL/MariaDB: Disable foreign key checks to allow column modifications
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS=0");

            // Drop foreign key constraints only if they exist before modifying columns to make them nullable
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
                    WHERE CONSTRAINT_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'CurrenciesPegging'
                    AND CONSTRAINT_NAME = 'FK_CurrenciesPegging_Iso4217_SourceCode'
                )
                THEN
                    ALTER TABLE `CurrenciesPegging` DROP FOREIGN KEY `FK_CurrenciesPegging_Iso4217_SourceCode`;
                END IF;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
                    WHERE CONSTRAINT_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'CurrenciesPegging'
                    AND CONSTRAINT_NAME = 'FK_CurrenciesPegging_Iso4217_DestinationCode'
                )
                THEN
                    ALTER TABLE `CurrenciesPegging` DROP FOREIGN KEY `FK_CurrenciesPegging_Iso4217_DestinationCode`;
                END IF;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
                    WHERE CONSTRAINT_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'CurrenciesInflation'
                    AND CONSTRAINT_NAME = 'FK_CurrenciesInflation_Iso4217_CurrencyCode'
                )
                THEN
                    ALTER TABLE `CurrenciesInflation` DROP FOREIGN KEY `FK_CurrenciesInflation_Iso4217_CurrencyCode`;
                END IF;
            ");

            migrationBuilder.AlterColumn<string>("SourceCode", "CurrenciesPegging", nullable: true,
                                                 oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4");

            migrationBuilder.AlterColumn<string>("DestinationCode", "CurrenciesPegging", nullable: true,
                                                 oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4");

            migrationBuilder.AlterColumn<string>("CurrencyCode", "CurrenciesInflation", nullable: true,
                                                 oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4");

            // Re-enable foreign key checks
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS=1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>("MediaId", "Dumps", "bigint unsigned", nullable: true,
                                                oldClrType: typeof(ulong));

            migrationBuilder.AlterColumn<ulong>("MediaDumpId", "Dumps", "bigint unsigned", nullable: true,
                                                oldClrType: typeof(ulong));

            migrationBuilder.AlterColumn<string>("Dumper", "Dumps", "varchar(255) CHARACTER SET utf8mb4",
                                                 nullable: true, oldClrType: typeof(string));

            // MySQL/MariaDB: Disable foreign key checks to allow column modifications
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS=0");

            migrationBuilder.AlterColumn<string>("SourceCode", "CurrenciesPegging", "varchar(3) CHARACTER SET utf8mb4",
                                                 nullable: false, oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<string>("DestinationCode", "CurrenciesPegging",
                                                 "varchar(3) CHARACTER SET utf8mb4", nullable: false,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<string>("CurrencyCode", "CurrenciesInflation",
                                                 "varchar(3) CHARACTER SET utf8mb4", nullable: false,
                                                 oldClrType: typeof(string), oldNullable: true);

            // Recreate foreign key constraints for Down operation (when reverting to NOT NULL)
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
    }
}
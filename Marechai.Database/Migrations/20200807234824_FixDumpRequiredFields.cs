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

            migrationBuilder.AlterColumn<string>("SourceCode", "CurrenciesPegging", nullable: true,
                                                 oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4");

            migrationBuilder.AlterColumn<string>("DestinationCode", "CurrenciesPegging", nullable: true,
                                                 oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4");

            migrationBuilder.AlterColumn<string>("CurrencyCode", "CurrenciesInflation", nullable: true,
                                                 oldClrType: typeof(string),
                                                 oldType: "varchar(3) CHARACTER SET utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>("MediaId", "Dumps", "bigint unsigned", nullable: true,
                                                oldClrType: typeof(ulong));

            migrationBuilder.AlterColumn<ulong>("MediaDumpId", "Dumps", "bigint unsigned", nullable: true,
                                                oldClrType: typeof(ulong));

            migrationBuilder.AlterColumn<string>("Dumper", "Dumps", "varchar(255) CHARACTER SET utf8mb4",
                                                 nullable: true, oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>("SourceCode", "CurrenciesPegging", "varchar(3) CHARACTER SET utf8mb4",
                                                 nullable: false, oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<string>("DestinationCode", "CurrenciesPegging",
                                                 "varchar(3) CHARACTER SET utf8mb4", nullable: false,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<string>("CurrencyCode", "CurrenciesInflation",
                                                 "varchar(3) CHARACTER SET utf8mb4", nullable: false,
                                                 oldClrType: typeof(string), oldNullable: true);
        }
    }
}
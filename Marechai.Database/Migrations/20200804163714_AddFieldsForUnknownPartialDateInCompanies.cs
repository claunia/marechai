using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddFieldsForUnknownPartialDateInCompanies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>("FoundedDayIsUnknown", "companies", nullable: false, defaultValue: false);

            migrationBuilder.AddColumn<bool>("FoundedMonthIsUnknown", "companies", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<bool>("SoldDayIsUnknown", "companies", nullable: false, defaultValue: false);

            migrationBuilder.AddColumn<bool>("SoldMonthIsUnknown", "companies", nullable: false, defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("FoundedDayIsUnknown", "companies");

            migrationBuilder.DropColumn("FoundedMonthIsUnknown", "companies");

            migrationBuilder.DropColumn("SoldDayIsUnknown", "companies");

            migrationBuilder.DropColumn("SoldMonthIsUnknown", "companies");
        }
    }
}
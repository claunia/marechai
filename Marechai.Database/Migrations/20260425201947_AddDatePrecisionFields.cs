using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDatePrecisionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, add the new precision columns for companies (before dropping booleans)
            migrationBuilder.AddColumn<byte>(
                name: "FoundedPrecision",
                table: "companies",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "SoldPrecision",
                table: "companies",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            // Migrate Company boolean flags to enum values:
            // MonthIsUnknown=true → YearOnly(2), DayIsUnknown=true → MonthYear(1), both false → Full(0)
            migrationBuilder.Sql(
                "UPDATE companies SET FoundedPrecision = CASE WHEN FoundedMonthIsUnknown = 1 THEN 2 WHEN FoundedDayIsUnknown = 1 THEN 1 ELSE 0 END");
            migrationBuilder.Sql(
                "UPDATE companies SET SoldPrecision = CASE WHEN SoldMonthIsUnknown = 1 THEN 2 WHEN SoldDayIsUnknown = 1 THEN 1 ELSE 0 END");

            // Now drop the old boolean columns
            migrationBuilder.DropColumn(
                name: "FoundedDayIsUnknown",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "FoundedMonthIsUnknown",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "SoldDayIsUnknown",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "SoldMonthIsUnknown",
                table: "companies");

            // Add precision columns for all other entities

            migrationBuilder.AddColumn<byte>(
                name: "IntroducedPrecision",
                table: "sound_synths",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "ReleaseDatePrecision",
                table: "SoftwareReleases",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "IntroducedPrecision",
                table: "SoftwareFamilies",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "IntroducedPrecision",
                table: "processors",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "EndPrecision",
                table: "PeopleByCompany",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "StartPrecision",
                table: "PeopleByCompany",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "BirthDatePrecision",
                table: "People",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "DeathDatePrecision",
                table: "People",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "AcquisitionDatePrecision",
                table: "OwnedMachines",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "LastStatusDatePrecision",
                table: "OwnedMachines",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "LostDatePrecision",
                table: "OwnedMachines",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "FirstPublicationPrecision",
                table: "Magazines",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "PublishedPrecision",
                table: "Magazines",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "PublishedPrecision",
                table: "MagazineIssues",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "IntroducedPrecision",
                table: "machines",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "IntroducedPrecision",
                table: "gpus",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "PublishedPrecision",
                table: "Documents",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "PublishedPrecision",
                table: "Books",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntroducedPrecision",
                table: "sound_synths");

            migrationBuilder.DropColumn(
                name: "ReleaseDatePrecision",
                table: "SoftwareReleases");

            migrationBuilder.DropColumn(
                name: "IntroducedPrecision",
                table: "SoftwareFamilies");

            migrationBuilder.DropColumn(
                name: "IntroducedPrecision",
                table: "processors");

            migrationBuilder.DropColumn(
                name: "EndPrecision",
                table: "PeopleByCompany");

            migrationBuilder.DropColumn(
                name: "StartPrecision",
                table: "PeopleByCompany");

            migrationBuilder.DropColumn(
                name: "BirthDatePrecision",
                table: "People");

            migrationBuilder.DropColumn(
                name: "DeathDatePrecision",
                table: "People");

            migrationBuilder.DropColumn(
                name: "AcquisitionDatePrecision",
                table: "OwnedMachines");

            migrationBuilder.DropColumn(
                name: "LastStatusDatePrecision",
                table: "OwnedMachines");

            migrationBuilder.DropColumn(
                name: "LostDatePrecision",
                table: "OwnedMachines");

            migrationBuilder.DropColumn(
                name: "FirstPublicationPrecision",
                table: "Magazines");

            migrationBuilder.DropColumn(
                name: "PublishedPrecision",
                table: "Magazines");

            migrationBuilder.DropColumn(
                name: "PublishedPrecision",
                table: "MagazineIssues");

            migrationBuilder.DropColumn(
                name: "IntroducedPrecision",
                table: "machines");

            migrationBuilder.DropColumn(
                name: "IntroducedPrecision",
                table: "gpus");

            migrationBuilder.DropColumn(
                name: "PublishedPrecision",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "FoundedPrecision",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "SoldPrecision",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "PublishedPrecision",
                table: "Books");

            migrationBuilder.AddColumn<bool>(
                name: "FoundedDayIsUnknown",
                table: "companies",
                type: "bit(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FoundedMonthIsUnknown",
                table: "companies",
                type: "bit(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoldDayIsUnknown",
                table: "companies",
                type: "bit(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoldMonthIsUnknown",
                table: "companies",
                type: "bit(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddIgdbCompanyEnrichmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChangeDate",
                table: "IgdbCompanies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChangeDateFormat",
                table: "IgdbCompanies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyTypeHistoryJson",
                table: "IgdbCompanies",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<short>(
                name: "Country",
                table: "IgdbCompanies",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionRaw",
                table: "IgdbCompanies",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "EnrichedOn",
                table: "IgdbCompanies",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnrichmentApplied",
                table: "IgdbCompanies",
                type: "bit(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LogoImageId",
                table: "IgdbCompanies",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "ParentIgdbId",
                table: "IgdbCompanies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StartDate",
                table: "IgdbCompanies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartDateFormat",
                table: "IgdbCompanies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "IgdbCompanies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsitesJson",
                table: "IgdbCompanies",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeDate",
                table: "IgdbCompanies");

            migrationBuilder.DropColumn(
                name: "ChangeDateFormat",
                table: "IgdbCompanies");

            migrationBuilder.DropColumn(
                name: "CompanyTypeHistoryJson",
                table: "IgdbCompanies");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "IgdbCompanies");

            migrationBuilder.DropColumn(
                name: "DescriptionRaw",
                table: "IgdbCompanies");

            migrationBuilder.DropColumn(
                name: "EnrichedOn",
                table: "IgdbCompanies");

            migrationBuilder.DropColumn(
                name: "EnrichmentApplied",
                table: "IgdbCompanies");

            migrationBuilder.DropColumn(
                name: "LogoImageId",
                table: "IgdbCompanies");

            migrationBuilder.DropColumn(
                name: "ParentIgdbId",
                table: "IgdbCompanies");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "IgdbCompanies");

            migrationBuilder.DropColumn(
                name: "StartDateFormat",
                table: "IgdbCompanies");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "IgdbCompanies");

            migrationBuilder.DropColumn(
                name: "WebsitesJson",
                table: "IgdbCompanies");
        }
    }
}

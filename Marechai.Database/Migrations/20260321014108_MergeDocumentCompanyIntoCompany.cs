using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class MergeDocumentCompanyIntoCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompaniesByBooks_DocumentCompanies_CompanyId",
                table: "CompaniesByBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_CompaniesByDocuments_DocumentCompanies_CompanyId",
                table: "CompaniesByDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_CompaniesByMagazines_DocumentCompanies_CompanyId",
                table: "CompaniesByMagazines");

            // Step 1: For DocumentCompanies linked to a Company, update junction tables
            // to point directly to the linked Company.Id
            migrationBuilder.Sql("""
                UPDATE `CompaniesByBooks` cbb
                JOIN `DocumentCompanies` dc ON cbb.`CompanyId` = dc.`Id`
                SET cbb.`CompanyId` = dc.`CompanyId`
                WHERE dc.`CompanyId` IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE `CompaniesByDocuments` cbd
                JOIN `DocumentCompanies` dc ON cbd.`CompanyId` = dc.`Id`
                SET cbd.`CompanyId` = dc.`CompanyId`
                WHERE dc.`CompanyId` IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE `CompaniesByMagazines` cbm
                JOIN `DocumentCompanies` dc ON cbm.`CompanyId` = dc.`Id`
                SET cbm.`CompanyId` = dc.`CompanyId`
                WHERE dc.`CompanyId` IS NOT NULL;
                """);

            // Step 2: For DocumentCompanies NOT linked to a Company, insert into companies
            migrationBuilder.Sql("""
                INSERT INTO `companies` (`name`, `status`, `CreatedOn`)
                SELECT `Name`, 0, NOW()
                FROM `DocumentCompanies`
                WHERE `CompanyId` IS NULL;
                """);

            // Step 3: Update junction tables for the newly created company records
            // Match by Name
            migrationBuilder.Sql("""
                UPDATE `CompaniesByBooks` cbb
                JOIN `DocumentCompanies` dc ON cbb.`CompanyId` = dc.`Id`
                JOIN `companies` c ON c.`name` = dc.`Name`
                SET cbb.`CompanyId` = c.`id`
                WHERE dc.`CompanyId` IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE `CompaniesByDocuments` cbd
                JOIN `DocumentCompanies` dc ON cbd.`CompanyId` = dc.`Id`
                JOIN `companies` c ON c.`name` = dc.`Name`
                SET cbd.`CompanyId` = c.`id`
                WHERE dc.`CompanyId` IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE `CompaniesByMagazines` cbm
                JOIN `DocumentCompanies` dc ON cbm.`CompanyId` = dc.`Id`
                JOIN `companies` c ON c.`name` = dc.`Name`
                SET cbm.`CompanyId` = c.`id`
                WHERE dc.`CompanyId` IS NULL;
                """);

            migrationBuilder.DropTable(
                name: "DocumentCompanies");

            migrationBuilder.DropColumn(
                name: "DocumentCompanyId",
                table: "companies");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "CompaniesByMagazines",
                type: "int(11)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "CompaniesByDocuments",
                type: "int(11)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "CompaniesByBooks",
                type: "int(11)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CompaniesByBooks_companies_CompanyId",
                table: "CompaniesByBooks",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompaniesByDocuments_companies_CompanyId",
                table: "CompaniesByDocuments",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompaniesByMagazines_companies_CompanyId",
                table: "CompaniesByMagazines",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompaniesByBooks_companies_CompanyId",
                table: "CompaniesByBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_CompaniesByDocuments_companies_CompanyId",
                table: "CompaniesByDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_CompaniesByMagazines_companies_CompanyId",
                table: "CompaniesByMagazines");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "CompaniesByMagazines",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(11)");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "CompaniesByDocuments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(11)");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "CompaniesByBooks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(11)");

            migrationBuilder.AddColumn<int>(
                name: "DocumentCompanyId",
                table: "companies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int(11)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentCompanies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentCompanies_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCompanies_CompanyId",
                table: "DocumentCompanies",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCompanies_Name",
                table: "DocumentCompanies",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_CompaniesByBooks_DocumentCompanies_CompanyId",
                table: "CompaniesByBooks",
                column: "CompanyId",
                principalTable: "DocumentCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompaniesByDocuments_DocumentCompanies_CompanyId",
                table: "CompaniesByDocuments",
                column: "CompanyId",
                principalTable: "DocumentCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompaniesByMagazines_DocumentCompanies_CompanyId",
                table: "CompaniesByMagazines",
                column: "CompanyId",
                principalTable: "DocumentCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

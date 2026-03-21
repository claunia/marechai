using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class MergeDocumentPersonIntoPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeopleByBooks_DocumentPeople_PersonId",
                table: "PeopleByBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_PeopleByDocuments_DocumentPeople_PersonId",
                table: "PeopleByDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_PeopleByMagazines_DocumentPeople_PersonId",
                table: "PeopleByMagazines");

            // Step 1: For DocumentPeople linked to a Person, update junction tables
            // to point directly to the linked Person.Id
            migrationBuilder.Sql("""
                UPDATE `PeopleByBooks` pbb
                JOIN `DocumentPeople` dp ON pbb.`PersonId` = dp.`Id`
                SET pbb.`PersonId` = dp.`PersonId`
                WHERE dp.`PersonId` IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE `PeopleByDocuments` pbd
                JOIN `DocumentPeople` dp ON pbd.`PersonId` = dp.`Id`
                SET pbd.`PersonId` = dp.`PersonId`
                WHERE dp.`PersonId` IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE `PeopleByMagazines` pbm
                JOIN `DocumentPeople` dp ON pbm.`PersonId` = dp.`Id`
                SET pbm.`PersonId` = dp.`PersonId`
                WHERE dp.`PersonId` IS NOT NULL;
                """);

            // Step 2: For DocumentPeople NOT linked to a Person, insert into People
            migrationBuilder.Sql("""
                INSERT INTO `People` (`Name`, `Surname`, `Alias`, `DisplayName`, `BirthDate`, `Photo`, `CreatedOn`)
                SELECT `Name`, `Surname`, `Alias`, `DisplayName`,
                       '0001-01-01', UNHEX(REPLACE('00000000-0000-0000-0000-000000000000', '-', '')),
                       NOW()
                FROM `DocumentPeople`
                WHERE `PersonId` IS NULL;
                """);

            // Step 3: Update junction tables for the newly created People records
            // Match by Name + Surname + COALESCE(Alias) + COALESCE(DisplayName)
            migrationBuilder.Sql("""
                UPDATE `PeopleByBooks` pbb
                JOIN `DocumentPeople` dp ON pbb.`PersonId` = dp.`Id`
                JOIN `People` p ON p.`Name` = dp.`Name`
                    AND p.`Surname` = dp.`Surname`
                    AND (p.`Alias` = dp.`Alias` OR (p.`Alias` IS NULL AND dp.`Alias` IS NULL))
                    AND (p.`DisplayName` = dp.`DisplayName` OR (p.`DisplayName` IS NULL AND dp.`DisplayName` IS NULL))
                    AND p.`BirthDate` = '0001-01-01'
                SET pbb.`PersonId` = p.`Id`
                WHERE dp.`PersonId` IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE `PeopleByDocuments` pbd
                JOIN `DocumentPeople` dp ON pbd.`PersonId` = dp.`Id`
                JOIN `People` p ON p.`Name` = dp.`Name`
                    AND p.`Surname` = dp.`Surname`
                    AND (p.`Alias` = dp.`Alias` OR (p.`Alias` IS NULL AND dp.`Alias` IS NULL))
                    AND (p.`DisplayName` = dp.`DisplayName` OR (p.`DisplayName` IS NULL AND dp.`DisplayName` IS NULL))
                    AND p.`BirthDate` = '0001-01-01'
                SET pbd.`PersonId` = p.`Id`
                WHERE dp.`PersonId` IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE `PeopleByMagazines` pbm
                JOIN `DocumentPeople` dp ON pbm.`PersonId` = dp.`Id`
                JOIN `People` p ON p.`Name` = dp.`Name`
                    AND p.`Surname` = dp.`Surname`
                    AND (p.`Alias` = dp.`Alias` OR (p.`Alias` IS NULL AND dp.`Alias` IS NULL))
                    AND (p.`DisplayName` = dp.`DisplayName` OR (p.`DisplayName` IS NULL AND dp.`DisplayName` IS NULL))
                    AND p.`BirthDate` = '0001-01-01'
                SET pbm.`PersonId` = p.`Id`
                WHERE dp.`PersonId` IS NULL;
                """);

            migrationBuilder.DropTable(
                name: "DocumentPeople");

            migrationBuilder.DropColumn(
                name: "DocumentPersonId",
                table: "People");

            migrationBuilder.AddForeignKey(
                name: "FK_PeopleByBooks_People_PersonId",
                table: "PeopleByBooks",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PeopleByDocuments_People_PersonId",
                table: "PeopleByDocuments",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PeopleByMagazines_People_PersonId",
                table: "PeopleByMagazines",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeopleByBooks_People_PersonId",
                table: "PeopleByBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_PeopleByDocuments_People_PersonId",
                table: "PeopleByDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_PeopleByMagazines_People_PersonId",
                table: "PeopleByMagazines");

            migrationBuilder.AddColumn<int>(
                name: "DocumentPersonId",
                table: "People",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentPeople",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PersonId = table.Column<int>(type: "int", nullable: true),
                    Alias = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DisplayName = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Surname = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentPeople", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentPeople_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPeople_Alias",
                table: "DocumentPeople",
                column: "Alias");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPeople_DisplayName",
                table: "DocumentPeople",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPeople_Name",
                table: "DocumentPeople",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPeople_PersonId",
                table: "DocumentPeople",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPeople_Surname",
                table: "DocumentPeople",
                column: "Surname");

            migrationBuilder.AddForeignKey(
                name: "FK_PeopleByBooks_DocumentPeople_PersonId",
                table: "PeopleByBooks",
                column: "PersonId",
                principalTable: "DocumentPeople",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PeopleByDocuments_DocumentPeople_PersonId",
                table: "PeopleByDocuments",
                column: "PersonId",
                principalTable: "DocumentPeople",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PeopleByMagazines_DocumentPeople_PersonId",
                table: "PeopleByMagazines",
                column: "PersonId",
                principalTable: "DocumentPeople",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

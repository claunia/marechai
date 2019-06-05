using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class AddPeopleByCompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeopleByCompany",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PersonId = table.Column<int>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    Position = table.Column<string>(nullable: true),
                    Start = table.Column<DateTime>(nullable: true),
                    End = table.Column<DateTime>(nullable: true),
                    Ongoing = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeopleByCompany", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeopleByCompany_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeopleByCompany_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeopleByCompany_CompanyId",
                table: "PeopleByCompany",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleByCompany_End",
                table: "PeopleByCompany",
                column: "End");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleByCompany_PersonId",
                table: "PeopleByCompany",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleByCompany_Position",
                table: "PeopleByCompany",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleByCompany_Start",
                table: "PeopleByCompany",
                column: "Start");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeopleByCompany");
        }
    }
}

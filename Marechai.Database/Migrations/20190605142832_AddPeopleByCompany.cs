using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddPeopleByCompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("PeopleByCompany", table => new
            {
                Id = table.Column<long>().
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                PersonId  = table.Column<int>(),
                CompanyId = table.Column<int>(),
                Position  = table.Column<string>(nullable: true),
                Start     = table.Column<DateTime>(nullable: true),
                End       = table.Column<DateTime>(nullable: true),
                Ongoing   = table.Column<bool>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_PeopleByCompany", x => x.Id);

                table.ForeignKey("FK_PeopleByCompany_companies_CompanyId", x => x.CompanyId, "companies", "id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_PeopleByCompany_People_PersonId", x => x.PersonId, "People", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_PeopleByCompany_CompanyId", "PeopleByCompany", "CompanyId");

            migrationBuilder.CreateIndex("IX_PeopleByCompany_End", "PeopleByCompany", "End");

            migrationBuilder.CreateIndex("IX_PeopleByCompany_PersonId", "PeopleByCompany", "PersonId");

            migrationBuilder.CreateIndex("IX_PeopleByCompany_Position", "PeopleByCompany", "Position");

            migrationBuilder.CreateIndex("IX_PeopleByCompany_Start", "PeopleByCompany", "Start");
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable("PeopleByCompany");
    }
}
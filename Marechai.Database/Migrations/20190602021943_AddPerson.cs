using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("People", table => new
            {
                Id = table.Column<int>().
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name      = table.Column<string>(), Surname = table.Column<string>(),
                BirthDate = table.Column<DateTime>(),
                DeathDate = table.Column<DateTime>(nullable: true), Webpage = table.Column<string>(nullable: true),
                Twitter   = table.Column<string>(nullable: true), Facebook  = table.Column<string>(nullable: true),
                Photo     = table.Column<Guid>(), CountryOfBirthId          = table.Column<short>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_People", x => x.Id);

                table.ForeignKey("FK_People_iso3166_1_numeric_CountryOfBirthId", x => x.CountryOfBirthId,
                                 "iso3166_1_numeric", "id", onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateIndex("IX_People_BirthDate", "People", "BirthDate");

            migrationBuilder.CreateIndex("IX_People_CountryOfBirthId", "People", "CountryOfBirthId");

            migrationBuilder.CreateIndex("IX_People_DeathDate", "People", "DeathDate");

            migrationBuilder.CreateIndex("IX_People_Facebook", "People", "Facebook");

            migrationBuilder.CreateIndex("IX_People_Name", "People", "Name");

            migrationBuilder.CreateIndex("IX_People_Photo", "People", "Photo");

            migrationBuilder.CreateIndex("IX_People_Surname", "People", "Surname");

            migrationBuilder.CreateIndex("IX_People_Twitter", "People", "Twitter");

            migrationBuilder.CreateIndex("IX_People_Webpage", "People", "Webpage");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("People");
    }
}
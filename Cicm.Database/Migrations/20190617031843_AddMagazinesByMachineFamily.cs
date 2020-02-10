using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddMagazinesByMachineFamily : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("MagazinesByMachinesFamilies",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             MagazineId      = table.Column<long>(),
                                             MachineFamilyId = table.Column<int>()
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_MagazinesByMachinesFamilies", x => x.Id);
                                             table
                                                .ForeignKey("FK_MagazinesByMachinesFamilies_machine_families_MachineFamilyId",
                                                            x => x.MachineFamilyId, "machine_families", "id",
                                                            onDelete: ReferentialAction.Cascade);
                                             table
                                                .ForeignKey("FK_MagazinesByMachinesFamilies_MagazineIssues_MagazineId",
                                                            x => x.MagazineId, "MagazineIssues", "Id",
                                                            onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateIndex("IX_MagazinesByMachinesFamilies_MachineFamilyId",
                                         "MagazinesByMachinesFamilies", "MachineFamilyId");

            migrationBuilder.CreateIndex("IX_MagazinesByMachinesFamilies_MagazineId", "MagazinesByMachinesFamilies",
                                         "MagazineId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("MagazinesByMachinesFamilies");
        }
    }
}
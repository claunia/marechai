using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddMagazinesByMachine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("MagazinesByMachines", table => new
            {
                Id = table.Column<long>().
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                MagazineId = table.Column<long>(),
                MachineId  = table.Column<int>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_MagazinesByMachines", x => x.Id);

                table.ForeignKey("FK_MagazinesByMachines_machines_MachineId", x => x.MachineId, "machines", "id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_MagazinesByMachines_MagazineIssues_MagazineId", x => x.MagazineId,
                                 "MagazineIssues", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_MagazinesByMachines_MachineId", "MagazinesByMachines", "MachineId");

            migrationBuilder.CreateIndex("IX_MagazinesByMachines_MagazineId", "MagazinesByMachines", "MagazineId");
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable("MagazinesByMachines");
    }
}
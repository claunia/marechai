using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class AddDocumentssByMachine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("DocumentsByMachines",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             DocumentId = table.Column<long>(),
                                             MachineId  = table.Column<int>()
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_DocumentsByMachines", x => x.Id);
                                             table.ForeignKey("FK_DocumentsByMachines_Documents_DocumentId",
                                                              x => x.DocumentId, "Documents", "Id",
                                                              onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_DocumentsByMachines_machines_MachineId",
                                                              x => x.MachineId, "machines", "id",
                                                              onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateIndex("IX_DocumentsByMachines_DocumentId", "DocumentsByMachines", "DocumentId");

            migrationBuilder.CreateIndex("IX_DocumentsByMachines_MachineId", "DocumentsByMachines", "MachineId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("DocumentsByMachines");
        }
    }
}
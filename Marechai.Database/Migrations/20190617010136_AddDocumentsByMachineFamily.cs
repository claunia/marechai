using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddDocumentsByMachineFamily : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("DocumentsByMachineFamily", table => new
            {
                Id = table.Column<long>().
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                DocumentId = table.Column<long>(), MachineFamilyId = table.Column<int>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_DocumentsByMachineFamily", x => x.Id);

                table.ForeignKey("FK_DocumentsByMachineFamily_Documents_DocumentId", x => x.DocumentId, "Documents",
                                 "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_DocumentsByMachineFamily_machine_families_MachineFamilyId", x => x.MachineFamilyId,
                                 "machine_families", "id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_DocumentsByMachineFamily_DocumentId", "DocumentsByMachineFamily",
                                         "DocumentId");

            migrationBuilder.CreateIndex("IX_DocumentsByMachineFamily_MachineFamilyId", "DocumentsByMachineFamily",
                                         "MachineFamilyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable("DocumentsByMachineFamily");
    }
}
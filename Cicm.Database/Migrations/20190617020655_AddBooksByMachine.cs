using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class AddBooksByMachine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("BooksByMachines",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             BookId    = table.Column<long>(),
                                             MachineId = table.Column<int>()
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_BooksByMachines", x => x.Id);
                                             table.ForeignKey("FK_BooksByMachines_Books_BookId", x => x.BookId, "Books",
                                                              "Id", onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_BooksByMachines_machines_MachineId", x => x.MachineId,
                                                              "machines", "id", onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateIndex("IX_BooksByMachines_BookId", "BooksByMachines", "BookId");

            migrationBuilder.CreateIndex("IX_BooksByMachines_MachineId", "BooksByMachines", "MachineId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("BooksByMachines");
        }
    }
}
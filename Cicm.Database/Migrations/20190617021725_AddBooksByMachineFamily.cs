using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddBooksByMachineFamily : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("BooksByMachineFamilies",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             BookId          = table.Column<long>(),
                                             MachineFamilyId = table.Column<int>()
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_BooksByMachineFamilies", x => x.Id);
                                             table.ForeignKey("FK_BooksByMachineFamilies_Books_BookId", x => x.BookId,
                                                              "Books", "Id", onDelete: ReferentialAction.Cascade);
                                             table
                                                .ForeignKey("FK_BooksByMachineFamilies_machine_families_MachineFamilyId",
                                                            x => x.MachineFamilyId, "machine_families", "id",
                                                            onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateIndex("IX_BooksByMachineFamilies_BookId", "BooksByMachineFamilies", "BookId");

            migrationBuilder.CreateIndex("IX_BooksByMachineFamilies_MachineFamilyId", "BooksByMachineFamilies",
                                         "MachineFamilyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("BooksByMachineFamilies");
        }
    }
}
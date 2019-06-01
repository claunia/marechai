using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class AddScreens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Screens",
                                         table => new
                                         {
                                             Id = table.Column<int>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             Width              = table.Column<double>(nullable: true),
                                             Height             = table.Column<double>(nullable: true),
                                             Diagonal           = table.Column<double>(),
                                             NativeResolutionId = table.Column<int>(),
                                             EffectiveColors    = table.Column<long>(nullable: true),
                                             Type               = table.Column<string>()
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_Screens", x => x.Id);
                                             table.ForeignKey("FK_Screens_resolutions_NativeResolutionId",
                                                              x => x.NativeResolutionId, "resolutions", "id",
                                                              onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("ResolutionsByScreen",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             ScreenId     = table.Column<int>(),
                                             ResolutionId = table.Column<int>()
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_ResolutionsByScreen", x => x.Id);
                                             table.ForeignKey("FK_ResolutionsByScreen_resolutions_ResolutionId",
                                                              x => x.ResolutionId, "resolutions", "id",
                                                              onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_ResolutionsByScreen_Screens_ScreenId",
                                                              x => x.ScreenId, "Screens", "Id",
                                                              onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("ScreensByMachine",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             ScreenId  = table.Column<int>(),
                                             MachineId = table.Column<int>()
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_ScreensByMachine", x => x.Id);
                                             table.ForeignKey("FK_ScreensByMachine_machines_MachineId",
                                                              x => x.MachineId, "machines", "id",
                                                              onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_ScreensByMachine_Screens_ScreenId", x => x.ScreenId,
                                                              "Screens", "Id", onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateIndex("IX_ResolutionsByScreen_ResolutionId", "ResolutionsByScreen", "ResolutionId");

            migrationBuilder.CreateIndex("IX_ResolutionsByScreen_ScreenId", "ResolutionsByScreen", "ScreenId");

            migrationBuilder.CreateIndex("IX_Screens_Diagonal", "Screens", "Diagonal");

            migrationBuilder.CreateIndex("IX_Screens_EffectiveColors", "Screens", "EffectiveColors");

            migrationBuilder.CreateIndex("IX_Screens_Height", "Screens", "Height");

            migrationBuilder.CreateIndex("IX_Screens_NativeResolutionId", "Screens", "NativeResolutionId");

            migrationBuilder.CreateIndex("IX_Screens_Type", "Screens", "Type");

            migrationBuilder.CreateIndex("IX_Screens_Width", "Screens", "Width");

            migrationBuilder.CreateIndex("IX_ScreensByMachine_MachineId", "ScreensByMachine", "MachineId");

            migrationBuilder.CreateIndex("IX_ScreensByMachine_ScreenId", "ScreensByMachine", "ScreenId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("ResolutionsByScreen");

            migrationBuilder.DropTable("ScreensByMachine");

            migrationBuilder.DropTable("Screens");
        }
    }
}
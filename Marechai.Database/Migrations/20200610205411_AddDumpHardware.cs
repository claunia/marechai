using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddDumpHardware : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("DumpHardwares", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Manufacturer            = table.Column<string>(maxLength: 48, nullable: true),
                Model                   = table.Column<string>(maxLength: 48, nullable: false),
                Revision                = table.Column<string>(maxLength: 48, nullable: true),
                Firmware                = table.Column<string>(maxLength: 32, nullable: true),
                Serial                  = table.Column<string>(maxLength: 64, nullable: true),
                SoftwareName            = table.Column<string>(maxLength: 64, nullable: false),
                SoftwareVersion         = table.Column<string>(maxLength: 32, nullable: true),
                SoftwareOperatingSystem = table.Column<string>(maxLength: 64, nullable: true),
                Extents                 = table.Column<string>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_DumpHardwares", x => x.Id);
            });

            migrationBuilder.CreateIndex("IX_DumpHardwares_Firmware", "DumpHardwares", "Firmware");

            migrationBuilder.CreateIndex("IX_DumpHardwares_Manufacturer", "DumpHardwares", "Manufacturer");

            migrationBuilder.CreateIndex("IX_DumpHardwares_Model", "DumpHardwares", "Model");

            migrationBuilder.CreateIndex("IX_DumpHardwares_Revision", "DumpHardwares", "Revision");

            migrationBuilder.CreateIndex("IX_DumpHardwares_Serial", "DumpHardwares", "Serial");

            migrationBuilder.CreateIndex("IX_DumpHardwares_SoftwareName", "DumpHardwares", "SoftwareName");

            migrationBuilder.CreateIndex("IX_DumpHardwares_SoftwareOperatingSystem", "DumpHardwares",
                                         "SoftwareOperatingSystem");

            migrationBuilder.CreateIndex("IX_DumpHardwares_SoftwareVersion", "DumpHardwares", "SoftwareVersion");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("DumpHardwares");
    }
}
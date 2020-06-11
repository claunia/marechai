/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddDumps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>("DumpId", "DumpHardwares", nullable: false, defaultValue: 0ul);

            migrationBuilder.CreateTable("Dumps", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Dumper       = table.Column<string>(nullable: true), UserId = table.Column<string>(nullable: true),
                DumpingGroup = table.Column<string>(nullable: true), DumpDate = table.Column<DateTime>(nullable: true),
                MediaId      = table.Column<ulong>(nullable: true), MediaDumpId = table.Column<ulong>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Dumps", x => x.Id);

                table.ForeignKey("FK_Dumps_MediaDumps_MediaDumpId", x => x.MediaDumpId, "MediaDumps", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_Dumps_Media_MediaId", x => x.MediaId, "Media", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_Dumps_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id",
                                 onDelete: ReferentialAction.SetNull);
            });

            migrationBuilder.CreateIndex("IX_DumpHardwares_DumpId", "DumpHardwares", "DumpId");

            migrationBuilder.CreateIndex("IX_Dumps_DumpDate", "Dumps", "DumpDate");

            migrationBuilder.CreateIndex("IX_Dumps_Dumper", "Dumps", "Dumper");

            migrationBuilder.CreateIndex("IX_Dumps_DumpingGroup", "Dumps", "DumpingGroup");

            migrationBuilder.CreateIndex("IX_Dumps_MediaDumpId", "Dumps", "MediaDumpId");

            migrationBuilder.CreateIndex("IX_Dumps_MediaId", "Dumps", "MediaId");

            migrationBuilder.CreateIndex("IX_Dumps_UserId", "Dumps", "UserId");

            migrationBuilder.AddForeignKey("FK_DumpHardwares_Dumps_DumpId", "DumpHardwares", "DumpId", "Dumps",
                                           principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_DumpHardwares_Dumps_DumpId", "DumpHardwares");

            migrationBuilder.DropTable("Dumps");

            migrationBuilder.DropIndex("IX_DumpHardwares_DumpId", "DumpHardwares");

            migrationBuilder.DropColumn("DumpId", "DumpHardwares");
        }
    }
}
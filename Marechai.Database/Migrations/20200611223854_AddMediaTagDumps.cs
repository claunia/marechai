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
    public partial class AddMediaTagDumps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("MediaTagDumps", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                MediaDumpId = table.Column<ulong>(nullable: false), Type = table.Column<int>(nullable: false),
                FileId      = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_MediaTagDumps", x => x.Id);

                table.ForeignKey("FK_MediaTagDumps_Files_FileId", x => x.FileId, "Files", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_MediaTagDumps_MediaDumps_MediaDumpId", x => x.MediaDumpId, "MediaDumps", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_MediaTagDumps_FileId", "MediaTagDumps", "FileId");

            migrationBuilder.CreateIndex("IX_MediaTagDumps_MediaDumpId", "MediaTagDumps", "MediaDumpId");

            migrationBuilder.CreateIndex("IX_MediaTagDumps_Type", "MediaTagDumps", "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("MediaTagDumps");
    }
}
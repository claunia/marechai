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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddMediaDump : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("MediaDumps", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                MediaId = table.Column<ulong>(nullable: false),
                Format  = table.Column<string>(nullable: true),
                Status  = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_MediaDumps", x => x.Id);

                table.ForeignKey("FK_MediaDumps_Media_MediaId", x => x.MediaId, "Media", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_MediaDumps_Format", "MediaDumps", "Format");

            migrationBuilder.CreateIndex("IX_MediaDumps_MediaId", "MediaDumps", "MediaId");

            migrationBuilder.CreateIndex("IX_MediaDumps_Status", "MediaDumps", "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("MediaDumps");
    }
}
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
    public partial class AddLogicalPartitionsByMedia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("LogicalPartitionsByMedia", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                MediaId     = table.Column<ulong>(nullable: false),
                PartitionId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_LogicalPartitionsByMedia", x => x.Id);

                table.ForeignKey("FK_LogicalPartitionsByMedia_Media_MediaId", x => x.MediaId, "Media", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_LogicalPartitionsByMedia_LogicalPartitions_PartitionId", x => x.PartitionId,
                                 "LogicalPartitions", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_LogicalPartitionsByMedia_MediaId", "LogicalPartitionsByMedia", "MediaId");

            migrationBuilder.CreateIndex("IX_LogicalPartitionsByMedia_PartitionId", "LogicalPartitionsByMedia",
                                         "PartitionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable("LogicalPartitionsByMedia");
    }
}
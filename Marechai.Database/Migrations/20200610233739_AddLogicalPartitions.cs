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
    public partial class AddLogicalPartitions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("LogicalPartitions", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Sequence    = table.Column<uint>(nullable: false),
                Name        = table.Column<string>(nullable: true),
                Type        = table.Column<string>(nullable: false),
                FirstSector = table.Column<ulong>(nullable: false),
                LastSector  = table.Column<ulong>(nullable: false),
                Size        = table.Column<ulong>(nullable: false),
                Description = table.Column<string>(nullable: true),
                Scheme      = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_LogicalPartitions", x => x.Id);
            });

            migrationBuilder.CreateTable("FilesystemsByLogicalPartition", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                FilesystemId = table.Column<ulong>(nullable: false),
                PartitionId  = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_FilesystemsByLogicalPartition", x => x.Id);

                table.ForeignKey("FK_FilesystemsByLogicalPartition_Filesystems_FilesystemId", x => x.FilesystemId,
                                 "Filesystems", "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_FilesystemsByLogicalPartition_LogicalPartitions_PartitionId", x => x.PartitionId,
                                 "LogicalPartitions", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_FilesystemsByLogicalPartition_FilesystemId",
                                         "FilesystemsByLogicalPartition", "FilesystemId");

            migrationBuilder.CreateIndex("IX_FilesystemsByLogicalPartition_PartitionId",
                                         "FilesystemsByLogicalPartition", "PartitionId");

            migrationBuilder.CreateIndex("IX_LogicalPartitions_Description", "LogicalPartitions", "Description");

            migrationBuilder.CreateIndex("IX_LogicalPartitions_FirstSector", "LogicalPartitions", "FirstSector");

            migrationBuilder.CreateIndex("IX_LogicalPartitions_LastSector", "LogicalPartitions", "LastSector");

            migrationBuilder.CreateIndex("IX_LogicalPartitions_Name", "LogicalPartitions", "Name");

            migrationBuilder.CreateIndex("IX_LogicalPartitions_Scheme", "LogicalPartitions", "Scheme");

            migrationBuilder.CreateIndex("IX_LogicalPartitions_Type", "LogicalPartitions", "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("FilesystemsByLogicalPartition");

            migrationBuilder.DropTable("LogicalPartitions");
        }
    }
}
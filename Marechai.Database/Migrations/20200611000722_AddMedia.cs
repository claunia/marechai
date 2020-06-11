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
    public partial class AddMedia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Media", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Title = table.Column<string>(nullable: true), Sequence = table.Column<ushort>(nullable: true),
                LastSequence = table.Column<ushort>(nullable: true), Type = table.Column<uint>(nullable: false),
                WriteOffset = table.Column<int>(nullable: true), Sides = table.Column<ushort>(nullable: true),
                Layers = table.Column<ushort>(nullable: true), Sessions = table.Column<ushort>(nullable: true),
                Tracks = table.Column<ushort>(nullable: true), Sectors = table.Column<ulong>(nullable: false),
                Size = table.Column<ulong>(nullable: false), CopyProtection = table.Column<string>(nullable: true),
                PartNumber = table.Column<string>(nullable: true), SerialNumber = table.Column<string>(nullable: true),
                Barcode = table.Column<string>(nullable: true), CatalogueNumber = table.Column<string>(nullable: true),
                Manufacturer = table.Column<string>(nullable: true), Model = table.Column<string>(nullable: true),
                Revision = table.Column<string>(nullable: true), Firmware = table.Column<string>(nullable: true),
                PhysicalBlockSize = table.Column<int>(nullable: true),
                LogicalBlockSize = table.Column<int>(nullable: true), BlockSizes = table.Column<string>(nullable: true),
                StorageInterface = table.Column<int>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Media", x => x.Id);
            });

            migrationBuilder.CreateIndex("IX_Media_Barcode", "Media", "Barcode");

            migrationBuilder.CreateIndex("IX_Media_CatalogueNumber", "Media", "CatalogueNumber");

            migrationBuilder.CreateIndex("IX_Media_CopyProtection", "Media", "CopyProtection");

            migrationBuilder.CreateIndex("IX_Media_Firmware", "Media", "Firmware");

            migrationBuilder.CreateIndex("IX_Media_Manufacturer", "Media", "Manufacturer");

            migrationBuilder.CreateIndex("IX_Media_Model", "Media", "Model");

            migrationBuilder.CreateIndex("IX_Media_PartNumber", "Media", "PartNumber");

            migrationBuilder.CreateIndex("IX_Media_Revision", "Media", "Revision");

            migrationBuilder.CreateIndex("IX_Media_SerialNumber", "Media", "SerialNumber");

            migrationBuilder.CreateIndex("IX_Media_Title", "Media", "Title");

            migrationBuilder.CreateIndex("IX_Media_Type", "Media", "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("Media");
    }
}
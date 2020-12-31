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
    public partial class AddFilesystems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Filesystems", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Type                   = table.Column<string>(nullable: false),
                CreationDate           = table.Column<DateTime>(nullable: true),
                ModificationDate       = table.Column<DateTime>(nullable: true),
                BackupDate             = table.Column<DateTime>(nullable: true),
                ClusterSize            = table.Column<int>(nullable: false),
                Clusters               = table.Column<ulong>(nullable: false),
                Files                  = table.Column<ulong>(nullable: true),
                Bootable               = table.Column<bool>(nullable: false),
                Serial                 = table.Column<string>(nullable: true),
                Label                  = table.Column<string>(nullable: true),
                FreeClusters           = table.Column<ulong>(nullable: true),
                ExpirationDate         = table.Column<DateTime>(nullable: true),
                EffectiveDate          = table.Column<DateTime>(nullable: true),
                SystemIdentifier       = table.Column<string>(nullable: true),
                VolumeSetIdentifier    = table.Column<string>(nullable: true),
                PublisherIdentifier    = table.Column<string>(nullable: true),
                DataPreparerIdentifier = table.Column<string>(nullable: true),
                ApplicationIdentifier  = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Filesystems", x => x.Id);
            });

            migrationBuilder.CreateIndex("IX_Filesystems_ApplicationIdentifier", "Filesystems",
                                         "ApplicationIdentifier");

            migrationBuilder.CreateIndex("IX_Filesystems_BackupDate", "Filesystems", "BackupDate");

            migrationBuilder.CreateIndex("IX_Filesystems_CreationDate", "Filesystems", "CreationDate");

            migrationBuilder.CreateIndex("IX_Filesystems_DataPreparerIdentifier", "Filesystems",
                                         "DataPreparerIdentifier");

            migrationBuilder.CreateIndex("IX_Filesystems_Label", "Filesystems", "Label");

            migrationBuilder.CreateIndex("IX_Filesystems_ModificationDate", "Filesystems", "ModificationDate");

            migrationBuilder.CreateIndex("IX_Filesystems_PublisherIdentifier", "Filesystems", "PublisherIdentifier");

            migrationBuilder.CreateIndex("IX_Filesystems_Serial", "Filesystems", "Serial");

            migrationBuilder.CreateIndex("IX_Filesystems_SystemIdentifier", "Filesystems", "SystemIdentifier");

            migrationBuilder.CreateIndex("IX_Filesystems_Type", "Filesystems", "Type");

            migrationBuilder.CreateIndex("IX_Filesystems_VolumeSetIdentifier", "Filesystems", "VolumeSetIdentifier");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("Filesystems");
    }
}
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
    public partial class AddMediaDumpFileImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("MediaDumpFileImages", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                MediaDumpId = table.Column<ulong>(nullable: false), FileSequence = table.Column<long>(nullable: false),
                PartitionSequence = table.Column<short>(nullable: false), Size = table.Column<ulong>(nullable: false),
                Md5 = table.Column<byte[]>("binary(16)", nullable: true),
                Sha1 = table.Column<byte[]>("binary(20)", nullable: true),
                Sha256 = table.Column<byte[]>("binary(32)", nullable: true),
                Sha3 = table.Column<byte[]>("binary(64)", nullable: true),
                Spamsum = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_MediaDumpFileImages", x => x.Id);

                table.ForeignKey("FK_MediaDumpFileImages_MediaDumps_MediaDumpId", x => x.MediaDumpId, "MediaDumps",
                                 "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("FilesystemsByMediaDumpFile", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                FilesystemId         = table.Column<ulong>(nullable: false),
                MediaDumpFileImageId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_FilesystemsByMediaDumpFile", x => x.Id);

                table.ForeignKey("FK_FilesystemsByMediaDumpFile_Filesystems_FilesystemId", x => x.FilesystemId,
                                 "Filesystems", "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_FilesystemsByMediaDumpFile_MediaDumpFileImages_MediaDumpFile~",
                                 x => x.MediaDumpFileImageId, "MediaDumpFileImages", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_FilesystemsByMediaDumpFile_FilesystemId", "FilesystemsByMediaDumpFile",
                                         "FilesystemId");

            migrationBuilder.CreateIndex("IX_FilesystemsByMediaDumpFile_MediaDumpFileImageId",
                                         "FilesystemsByMediaDumpFile", "MediaDumpFileImageId");

            migrationBuilder.CreateIndex("IX_MediaDumpFileImages_Md5", "MediaDumpFileImages", "Md5");

            migrationBuilder.CreateIndex("IX_MediaDumpFileImages_MediaDumpId", "MediaDumpFileImages", "MediaDumpId");

            migrationBuilder.CreateIndex("IX_MediaDumpFileImages_Sha1", "MediaDumpFileImages", "Sha1");

            migrationBuilder.CreateIndex("IX_MediaDumpFileImages_Sha256", "MediaDumpFileImages", "Sha256");

            migrationBuilder.CreateIndex("IX_MediaDumpFileImages_Sha3", "MediaDumpFileImages", "Sha3");

            migrationBuilder.CreateIndex("IX_MediaDumpFileImages_Size", "MediaDumpFileImages", "Size");

            migrationBuilder.CreateIndex("IX_MediaDumpFileImages_Spamsum", "MediaDumpFileImages", "Spamsum");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("FilesystemsByMediaDumpFile");

            migrationBuilder.DropTable("MediaDumpFileImages");
        }
    }
}
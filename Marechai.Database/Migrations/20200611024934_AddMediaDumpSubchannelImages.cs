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
    public partial class AddMediaDumpSubchannelImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("MediaDumpSubchannelImages", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                TrackSequence = table.Column<short>(nullable: false), Status = table.Column<byte>(nullable: false),
                Size = table.Column<ulong>(nullable: false), Md5 = table.Column<byte[]>("binary(16)", nullable: true),
                Sha1 = table.Column<byte[]>("binary(20)", nullable: true),
                Sha256 = table.Column<byte[]>("binary(32)", nullable: true),
                Sha3 = table.Column<byte[]>("binary(64)", nullable: true),
                Spamsum = table.Column<string>(nullable: true), MediaDumpId = table.Column<ulong>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_MediaDumpSubchannelImages", x => x.Id);

                table.ForeignKey("FK_MediaDumpSubchannelImages_MediaDumps_MediaDumpId", x => x.MediaDumpId,
                                 "MediaDumps", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_MediaDumpSubchannelImages_Md5", "MediaDumpSubchannelImages", "Md5");

            migrationBuilder.CreateIndex("IX_MediaDumpSubchannelImages_MediaDumpId", "MediaDumpSubchannelImages",
                                         "MediaDumpId", unique: true);

            migrationBuilder.CreateIndex("IX_MediaDumpSubchannelImages_Sha1", "MediaDumpSubchannelImages", "Sha1");

            migrationBuilder.CreateIndex("IX_MediaDumpSubchannelImages_Sha256", "MediaDumpSubchannelImages", "Sha256");

            migrationBuilder.CreateIndex("IX_MediaDumpSubchannelImages_Sha3", "MediaDumpSubchannelImages", "Sha3");

            migrationBuilder.CreateIndex("IX_MediaDumpSubchannelImages_Size", "MediaDumpSubchannelImages", "Size");

            migrationBuilder.CreateIndex("IX_MediaDumpSubchannelImages_Spamsum", "MediaDumpSubchannelImages",
                                         "Spamsum");
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable("MediaDumpSubchannelImages");
    }
}
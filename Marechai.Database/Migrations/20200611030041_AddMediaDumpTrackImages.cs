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
    public partial class AddMediaDumpTrackImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>("TrackId", "MediaDumpSubchannelImages", nullable: true);

            migrationBuilder.CreateTable("MediaDumpTrackImages", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                TrackSequence = table.Column<short>(nullable: false),
                Format        = table.Column<string>(nullable: true),
                Size          = table.Column<ulong>(nullable: false),
                Md5           = table.Column<byte[]>("binary(16)", nullable: true),
                Sha1          = table.Column<byte[]>("binary(20)", nullable: true),
                Sha256        = table.Column<byte[]>("binary(32)", nullable: true),
                Sha3          = table.Column<byte[]>("binary(64)", nullable: true),
                Spamsum       = table.Column<string>(nullable: true),
                MediaDumpId   = table.Column<ulong>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_MediaDumpTrackImages", x => x.Id);

                table.ForeignKey("FK_MediaDumpTrackImages_MediaDumps_MediaDumpId", x => x.MediaDumpId, "MediaDumps",
                                 "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_MediaDumpSubchannelImages_TrackId", "MediaDumpSubchannelImages", "TrackId",
                                         unique: true);

            migrationBuilder.CreateIndex("IX_MediaDumpTrackImages_Format", "MediaDumpTrackImages", "Format");

            migrationBuilder.CreateIndex("IX_MediaDumpTrackImages_Md5", "MediaDumpTrackImages", "Md5");

            migrationBuilder.CreateIndex("IX_MediaDumpTrackImages_MediaDumpId", "MediaDumpTrackImages", "MediaDumpId");

            migrationBuilder.CreateIndex("IX_MediaDumpTrackImages_Sha1", "MediaDumpTrackImages", "Sha1");

            migrationBuilder.CreateIndex("IX_MediaDumpTrackImages_Sha256", "MediaDumpTrackImages", "Sha256");

            migrationBuilder.CreateIndex("IX_MediaDumpTrackImages_Sha3", "MediaDumpTrackImages", "Sha3");

            migrationBuilder.CreateIndex("IX_MediaDumpTrackImages_Size", "MediaDumpTrackImages", "Size");

            migrationBuilder.CreateIndex("IX_MediaDumpTrackImages_Spamsum", "MediaDumpTrackImages", "Spamsum");

            migrationBuilder.AddForeignKey("FK_MediaDumpSubchannelImages_MediaDumpTrackImages_TrackId",
                                           "MediaDumpSubchannelImages", "TrackId", "MediaDumpTrackImages",
                                           principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_MediaDumpSubchannelImages_MediaDumpTrackImages_TrackId",
                                            "MediaDumpSubchannelImages");

            migrationBuilder.DropTable("MediaDumpTrackImages");

            migrationBuilder.DropIndex("IX_MediaDumpSubchannelImages_TrackId", "MediaDumpSubchannelImages");

            migrationBuilder.DropColumn("TrackId", "MediaDumpSubchannelImages");
        }
    }
}
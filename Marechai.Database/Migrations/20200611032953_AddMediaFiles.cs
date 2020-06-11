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
    public partial class AddMediaFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* TODO: For MariaDB >=10.5.2
                migrationBuilder.RenameColumn(
                name: "Files",
                table: "Filesystems", newName: "FilesCount");
             */

            migrationBuilder.DropColumn("Files", "Filesystems");

            migrationBuilder.AddColumn<ulong>("FilesCount", "Filesystems", nullable: true);

            migrationBuilder.CreateTable("MediaFiles", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Path = table.Column<string>(maxLength: 8192, nullable: false),
                Name = table.Column<string>(maxLength: 255, nullable: false),
                PathSeparator = table.Column<string>(nullable: false),
                IsDirectory = table.Column<bool>(nullable: false),
                CreationDate = table.Column<DateTime>(nullable: true),
                AccessDate = table.Column<DateTime>(nullable: true),
                StatusChangeDate = table.Column<DateTime>(nullable: true),
                BackupDate = table.Column<DateTime>(nullable: true),
                LastWriteDate = table.Column<DateTime>(nullable: true),
                Attributes = table.Column<ulong>(nullable: false), PosixMode = table.Column<ushort>(nullable: true),
                DeviceNumber = table.Column<uint>(nullable: true), GroupId = table.Column<ulong>(nullable: true),
                UserId = table.Column<ulong>(nullable: true), Inode = table.Column<ulong>(nullable: true),
                Links = table.Column<ulong>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_MediaFiles", x => x.Id);
            });

            migrationBuilder.CreateTable("FileDataStreamsByMediaFile", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                FileDataStreamId = table.Column<ulong>(nullable: false),
                MediaFileId      = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_FileDataStreamsByMediaFile", x => x.Id);

                table.ForeignKey("FK_FileDataStreamsByMediaFile_FileDataStreams_FileDataStreamId",
                                 x => x.FileDataStreamId, "FileDataStreams", "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_FileDataStreamsByMediaFile_MediaFiles_MediaFileId", x => x.MediaFileId,
                                 "MediaFiles", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("FilesByFilesystem", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                FilesystemId = table.Column<ulong>(nullable: false), FileId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_FilesByFilesystem", x => x.Id);

                table.ForeignKey("FK_FilesByFilesystem_MediaFiles_FileId", x => x.FileId, "MediaFiles", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_FilesByFilesystem_Filesystems_FilesystemId", x => x.FilesystemId, "Filesystems",
                                 "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_FileDataStreamsByMediaFile_FileDataStreamId", "FileDataStreamsByMediaFile",
                                         "FileDataStreamId");

            migrationBuilder.CreateIndex("IX_FileDataStreamsByMediaFile_MediaFileId", "FileDataStreamsByMediaFile",
                                         "MediaFileId");

            migrationBuilder.CreateIndex("IX_FilesByFilesystem_FileId", "FilesByFilesystem", "FileId");

            migrationBuilder.CreateIndex("IX_FilesByFilesystem_FilesystemId", "FilesByFilesystem", "FilesystemId");

            migrationBuilder.CreateIndex("IX_MediaFiles_AccessDate", "MediaFiles", "AccessDate");

            migrationBuilder.CreateIndex("IX_MediaFiles_BackupDate", "MediaFiles", "BackupDate");

            migrationBuilder.CreateIndex("IX_MediaFiles_CreationDate", "MediaFiles", "CreationDate");

            migrationBuilder.CreateIndex("IX_MediaFiles_GroupId", "MediaFiles", "GroupId");

            migrationBuilder.CreateIndex("IX_MediaFiles_IsDirectory", "MediaFiles", "IsDirectory");

            migrationBuilder.CreateIndex("IX_MediaFiles_LastWriteDate", "MediaFiles", "LastWriteDate");

            migrationBuilder.CreateIndex("IX_MediaFiles_Name", "MediaFiles", "Name");

            migrationBuilder.CreateIndex("IX_MediaFiles_Path", "MediaFiles", "Path");

            migrationBuilder.CreateIndex("IX_MediaFiles_StatusChangeDate", "MediaFiles", "StatusChangeDate");

            migrationBuilder.CreateIndex("IX_MediaFiles_UserId", "MediaFiles", "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("FileDataStreamsByMediaFile");

            migrationBuilder.DropTable("FilesByFilesystem");

            migrationBuilder.DropTable("MediaFiles");

            migrationBuilder.DropColumn("FilesCount", "Filesystems");

            migrationBuilder.AddColumn<ulong>("Files", "Filesystems", "bigint unsigned", nullable: true);
        }
    }
}
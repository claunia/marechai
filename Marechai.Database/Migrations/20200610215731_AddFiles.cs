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
    public partial class AddFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Files", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Size      = table.Column<ulong>(nullable: false),
                Md5       = table.Column<byte[]>("binary(16)", nullable: true),
                Sha1      = table.Column<byte[]>("binary(20)", nullable: true),
                Sha256    = table.Column<byte[]>("binary(32)", nullable: true),
                Sha3      = table.Column<byte[]>("binary(64)", nullable: true),
                Spamsum   = table.Column<string>(nullable: true),
                Mime      = table.Column<string>(nullable: true),
                Magic     = table.Column<string>(nullable: true),
                AccoustId = table.Column<string>(nullable: true),
                Infected  = table.Column<bool>(nullable: false),
                Malware   = table.Column<string>(nullable: true),
                Hack      = table.Column<bool>(nullable: false),
                HackGroup = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Files", x => x.Id);
            });

            migrationBuilder.CreateIndex("IX_Files_AccoustId", "Files", "AccoustId");

            migrationBuilder.CreateIndex("IX_Files_Hack", "Files", "Hack");

            migrationBuilder.CreateIndex("IX_Files_HackGroup", "Files", "HackGroup");

            migrationBuilder.CreateIndex("IX_Files_Infected", "Files", "Infected");

            migrationBuilder.CreateIndex("IX_Files_Magic", "Files", "Magic");

            migrationBuilder.CreateIndex("IX_Files_Malware", "Files", "Malware");

            migrationBuilder.CreateIndex("IX_Files_Md5", "Files", "Md5");

            migrationBuilder.CreateIndex("IX_Files_Mime", "Files", "Mime");

            migrationBuilder.CreateIndex("IX_Files_Sha1", "Files", "Sha1");

            migrationBuilder.CreateIndex("IX_Files_Sha256", "Files", "Sha256");

            migrationBuilder.CreateIndex("IX_Files_Sha3", "Files", "Sha3");

            migrationBuilder.CreateIndex("IX_Files_Size", "Files", "Size");

            migrationBuilder.CreateIndex("IX_Files_Spamsum", "Files", "Spamsum");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("Files");
    }
}
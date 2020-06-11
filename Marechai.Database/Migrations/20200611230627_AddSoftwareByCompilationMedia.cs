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
    public partial class AddSoftwareByCompilationMedia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("SoftwareVariantByCompilationMedia", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Path = table.Column<string>(nullable: true), PathSeparator = table.Column<string>(nullable: false),
                SoftwareVariantId = table.Column<ulong>(nullable: false), MediaId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_SoftwareVariantByCompilationMedia", x => x.Id);

                table.ForeignKey("FK_SoftwareVariantByCompilationMedia_Media_MediaId", x => x.MediaId, "Media", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_SoftwareVariantByCompilationMedia_SoftwareVariants_SoftwareV~",
                                 x => x.SoftwareVariantId, "SoftwareVariants", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_SoftwareVariantByCompilationMedia_MediaId",
                                         "SoftwareVariantByCompilationMedia", "MediaId");

            migrationBuilder.CreateIndex("IX_SoftwareVariantByCompilationMedia_Path",
                                         "SoftwareVariantByCompilationMedia", "Path");

            migrationBuilder.CreateIndex("IX_SoftwareVariantByCompilationMedia_SoftwareVariantId",
                                         "SoftwareVariantByCompilationMedia", "SoftwareVariantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable("SoftwareVariantByCompilationMedia");
    }
}
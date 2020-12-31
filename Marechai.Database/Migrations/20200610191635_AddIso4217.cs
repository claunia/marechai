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
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddIso4217 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Iso4217", table => new
            {
                Code       = table.Column<string>(maxLength: 3, nullable: false),
                Numeric    = table.Column<short>("smallint(3)", nullable: false),
                MinorUnits = table.Column<byte>(nullable: true),
                Name       = table.Column<string>(maxLength: 150, nullable: false),
                Withdrawn  = table.Column<DateTime>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Iso4217", x => x.Code);
            });

            migrationBuilder.CreateIndex("IX_Iso4217_Numeric", "Iso4217", "Numeric");

            migrationBuilder.CreateIndex("IX_Iso4217_Withdrawn", "Iso4217", "Withdrawn");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("Iso4217");
    }
}
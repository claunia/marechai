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
    public partial class AddCurrencyPegging : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("CurrenciesPegging", table => new
            {
                Id = table.Column<int>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                SourceCode      = table.Column<string>(nullable: false),
                DestinationCode = table.Column<string>(nullable: false),
                Ratio           = table.Column<float>(nullable: false),
                Start           = table.Column<DateTime>(nullable: false),
                End             = table.Column<DateTime>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_CurrenciesPegging", x => x.Id);

                table.ForeignKey("FK_CurrenciesPegging_Iso4217_DestinationCode", x => x.DestinationCode, "Iso4217",
                                 "Code", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_CurrenciesPegging_Iso4217_SourceCode", x => x.SourceCode, "Iso4217", "Code",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_CurrenciesPegging_DestinationCode", "CurrenciesPegging",
                                         "DestinationCode");

            migrationBuilder.CreateIndex("IX_CurrenciesPegging_End", "CurrenciesPegging", "End");

            migrationBuilder.CreateIndex("IX_CurrenciesPegging_SourceCode", "CurrenciesPegging", "SourceCode");

            migrationBuilder.CreateIndex("IX_CurrenciesPegging_Start", "CurrenciesPegging", "Start");
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable("CurrenciesPegging");
    }
}
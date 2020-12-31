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

using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddCoverdiscs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>("MagazineIssueId", "Media", nullable: true);

            migrationBuilder.CreateIndex("IX_Media_MagazineIssueId", "Media", "MagazineIssueId");

            migrationBuilder.AddForeignKey("FK_Media_MagazineIssues_MagazineIssueId", "Media", "MagazineIssueId",
                                           "MagazineIssues", principalColumn: "Id",
                                           onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_Media_MagazineIssues_MagazineIssueId", "Media");

            migrationBuilder.DropIndex("IX_Media_MagazineIssueId", "Media");

            migrationBuilder.DropColumn("MagazineIssueId", "Media");
        }
    }
}
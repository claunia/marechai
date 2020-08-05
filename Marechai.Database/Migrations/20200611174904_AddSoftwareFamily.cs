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
    public partial class AddSoftwareFamily : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("SoftwareFamilies", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Name       = table.Column<string>(nullable: false),
                Introduced = table.Column<DateTime>(nullable: true),
                ParentId   = table.Column<ulong>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_SoftwareFamilies", x => x.Id);

                table.ForeignKey("FK_SoftwareFamilies_SoftwareFamilies_ParentId", x => x.ParentId, "SoftwareFamilies",
                                 "Id", onDelete: ReferentialAction.SetNull);
            });

            migrationBuilder.CreateTable("CompaniesBySoftwareFamily", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                RoleId           = table.Column<string>(nullable: false),
                CompanyId        = table.Column<int>(nullable: false),
                SoftwareFamilyId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_CompaniesBySoftwareFamily", x => x.Id);

                table.ForeignKey("FK_CompaniesBySoftwareFamily_companies_CompanyId", x => x.CompanyId, "companies",
                                 "id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_CompaniesBySoftwareFamily_DocumentRoles_RoleId", x => x.RoleId, "DocumentRoles",
                                 "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_CompaniesBySoftwareFamily_SoftwareFamilies_SoftwareFamilyId",
                                 x => x.SoftwareFamilyId, "SoftwareFamilies", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("PeopleBySoftwareFamily", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                RoleId           = table.Column<string>(nullable: false),
                PersonId         = table.Column<int>(nullable: false),
                SoftwareFamilyId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_PeopleBySoftwareFamily", x => x.Id);

                table.ForeignKey("FK_PeopleBySoftwareFamily_People_PersonId", x => x.PersonId, "People", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_PeopleBySoftwareFamily_DocumentRoles_RoleId", x => x.RoleId, "DocumentRoles", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_PeopleBySoftwareFamily_SoftwareFamilies_SoftwareFamilyId", x => x.SoftwareFamilyId,
                                 "SoftwareFamilies", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_CompaniesBySoftwareFamily_CompanyId", "CompaniesBySoftwareFamily",
                                         "CompanyId");

            migrationBuilder.CreateIndex("IX_CompaniesBySoftwareFamily_RoleId", "CompaniesBySoftwareFamily", "RoleId");

            migrationBuilder.CreateIndex("IX_CompaniesBySoftwareFamily_SoftwareFamilyId", "CompaniesBySoftwareFamily",
                                         "SoftwareFamilyId");

            migrationBuilder.CreateIndex("IX_PeopleBySoftwareFamily_PersonId", "PeopleBySoftwareFamily", "PersonId");

            migrationBuilder.CreateIndex("IX_PeopleBySoftwareFamily_RoleId", "PeopleBySoftwareFamily", "RoleId");

            migrationBuilder.CreateIndex("IX_PeopleBySoftwareFamily_SoftwareFamilyId", "PeopleBySoftwareFamily",
                                         "SoftwareFamilyId");

            migrationBuilder.CreateIndex("IX_SoftwareFamilies_Introduced", "SoftwareFamilies", "Introduced");

            migrationBuilder.CreateIndex("IX_SoftwareFamilies_Name", "SoftwareFamilies", "Name");

            migrationBuilder.CreateIndex("IX_SoftwareFamilies_ParentId", "SoftwareFamilies", "ParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("CompaniesBySoftwareFamily");

            migrationBuilder.DropTable("PeopleBySoftwareFamily");

            migrationBuilder.DropTable("SoftwareFamilies");
        }
    }
}
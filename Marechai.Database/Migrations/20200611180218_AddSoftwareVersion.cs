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
    public partial class AddSoftwareVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("SoftwareVersion", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                FamilyId   = table.Column<ulong>(nullable: false),
                Name       = table.Column<string>(nullable: true),
                Codename   = table.Column<string>(nullable: true),
                Version    = table.Column<string>(nullable: false),
                Introduced = table.Column<DateTime>(nullable: true),
                LicenseId  = table.Column<int>(nullable: true),
                PreviousId = table.Column<ulong>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_SoftwareVersion", x => x.Id);

                table.ForeignKey("FK_SoftwareVersion_SoftwareFamilies_FamilyId", x => x.FamilyId, "SoftwareFamilies",
                                 "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_SoftwareVersion_Licenses_LicenseId", x => x.LicenseId, "Licenses", "Id",
                                 onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_SoftwareVersion_SoftwareVersion_PreviousId", x => x.PreviousId, "SoftwareVersion",
                                 "Id", onDelete: ReferentialAction.SetNull);
            });

            migrationBuilder.CreateTable("CompaniesBySoftwareVersion", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                CompanyId         = table.Column<int>(nullable: false),
                SoftwareVersionId = table.Column<ulong>(nullable: false),
                RoleId            = table.Column<string>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_CompaniesBySoftwareVersion", x => x.Id);

                table.ForeignKey("FK_CompaniesBySoftwareVersion_companies_CompanyId", x => x.CompanyId, "companies",
                                 "id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_CompaniesBySoftwareVersion_DocumentRoles_RoleId", x => x.RoleId, "DocumentRoles",
                                 "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_CompaniesBySoftwareVersion_SoftwareVersion_SoftwareVersionId",
                                 x => x.SoftwareVersionId, "SoftwareVersion", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("PeopleBySoftwareVersion", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                SoftwareVersionId = table.Column<ulong>(nullable: false),
                PersonId          = table.Column<int>(nullable: false),
                RoleId            = table.Column<string>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_PeopleBySoftwareVersion", x => x.Id);

                table.ForeignKey("FK_PeopleBySoftwareVersion_People_PersonId", x => x.PersonId, "People", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_PeopleBySoftwareVersion_DocumentRoles_RoleId", x => x.RoleId, "DocumentRoles",
                                 "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_PeopleBySoftwareVersion_SoftwareVersion_SoftwareVersionId",
                                 x => x.SoftwareVersionId, "SoftwareVersion", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_CompaniesBySoftwareVersion_CompanyId", "CompaniesBySoftwareVersion",
                                         "CompanyId");

            migrationBuilder.CreateIndex("IX_CompaniesBySoftwareVersion_RoleId", "CompaniesBySoftwareVersion",
                                         "RoleId");

            migrationBuilder.CreateIndex("IX_CompaniesBySoftwareVersion_SoftwareVersionId",
                                         "CompaniesBySoftwareVersion", "SoftwareVersionId");

            migrationBuilder.CreateIndex("IX_PeopleBySoftwareVersion_PersonId", "PeopleBySoftwareVersion", "PersonId");

            migrationBuilder.CreateIndex("IX_PeopleBySoftwareVersion_RoleId", "PeopleBySoftwareVersion", "RoleId");

            migrationBuilder.CreateIndex("IX_PeopleBySoftwareVersion_SoftwareVersionId", "PeopleBySoftwareVersion",
                                         "SoftwareVersionId");

            migrationBuilder.CreateIndex("IX_SoftwareVersion_Codename", "SoftwareVersion", "Codename");

            migrationBuilder.CreateIndex("IX_SoftwareVersion_FamilyId", "SoftwareVersion", "FamilyId");

            migrationBuilder.CreateIndex("IX_SoftwareVersion_Introduced", "SoftwareVersion", "Introduced");

            migrationBuilder.CreateIndex("IX_SoftwareVersion_LicenseId", "SoftwareVersion", "LicenseId");

            migrationBuilder.CreateIndex("IX_SoftwareVersion_Name", "SoftwareVersion", "Name");

            migrationBuilder.CreateIndex("IX_SoftwareVersion_PreviousId", "SoftwareVersion", "PreviousId",
                                         unique: true);

            migrationBuilder.CreateIndex("IX_SoftwareVersion_Version", "SoftwareVersion", "Version");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("CompaniesBySoftwareVersion");

            migrationBuilder.DropTable("PeopleBySoftwareVersion");

            migrationBuilder.DropTable("SoftwareVersion");
        }
    }
}
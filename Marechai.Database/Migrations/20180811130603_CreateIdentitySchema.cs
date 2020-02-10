/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : 20180811130603_CreateIdentitySchema.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Adds ASP.NET Identity tables
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
    public partial class CreateIdentitySchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("AspNetRoles", table => new
            {
                Id               = table.Column<string>(), Name = table.Column<string>(maxLength: 256, nullable: true),
                NormalizedName   = table.Column<string>(maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            });

            migrationBuilder.CreateTable("AspNetUsers", table => new
            {
                Id                 = table.Column<string>(),
                UserName           = table.Column<string>(maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                Email              = table.Column<string>(maxLength: 256, nullable: true),
                NormalizedEmail    = table.Column<string>(maxLength: 256, nullable: true),
                EmailConfirmed     = table.Column<bool>(), PasswordHash = table.Column<string>(nullable: true),
                SecurityStamp      = table.Column<string>(nullable: true),
                ConcurrencyStamp   = table.Column<string>(nullable: true),
                PhoneNumber        = table.Column<string>(nullable: true), PhoneNumberConfirmed = table.Column<bool>(),
                TwoFactorEnabled   = table.Column<bool>(),
                LockoutEnd         = table.Column<DateTimeOffset>(nullable: true),
                LockoutEnabled     = table.Column<bool>(), AccessFailedCount = table.Column<int>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            });

            migrationBuilder.CreateTable("AspNetRoleClaims", table => new
            {
                Id = table.Column<int>().
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                RoleId     = table.Column<string>(), ClaimType = table.Column<string>(nullable: true),
                ClaimValue = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);

                table.ForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", x => x.RoleId, "AspNetRoles", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("AspNetUserClaims", table => new
            {
                Id = table.Column<int>().
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                UserId     = table.Column<string>(), ClaimType = table.Column<string>(nullable: true),
                ClaimValue = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);

                table.ForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("AspNetUserLogins", table => new
            {
                LoginProvider       = table.Column<string>(maxLength: 128),
                ProviderKey         = table.Column<string>(maxLength: 128),
                ProviderDisplayName = table.Column<string>(nullable: true), UserId = table.Column<string>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new
                {
                    x.LoginProvider, x.ProviderKey
                });

                table.ForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("AspNetUserRoles", table => new
            {
                UserId = table.Column<string>(), RoleId = table.Column<string>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new
                {
                    x.UserId, x.RoleId
                });

                table.ForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", x => x.RoleId, "AspNetRoles", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("AspNetUserTokens", table => new
            {
                UserId = table.Column<string>(), LoginProvider       = table.Column<string>(maxLength: 128),
                Name   = table.Column<string>(maxLength: 128), Value = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new
                {
                    x.UserId, x.LoginProvider, x.Name
                });

                table.ForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_AspNetRoleClaims_RoleId", "AspNetRoleClaims", "RoleId");

            migrationBuilder.CreateIndex("RoleNameIndex", "AspNetRoles", "NormalizedName", unique: true);

            migrationBuilder.CreateIndex("IX_AspNetUserClaims_UserId", "AspNetUserClaims", "UserId");

            migrationBuilder.CreateIndex("IX_AspNetUserLogins_UserId", "AspNetUserLogins", "UserId");

            migrationBuilder.CreateIndex("IX_AspNetUserRoles_RoleId", "AspNetUserRoles", "RoleId");

            migrationBuilder.CreateIndex("EmailIndex", "AspNetUsers", "NormalizedEmail");

            migrationBuilder.CreateIndex("UserNameIndex", "AspNetUsers", "NormalizedUserName", unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("AspNetRoleClaims");

            migrationBuilder.DropTable("AspNetUserClaims");

            migrationBuilder.DropTable("AspNetUserLogins");

            migrationBuilder.DropTable("AspNetUserRoles");

            migrationBuilder.DropTable("AspNetUserTokens");

            migrationBuilder.DropTable("AspNetRoles");

            migrationBuilder.DropTable("AspNetUsers");
        }
    }
}
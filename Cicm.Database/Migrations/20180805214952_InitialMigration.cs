/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : 20180805214952_InitialMigration.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains the initial migration from MySQL to Entity Framework.
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
// Copyright © 2003-2018 Natalia Portillo
*******************************************************************************/

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("admins",
                                         table => new
                                         {
                                             id = table.Column<int>("int(11)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             user =
                                                 table.Column<string>("char(50)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             password = table.Column<string>("char(50)", nullable: false,
                                                                             defaultValueSql: "''")
                                         }, constraints: table => { table.PrimaryKey("PK_admins", x => x.id); });

            migrationBuilder.CreateTable("browser_tests",
                                         table => new
                                         {
                                             id =
                                                 table.Column<int>("int(11)", nullable: false)
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.IdentityColumn),
                                             user_agent =
                                                 table.Column<string>("varchar(128)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             browser =
                                                 table.Column<string>("varchar(64)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             version =
                                                 table.Column<string>("varchar(16)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             os =
                                                 table.Column<string>("varchar(32)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             platform =
                                                 table.Column<string>("varchar(8)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             gif87 =
                                                 table.Column<sbyte>("tinyint(1)", nullable: false,
                                                                     defaultValueSql: "'0'"),
                                             gif89 =
                                                 table.Column<sbyte>("tinyint(1)", nullable: false,
                                                                     defaultValueSql: "'0'"),
                                             jpeg =
                                                 table.Column<sbyte>("tinyint(1)", nullable: false,
                                                                     defaultValueSql: "'0'"),
                                             png =
                                                 table.Column<sbyte>("tinyint(1)", nullable: false,
                                                                     defaultValueSql: "'0'"),
                                             pngt =
                                                 table.Column<sbyte>("tinyint(1)", nullable: false,
                                                                     defaultValueSql: "'0'"),
                                             agif =
                                                 table.Column<sbyte>("tinyint(1)", nullable: false,
                                                                     defaultValueSql: "'0'"),
                                             table =
                                                 table.Column<sbyte>("tinyint(1)", nullable: false,
                                                                     defaultValueSql: "'0'"),
                                             colors =
                                                 table.Column<sbyte>("tinyint(1)", nullable: false,
                                                                     defaultValueSql: "'0'"),
                                             js =
                                                 table.Column<sbyte>("tinyint(1)", nullable: false,
                                                                     defaultValueSql: "'0'"),
                                             frames =
                                                 table.Column<sbyte>("tinyint(1)", nullable: false,
                                                                     defaultValueSql: "'0'"),
                                             flash = table.Column<sbyte>("tinyint(1)", nullable: false,
                                                                         defaultValueSql: "'0'")
                                         }, constraints: table => { table.PrimaryKey("PK_browser_tests", x => x.id); });

            migrationBuilder.CreateTable("cicm_db",
                                         table => new
                                         {
                                             id = table.Column<int>("int(11)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             version = table.Column<int>("int(11)", nullable: false),
                                             updated = table.Column<DateTime>("datetime", nullable: true,
                                                                              defaultValueSql: "'CURRENT_TIMESTAMP'")
                                         }, constraints: table => { table.PrimaryKey("PK_cicm_db", x => x.id); });

            migrationBuilder.CreateTable("forbidden",
                                         table => new
                                         {
                                             id =
                                                 table.Column<int>("int(11)", nullable: false)
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.IdentityColumn),
                                             browser =
                                                 table.Column<string>("char(128)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             date =
                                                 table.Column<string>("char(20)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             ip = table.Column<string>("char(16)", nullable: false,
                                                                       defaultValueSql: "''"),
                                             referer = table.Column<string>("char(255)", nullable: false,
                                                                            defaultValueSql: "''")
                                         }, constraints: table => { table.PrimaryKey("PK_forbidden", x => x.id); });

            migrationBuilder.CreateTable("instruction_set_extensions",
                                         table => new
                                         {
                                             id = table.Column<int>("int(11)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             extension = table.Column<string>("varchar(45)", nullable: false)
                                         },
                                         constraints: table =>
                                         {
                                             table.PrimaryKey("PK_instruction_set_extensions", x => x.id);
                                         });

            migrationBuilder.CreateTable("instruction_sets",
                                         table => new
                                         {
                                             id = table.Column<int>("int(11)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             instruction_set = table.Column<string>("varchar(45)", nullable: false)
                                         },
                                         constraints: table => { table.PrimaryKey("PK_instruction_sets", x => x.id); });

            migrationBuilder.CreateTable("iso3166_1_numeric",
                                         table => new
                                         {
                                             id = table.Column<short>("smallint(3)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             name = table.Column<string>("varchar(64)", nullable: false)
                                         },
                                         constraints: table =>
                                         {
                                             table.PrimaryKey("PK_iso3166_1_numeric", x => x.id);
                                         });

            migrationBuilder.CreateTable("log",
                                         table => new
                                         {
                                             id =
                                                 table.Column<int>("int(11)", nullable: false)
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.IdentityColumn),
                                             browser =
                                                 table.Column<string>("char(128)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             ip =
                                                 table.Column<string>("char(16)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             date =
                                                 table.Column<string>("char(20)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             referer = table.Column<string>("char(255)", nullable: false,
                                                                            defaultValueSql: "''")
                                         }, constraints: table => { table.PrimaryKey("PK_log", x => x.id); });

            migrationBuilder.CreateTable("money_donations",
                                         table => new
                                         {
                                             id = table.Column<int>("int(11)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             donator =
                                                 table.Column<string>("char(128)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             quantity = table.Column<decimal>("decimal(11,2)", nullable: false,
                                                                              defaultValueSql: "'0.00'")
                                         },
                                         constraints: table => { table.PrimaryKey("PK_money_donations", x => x.id); });

            migrationBuilder.CreateTable("news",
                                         table => new
                                         {
                                             id = table.Column<int>("int(11)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             date =
                                                 table.Column<string>("char(20)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             type =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             added_id = table.Column<int>("int(11)", nullable: false,
                                                                          defaultValueSql: "'0'")
                                         }, constraints: table => { table.PrimaryKey("PK_news", x => x.id); });

            migrationBuilder.CreateTable("owned_computers",
                                         table => new
                                         {
                                             id =
                                                 table.Column<int>("int(11)", nullable: false)
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.IdentityColumn),
                                             db_id =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             date =
                                                 table.Column<string>("varchar(20)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             status =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             trade =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             boxed =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             manuals =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             cpu1 =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             mhz1 =
                                                 table.Column<decimal>("decimal(10,0)", nullable: false,
                                                                       defaultValueSql: "'0'"),
                                             cpu2 =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             mhz2 =
                                                 table.Column<decimal>("decimal(10,0)", nullable: false,
                                                                       defaultValueSql: "'0'"),
                                             ram =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             vram =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             rigid =
                                                 table.Column<string>("varchar(64)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             disk1 =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             cap1 =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             disk2 =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             cap2 = table.Column<int>("int(11)", nullable: false,
                                                                      defaultValueSql: "'0'")
                                         },
                                         constraints: table => { table.PrimaryKey("PK_owned_computers", x => x.id); });

            migrationBuilder.CreateTable("owned_consoles",
                                         table => new
                                         {
                                             id =
                                                 table.Column<int>("int(11)", nullable: false)
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.IdentityColumn),
                                             db_id =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             date =
                                                 table.Column<string>("char(20)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             status =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             trade =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             boxed =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             manuals = table.Column<int>("int(11)", nullable: false,
                                                                         defaultValueSql: "'0'")
                                         }, constraints: table => { table.PrimaryKey("PK_owned_consoles", x => x.id); });

            migrationBuilder.CreateTable("resolutions",
                                         table => new
                                         {
                                             id =
                                                 table.Column<int>("int(11)", nullable: false)
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.IdentityColumn),
                                             width =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             height =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             colors  = table.Column<long>("bigint(20)", nullable: true),
                                             palette = table.Column<long>("bigint(20)", nullable: true),
                                             chars = table.Column<sbyte>("tinyint(1)", nullable: false,
                                                                         defaultValueSql: "'0'")
                                         }, constraints: table => { table.PrimaryKey("PK_resolutions", x => x.id); });

            migrationBuilder.CreateTable("companies",
                                         table => new
                                         {
                                             id =
                                                 table.Column<int>("int(11)", nullable: false)
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.IdentityColumn),
                                             name =
                                                 table.Column<string>("varchar(128)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             founded     = table.Column<DateTime>("datetime", nullable: true),
                                             website     = table.Column<string>("varchar(255)", nullable: true),
                                             twitter     = table.Column<string>("varchar(45)",  nullable: true),
                                             facebook    = table.Column<string>("varchar(45)",  nullable: true),
                                             sold        = table.Column<DateTime>("datetime", nullable: true),
                                             sold_to     = table.Column<int>("int(11)", nullable: true),
                                             address     = table.Column<string>("varchar(80)", nullable: true),
                                             city        = table.Column<string>("varchar(80)", nullable: true),
                                             province    = table.Column<string>("varchar(80)", nullable: true),
                                             postal_code = table.Column<string>("varchar(25)", nullable: true),
                                             country     = table.Column<short>("smallint(3)", nullable: true),
                                             status      = table.Column<int>("int(11)", nullable: false)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_companies", x => x.id);
                                             table.ForeignKey("fk_companies_country", x => x.country,
                                                              "iso3166_1_numeric", "id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("fk_companies_sold_to", x => x.sold_to, "companies", "id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("company_descriptions",
                                         table => new
                                         {
                                             id = table.Column<int>("int(11)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             company_id = table.Column<int>("int(11)", nullable: false),
                                             text       = table.Column<string>("text", nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_company_descriptions", x => x.id);
                                             table.ForeignKey("fk_company_id", x => x.id, "companies", "id",
                                                              onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("company_logos",
                                         table => new
                                         {
                                             id = table.Column<int>("int(11)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             company_id = table.Column<int>("int(11)", nullable: false),
                                             year       = table.Column<int>("int(4)",  nullable: true),
                                             logo_guid  = table.Column<string>("char(36)", nullable: false)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_company_logos",
                                                              x => new {x.id, x.company_id, x.logo_guid});
                                             table.ForeignKey("fk_company_logos_company1", x => x.company_id,
                                                              "companies", "id", onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("gpus",
                                         table => new
                                         {
                                             id =
                                                 table.Column<int>("int(11)", nullable: false)
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.IdentityColumn),
                                             name =
                                                 table.Column<string>("char(128)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             company     = table.Column<int>("int(11)", nullable: true),
                                             model_code  = table.Column<string>("varchar(45)", nullable: true),
                                             introduced  = table.Column<DateTime>("datetime", nullable: true),
                                             package     = table.Column<string>("varchar(45)", nullable: true),
                                             process     = table.Column<string>("varchar(45)", nullable: true),
                                             process_nm  = table.Column<float>(nullable: true),
                                             die_size    = table.Column<float>(nullable: true),
                                             transistors = table.Column<long>("bigint(20)", nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_gpus", x => x.id);
                                             table.ForeignKey("fk_gpus_company", x => x.company, "companies", "id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("machine_families",
                                         table => new
                                         {
                                             id = table.Column<int>("int(11)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             company = table.Column<int>("int(11)", nullable: false),
                                             name    = table.Column<string>("varchar(255)", nullable: false)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_machine_families", x => x.id);
                                             table.ForeignKey("fk_machine_families_company", x => x.company,
                                                              "companies", "id", onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("processors",
                                         table => new
                                         {
                                             id =
                                                 table.Column<int>("int(11)", nullable: false)
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.IdentityColumn),
                                             name =
                                                 table.Column<string>("char(50)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             company          = table.Column<int>("int(11)", nullable: true),
                                             model_code       = table.Column<string>("varchar(45)", nullable: true),
                                             introduced       = table.Column<DateTime>("datetime", nullable: true),
                                             instruction_set  = table.Column<int>("int(11)", nullable: true),
                                             speed            = table.Column<double>(nullable: true),
                                             package          = table.Column<string>("varchar(45)", nullable: true),
                                             GPRs             = table.Column<int>("int(11)", nullable: true),
                                             GPR_size         = table.Column<int>("int(11)", nullable: true),
                                             FPRs             = table.Column<int>("int(11)", nullable: true),
                                             FPR_size         = table.Column<int>("int(11)", nullable: true),
                                             cores            = table.Column<int>("int(11)", nullable: true),
                                             threads_per_core = table.Column<int>("int(11)", nullable: true),
                                             process          = table.Column<string>("varchar(45)", nullable: true),
                                             process_nm       = table.Column<float>(nullable: true),
                                             die_size         = table.Column<float>(nullable: true),
                                             transistors      = table.Column<long>("bigint(20)", nullable: true),
                                             data_bus         = table.Column<int>("int(11)", nullable: true),
                                             addr_bus         = table.Column<int>("int(11)", nullable: true),
                                             SIMD_registers   = table.Column<int>("int(11)", nullable: true),
                                             SIMD_size        = table.Column<int>("int(11)", nullable: true),
                                             L1_instruction   = table.Column<float>(nullable: true),
                                             L1_data          = table.Column<float>(nullable: true),
                                             L2               = table.Column<float>(nullable: true),
                                             L3               = table.Column<float>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_processors", x => x.id);
                                             table.ForeignKey("fk_processors_company", x => x.company, "companies",
                                                              "id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("fk_processors_instruction_set", x => x.instruction_set,
                                                              "instruction_sets", "id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("sound_synths",
                                         table => new
                                         {
                                             id =
                                                 table.Column<int>("int(11)", nullable: false)
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.IdentityColumn),
                                             name =
                                                 table.Column<string>("char(50)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             company     = table.Column<int>("int(11)", nullable: true),
                                             model_code  = table.Column<string>("varchar(45)", nullable: true),
                                             introduced  = table.Column<DateTime>("datetime", nullable: true),
                                             voices      = table.Column<int>("int(11)", nullable: true),
                                             frequency   = table.Column<double>(nullable: true),
                                             depth       = table.Column<int>("int(11)", nullable: true),
                                             square_wave = table.Column<int>("int(11)", nullable: true),
                                             white_noise = table.Column<int>("int(11)", nullable: true),
                                             type        = table.Column<int>("int(11)", nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_sound_synths", x => x.id);
                                             table.ForeignKey("fk_sound_synths_company", x => x.company, "companies",
                                                              "id", onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("resolutions_by_gpu",
                                         table => new
                                         {
                                             gpu        = table.Column<int>("int(11)", nullable: false),
                                             resolution = table.Column<int>("int(11)", nullable: false),
                                             id = table.Column<long>("bigint(20)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_resolutions_by_gpu", x => x.id);
                                             table.ForeignKey("fk_resolutions_by_gpu_gpu", x => x.gpu, "gpus", "id",
                                                              onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("fk_resolutions_by_gpu_resolution", x => x.resolution,
                                                              "resolutions", "id", onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("machines",
                                         table => new
                                         {
                                             id =
                                                 table.Column<int>("int(11)", nullable: false)
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.IdentityColumn),
                                             company =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             name = table.Column<string>("varchar(255)", nullable: false),
                                             type =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             introduced = table.Column<DateTime>("datetime", nullable: true),
                                             family     = table.Column<int>("int(11)", nullable: true),
                                             model      = table.Column<string>("varchar(50)", nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_machines", x => x.id);
                                             table.ForeignKey("fk_machines_company", x => x.company, "companies", "id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("fk_machines_family", x => x.family, "machine_families",
                                                              "id", onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("instruction_set_extensions_by_processor",
                                         table => new
                                         {
                                             id = table.Column<int>("int(11)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             processor_id = table.Column<int>("int(11)", nullable: false),
                                             extension_id = table.Column<int>("int(11)", nullable: false)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_instruction_set_extensions_by_processor",
                                                              x => new {x.id, x.processor_id, x.extension_id});
                                             table.ForeignKey("fk_extension_extension_id", x => x.extension_id,
                                                              "instruction_set_extensions", "id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("fk_extension_processor_id", x => x.processor_id,
                                                              "processors", "id", onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("gpus_by_machine",
                                         table => new
                                         {
                                             gpu     = table.Column<int>("int(11)", nullable: false),
                                             machine = table.Column<int>("int(11)", nullable: false),
                                             id = table.Column<long>("bigint(20)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_gpus_by_machine", x => x.id);
                                             table.ForeignKey("fk_gpus_by_machine_gpu", x => x.gpu, "gpus", "id",
                                                              onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("fk_gpus_by_machine_machine", x => x.machine, "machines",
                                                              "id", onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("memory_by_machine",
                                         table => new
                                         {
                                             machine = table.Column<int>("int(11)", nullable: false),
                                             type =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             usage =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             size  = table.Column<long>("bigint(20)", nullable: true),
                                             speed = table.Column<double>(nullable: true),
                                             id = table.Column<long>("bigint(20)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_memory_by_machine", x => x.id);
                                             table.ForeignKey("fk_memory_by_machine_machine", x => x.machine,
                                                              "machines", "id", onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("processors_by_machine",
                                         table => new
                                         {
                                             processor = table.Column<int>("int(11)", nullable: false),
                                             machine   = table.Column<int>("int(11)", nullable: false),
                                             speed     = table.Column<float>(nullable: true),
                                             id = table.Column<long>("bigint(20)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_processors_by_machine", x => x.id);
                                             table.ForeignKey("fk_processors_by_machine_machine", x => x.machine,
                                                              "machines", "id", onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("fk_processors_by_machine_processor", x => x.processor,
                                                              "processors", "id", onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("sound_by_machine",
                                         table => new
                                         {
                                             sound_synth = table.Column<int>("int(11)", nullable: false),
                                             machine     = table.Column<int>("int(11)", nullable: false),
                                             id = table.Column<long>("bigint(20)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_sound_by_machine", x => x.id);
                                             table.ForeignKey("fk_sound_by_machine_machine", x => x.machine, "machines",
                                                              "id", onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("fk_sound_by_machine_sound_synth", x => x.sound_synth,
                                                              "sound_synths", "id",
                                                              onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("storage_by_machine",
                                         table => new
                                         {
                                             machine = table.Column<int>("int(11)", nullable: false),
                                             type =
                                                 table.Column<int>("int(11)", nullable: false, defaultValueSql: "'0'"),
                                             @interface =
                                                 table.Column<int>(name: "interface", type: "int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             capacity = table.Column<long>("bigint(20)", nullable: true),
                                             id = table.Column<long>("bigint(20)", nullable: false)
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_storage_by_machine", x => x.id);
                                             table.ForeignKey("fk_storage_by_machine_machine", x => x.machine,
                                                              "machines", "id", onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateIndex("idx_admins_user", "admins", "user");

            migrationBuilder.CreateIndex("idx_browser_tests_browser", "browser_tests", "browser");

            migrationBuilder.CreateIndex("idx_browser_tests_os", "browser_tests", "os");

            migrationBuilder.CreateIndex("idx_browser_tests_platform", "browser_tests", "platform");

            migrationBuilder.CreateIndex("idx_browser_tests_user_agent", "browser_tests", "user_agent");

            migrationBuilder.CreateIndex("idx_browser_tests_version", "browser_tests", "version");

            migrationBuilder.CreateIndex("idx_companies_address", "companies", "address");

            migrationBuilder.CreateIndex("idx_companies_city", "companies", "city");

            migrationBuilder.CreateIndex("idx_companies_country", "companies", "country");

            migrationBuilder.CreateIndex("idx_companies_facebook", "companies", "facebook");

            migrationBuilder.CreateIndex("idx_companies_founded", "companies", "founded");

            migrationBuilder.CreateIndex("idx_companies_name", "companies", "name");

            migrationBuilder.CreateIndex("idx_companies_postal_code", "companies", "postal_code");

            migrationBuilder.CreateIndex("idx_companies_province", "companies", "province");

            migrationBuilder.CreateIndex("idx_companies_sold", "companies", "sold");

            migrationBuilder.CreateIndex("idx_companies_sold_to", "companies", "sold_to");

            migrationBuilder.CreateIndex("idx_companies_status", "companies", "status");

            migrationBuilder.CreateIndex("idx_companies_twitter", "companies", "twitter");

            migrationBuilder.CreateIndex("idx_companies_website", "companies", "website");

            migrationBuilder.CreateIndex("idx_company_id", "company_descriptions", "company_id");

            migrationBuilder.CreateIndex("idx_text", "company_descriptions", "text");

            migrationBuilder.CreateIndex("idx_company_id", "company_logos", "company_id");

            migrationBuilder.CreateIndex("idx_id", "company_logos", "id", unique: true);

            migrationBuilder.CreateIndex("idx_guid", "company_logos", "logo_guid");

            migrationBuilder.CreateIndex("idx_forbidden_browser", "forbidden", "browser");

            migrationBuilder.CreateIndex("idx_forbidden_date", "forbidden", "date");

            migrationBuilder.CreateIndex("idx_forbidden_ip", "forbidden", "ip");

            migrationBuilder.CreateIndex("idx_forbidden_referer", "forbidden", "referer");

            migrationBuilder.CreateIndex("idx_gpus_company", "gpus", "company");

            migrationBuilder.CreateIndex("idx_gpus_die_size", "gpus", "die_size");

            migrationBuilder.CreateIndex("idx_gpus_introduced", "gpus", "introduced");

            migrationBuilder.CreateIndex("idx_gpus_model_code", "gpus", "model_code");

            migrationBuilder.CreateIndex("idx_gpus_name", "gpus", "name");

            migrationBuilder.CreateIndex("idx_gpus_package", "gpus", "package");

            migrationBuilder.CreateIndex("idx_gpus_process", "gpus", "process");

            migrationBuilder.CreateIndex("idx_gpus_process_nm", "gpus", "process_nm");

            migrationBuilder.CreateIndex("idx_gpus_transistors", "gpus", "transistors");

            migrationBuilder.CreateIndex("idx_gpus_by_machine_gpus", "gpus_by_machine", "gpu");

            migrationBuilder.CreateIndex("idx_gpus_by_machine_machine", "gpus_by_machine", "machine");

            migrationBuilder.CreateIndex("idx_setextension_extension", "instruction_set_extensions_by_processor",
                                         "extension_id");

            migrationBuilder.CreateIndex("idx_setextension_processor", "instruction_set_extensions_by_processor",
                                         "processor_id");

            migrationBuilder.CreateIndex("idx_name", "iso3166_1_numeric", "name");

            migrationBuilder.CreateIndex("idx_log_browser", "log", "browser");

            migrationBuilder.CreateIndex("idx_log_date", "log", "date");

            migrationBuilder.CreateIndex("idx_log_ip", "log", "ip");

            migrationBuilder.CreateIndex("idx_log_referer", "log", "referer");

            migrationBuilder.CreateIndex("idx_machine_families_company", "machine_families", "company");

            migrationBuilder.CreateIndex("idx_machine_families_name", "machine_families", "name");

            migrationBuilder.CreateIndex("idx_machines_company", "machines", "company");

            migrationBuilder.CreateIndex("idx_machines_family", "machines", "family");

            migrationBuilder.CreateIndex("idx_machines_introduced", "machines", "introduced");

            migrationBuilder.CreateIndex("idx_machines_model", "machines", "model");

            migrationBuilder.CreateIndex("idx_machines_name", "machines", "name");

            migrationBuilder.CreateIndex("idx_machines_type", "machines", "type");

            migrationBuilder.CreateIndex("idx_memory_by_machine_machine", "memory_by_machine", "machine");

            migrationBuilder.CreateIndex("idx_memory_by_machine_size", "memory_by_machine", "size");

            migrationBuilder.CreateIndex("idx_memory_by_machine_speed", "memory_by_machine", "speed");

            migrationBuilder.CreateIndex("idx_memory_by_machine_type", "memory_by_machine", "type");

            migrationBuilder.CreateIndex("idx_memory_by_machine_usage", "memory_by_machine", "usage");

            migrationBuilder.CreateIndex("idx_money_donations_donator", "money_donations", "donator");

            migrationBuilder.CreateIndex("idx_money_donations_quantity", "money_donations", "quantity");

            migrationBuilder.CreateIndex("idx_news_ip", "news", "added_id");

            migrationBuilder.CreateIndex("idx_news_date", "news", "date");

            migrationBuilder.CreateIndex("idx_news_type", "news", "type");

            migrationBuilder.CreateIndex("idx_owned_computers_boxed", "owned_computers", "boxed");

            migrationBuilder.CreateIndex("idx_owned_computers_cap1", "owned_computers", "cap1");

            migrationBuilder.CreateIndex("idx_owned_computers_cap2", "owned_computers", "cap2");

            migrationBuilder.CreateIndex("idx_owned_computers_cpu1", "owned_computers", "cpu1");

            migrationBuilder.CreateIndex("idx_owned_computers_cpu2", "owned_computers", "cpu2");

            migrationBuilder.CreateIndex("idx_owned_computers_date", "owned_computers", "date");

            migrationBuilder.CreateIndex("idx_owned_computers_db_id", "owned_computers", "db_id");

            migrationBuilder.CreateIndex("idx_owned_computers_disk1", "owned_computers", "disk1");

            migrationBuilder.CreateIndex("idx_owned_computers_disk2", "owned_computers", "disk2");

            migrationBuilder.CreateIndex("idx_owned_computers_manuals", "owned_computers", "manuals");

            migrationBuilder.CreateIndex("idx_owned_computers_mhz1", "owned_computers", "mhz1");

            migrationBuilder.CreateIndex("idx_owned_computers_mhz2", "owned_computers", "mhz2");

            migrationBuilder.CreateIndex("idx_owned_computers_ram", "owned_computers", "ram");

            migrationBuilder.CreateIndex("idx_owned_computers_rigid", "owned_computers", "rigid");

            migrationBuilder.CreateIndex("idx_owned_computers_status", "owned_computers", "status");

            migrationBuilder.CreateIndex("idx_owned_computers_trade", "owned_computers", "trade");

            migrationBuilder.CreateIndex("idx_owned_computers_vram", "owned_computers", "vram");

            migrationBuilder.CreateIndex("idx_owned_consoles_boxed", "owned_consoles", "boxed");

            migrationBuilder.CreateIndex("idx_owned_consoles_date", "owned_consoles", "date");

            migrationBuilder.CreateIndex("idx_owned_consoles_db_id", "owned_consoles", "db_id");

            migrationBuilder.CreateIndex("idx_owned_consoles_manuals", "owned_consoles", "manuals");

            migrationBuilder.CreateIndex("idx_owned_consoles_status", "owned_consoles", "status");

            migrationBuilder.CreateIndex("idx_owned_consoles_trade", "owned_consoles", "trade");

            migrationBuilder.CreateIndex("idx_processors_addr_bus", "processors", "addr_bus");

            migrationBuilder.CreateIndex("idx_processors_company", "processors", "company");

            migrationBuilder.CreateIndex("idx_processors_cores", "processors", "cores");

            migrationBuilder.CreateIndex("idx_processors_data_bus", "processors", "data_bus");

            migrationBuilder.CreateIndex("idx_processors_die_size", "processors", "die_size");

            migrationBuilder.CreateIndex("idx_processors_FPR_size", "processors", "FPR_size");

            migrationBuilder.CreateIndex("idx_processors_FPRs", "processors", "FPRs");

            migrationBuilder.CreateIndex("idx_processors_GPR_size", "processors", "GPR_size");

            migrationBuilder.CreateIndex("idx_processors_GPRs", "processors", "GPRs");

            migrationBuilder.CreateIndex("idx_processors_instruction_set", "processors", "instruction_set");

            migrationBuilder.CreateIndex("idx_processors_introduced", "processors", "introduced");

            migrationBuilder.CreateIndex("idx_processors_L1_data", "processors", "L1_data");

            migrationBuilder.CreateIndex("idx_processors_L1_instruction", "processors", "L1_instruction");

            migrationBuilder.CreateIndex("idx_processors_L2", "processors", "L2");

            migrationBuilder.CreateIndex("idx_processors_L3", "processors", "L3");

            migrationBuilder.CreateIndex("idx_processors_model_code", "processors", "model_code");

            migrationBuilder.CreateIndex("idx_processors_name", "processors", "name");

            migrationBuilder.CreateIndex("idx_processors_package", "processors", "package");

            migrationBuilder.CreateIndex("idx_processors_process", "processors", "process");

            migrationBuilder.CreateIndex("idx_processors_process_nm", "processors", "process_nm");

            migrationBuilder.CreateIndex("idx_processors_SIMD_registers", "processors", "SIMD_registers");

            migrationBuilder.CreateIndex("idx_processors_SIMD_size", "processors", "SIMD_size");

            migrationBuilder.CreateIndex("idx_processors_speed", "processors", "speed");

            migrationBuilder.CreateIndex("idx_processors_threads_per_core", "processors", "threads_per_core");

            migrationBuilder.CreateIndex("idx_processors_transistors", "processors", "transistors");

            migrationBuilder.CreateIndex("idx_processors_by_machine_machine", "processors_by_machine", "machine");

            migrationBuilder.CreateIndex("idx_processors_by_machine_processor", "processors_by_machine", "processor");

            migrationBuilder.CreateIndex("idx_processors_by_machine_speed", "processors_by_machine", "speed");

            migrationBuilder.CreateIndex("idx_resolutions_colors", "resolutions", "colors");

            migrationBuilder.CreateIndex("idx_resolutions_height", "resolutions", "height");

            migrationBuilder.CreateIndex("idx_resolutions_palette", "resolutions", "palette");

            migrationBuilder.CreateIndex("idx_resolutions_width", "resolutions", "width");

            migrationBuilder.CreateIndex("idx_resolutions_resolution", "resolutions", new[] {"width", "height"});

            migrationBuilder.CreateIndex("idx_resolutions_resolution_with_color", "resolutions",
                                         new[] {"width", "height", "colors"});

            migrationBuilder.CreateIndex("idx_resolutions_resolution_with_color_and_palette", "resolutions",
                                         new[] {"width", "height", "colors", "palette"});

            migrationBuilder.CreateIndex("idx_resolutions_by_gpu_gpu", "resolutions_by_gpu", "gpu");

            migrationBuilder.CreateIndex("idx_resolutions_by_gpu_resolution", "resolutions_by_gpu", "resolution");

            migrationBuilder.CreateIndex("idx_sound_by_machine_machine", "sound_by_machine", "machine");

            migrationBuilder.CreateIndex("idx_sound_by_machine_sound_synth", "sound_by_machine", "sound_synth");

            migrationBuilder.CreateIndex("idx_sound_synths_company", "sound_synths", "company");

            migrationBuilder.CreateIndex("idx_sound_synths_depth", "sound_synths", "depth");

            migrationBuilder.CreateIndex("idx_sound_synths_frequency", "sound_synths", "frequency");

            migrationBuilder.CreateIndex("idx_sound_synths_introduced", "sound_synths", "introduced");

            migrationBuilder.CreateIndex("idx_sound_synths_model_code", "sound_synths", "model_code");

            migrationBuilder.CreateIndex("idx_sound_synths_name", "sound_synths", "name");

            migrationBuilder.CreateIndex("idx_sound_synths_square_wave", "sound_synths", "square_wave");

            migrationBuilder.CreateIndex("idx_sound_synths_type", "sound_synths", "type");

            migrationBuilder.CreateIndex("idx_sound_synths_voices", "sound_synths", "voices");

            migrationBuilder.CreateIndex("idx_sound_synths_white_noise", "sound_synths", "white_noise");

            migrationBuilder.CreateIndex("idx_storage_capacity", "storage_by_machine", "capacity");

            migrationBuilder.CreateIndex("idx_storage_interface", "storage_by_machine", "interface");

            migrationBuilder.CreateIndex("idx_storage_machine", "storage_by_machine", "machine");

            migrationBuilder.CreateIndex("idx_storage_type", "storage_by_machine", "type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("admins");

            migrationBuilder.DropTable("browser_tests");

            migrationBuilder.DropTable("cicm_db");

            migrationBuilder.DropTable("company_descriptions");

            migrationBuilder.DropTable("company_logos");

            migrationBuilder.DropTable("forbidden");

            migrationBuilder.DropTable("gpus_by_machine");

            migrationBuilder.DropTable("instruction_set_extensions_by_processor");

            migrationBuilder.DropTable("log");

            migrationBuilder.DropTable("memory_by_machine");

            migrationBuilder.DropTable("money_donations");

            migrationBuilder.DropTable("news");

            migrationBuilder.DropTable("owned_computers");

            migrationBuilder.DropTable("owned_consoles");

            migrationBuilder.DropTable("processors_by_machine");

            migrationBuilder.DropTable("resolutions_by_gpu");

            migrationBuilder.DropTable("sound_by_machine");

            migrationBuilder.DropTable("storage_by_machine");

            migrationBuilder.DropTable("instruction_set_extensions");

            migrationBuilder.DropTable("processors");

            migrationBuilder.DropTable("gpus");

            migrationBuilder.DropTable("resolutions");

            migrationBuilder.DropTable("sound_synths");

            migrationBuilder.DropTable("machines");

            migrationBuilder.DropTable("instruction_sets");

            migrationBuilder.DropTable("machine_families");

            migrationBuilder.DropTable("companies");

            migrationBuilder.DropTable("iso3166_1_numeric");
        }
    }
}
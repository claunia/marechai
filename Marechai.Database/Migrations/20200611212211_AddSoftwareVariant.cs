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
    public partial class AddSoftwareVariant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("SoftwareVariants", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Name              = table.Column<string>(nullable: true),
                Version           = table.Column<string>(nullable: true),
                Introduced        = table.Column<DateTime>(nullable: true),
                ParentId          = table.Column<ulong>(nullable: true),
                SoftwareVersionId = table.Column<ulong>(nullable: false),
                MinimumMemory     = table.Column<ulong>(nullable: true),
                RecommendedMemory = table.Column<ulong>(nullable: true),
                RequiredStorage   = table.Column<ulong>(nullable: true),
                PartNumber        = table.Column<string>(nullable: true),
                SerialNumber      = table.Column<string>(nullable: true),
                ProductCode       = table.Column<string>(nullable: true),
                CatalogueNumber   = table.Column<string>(nullable: true),
                DistributionMode  = table.Column<uint>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_SoftwareVariants", x => x.Id);

                table.ForeignKey("FK_SoftwareVariants_SoftwareVariants_ParentId", x => x.ParentId, "SoftwareVariants",
                                 "Id", onDelete: ReferentialAction.SetNull);

                table.ForeignKey("FK_SoftwareVariants_SoftwareVersion_SoftwareVersionId", x => x.SoftwareVersionId,
                                 "SoftwareVersion", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("CompaniesBySoftwareVariant", table => new
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
                SoftwareVariantId = table.Column<ulong>(nullable: false),
                RoleId            = table.Column<string>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_CompaniesBySoftwareVariant", x => x.Id);

                table.ForeignKey("FK_CompaniesBySoftwareVariant_companies_CompanyId", x => x.CompanyId, "companies",
                                 "id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_CompaniesBySoftwareVariant_DocumentRoles_RoleId", x => x.RoleId, "DocumentRoles",
                                 "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_CompaniesBySoftwareVariant_SoftwareVariants_SoftwareVariantId",
                                 x => x.SoftwareVariantId, "SoftwareVariants", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("GpusBySoftwareVariant", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                GpuId             = table.Column<int>(nullable: false),
                SoftwareVariantId = table.Column<ulong>(nullable: false),
                Minimum           = table.Column<bool>(nullable: true),
                Recommended       = table.Column<bool>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_GpusBySoftwareVariant", x => x.Id);

                table.ForeignKey("FK_GpusBySoftwareVariant_gpus_GpuId", x => x.GpuId, "gpus", "id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_GpusBySoftwareVariant_SoftwareVariants_SoftwareVariantId",
                                 x => x.SoftwareVariantId, "SoftwareVariants", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("InstructionSetsBySoftwareVariant", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                InstructionSetId  = table.Column<int>(nullable: false),
                SoftwareVariantId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_InstructionSetsBySoftwareVariant", x => x.Id);

                table.ForeignKey("FK_InstructionSetsBySoftwareVariant_instruction_sets_Instructio~",
                                 x => x.InstructionSetId, "instruction_sets", "id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_InstructionSetsBySoftwareVariant_SoftwareVariants_SoftwareVa~",
                                 x => x.SoftwareVariantId, "SoftwareVariants", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("LanguagesBySoftwareVariant", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                LanguageId        = table.Column<string>(nullable: false),
                SoftwareVariantId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_LanguagesBySoftwareVariant", x => x.Id);

                table.ForeignKey("FK_LanguagesBySoftwareVariant_ISO_639-3_LanguageId", x => x.LanguageId, "ISO_639-3",
                                 "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_LanguagesBySoftwareVariant_SoftwareVariants_SoftwareVariantId",
                                 x => x.SoftwareVariantId, "SoftwareVariants", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("MachineFamiliesBySoftwareVariant", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                MachineFamilyId   = table.Column<int>(nullable: false),
                SoftwareVariantId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_MachineFamiliesBySoftwareVariant", x => x.Id);

                table.ForeignKey("FK_MachineFamiliesBySoftwareVariant_machine_families_MachineFam~",
                                 x => x.MachineFamilyId, "machine_families", "id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_MachineFamiliesBySoftwareVariant_SoftwareVariants_SoftwareVa~",
                                 x => x.SoftwareVariantId, "SoftwareVariants", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("MachinesBySoftwareVariant", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                MachineId         = table.Column<int>(nullable: false),
                SoftwareVariantId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_MachinesBySoftwareVariant", x => x.Id);

                table.ForeignKey("FK_MachinesBySoftwareVariant_machines_MachineId", x => x.MachineId, "machines", "id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_MachinesBySoftwareVariant_SoftwareVariants_SoftwareVariantId",
                                 x => x.SoftwareVariantId, "SoftwareVariants", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("MediaBySoftwareVariant", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                MediaId           = table.Column<ulong>(nullable: false),
                SoftwareVariantId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_MediaBySoftwareVariant", x => x.Id);

                table.ForeignKey("FK_MediaBySoftwareVariant_Media_MediaId", x => x.MediaId, "Media", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_MediaBySoftwareVariant_SoftwareVariants_SoftwareVariantId",
                                 x => x.SoftwareVariantId, "SoftwareVariants", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("PeopleBySoftwareVariant", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                SoftwareVariantId = table.Column<ulong>(nullable: false),
                PersonId          = table.Column<int>(nullable: false),
                RoleId            = table.Column<string>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_PeopleBySoftwareVariant", x => x.Id);

                table.ForeignKey("FK_PeopleBySoftwareVariant_People_PersonId", x => x.PersonId, "People", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_PeopleBySoftwareVariant_DocumentRoles_RoleId", x => x.RoleId, "DocumentRoles",
                                 "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_PeopleBySoftwareVariant_SoftwareVariants_SoftwareVariantId",
                                 x => x.SoftwareVariantId, "SoftwareVariants", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("ProcessorsBySoftwareVariant", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                ProcessorId       = table.Column<int>(nullable: false),
                SoftwareVariantId = table.Column<ulong>(nullable: false),
                Speed             = table.Column<float>(nullable: true),
                Minimum           = table.Column<bool>(nullable: true),
                Recommended       = table.Column<bool>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_ProcessorsBySoftwareVariant", x => x.Id);

                table.ForeignKey("FK_ProcessorsBySoftwareVariant_processors_ProcessorId", x => x.ProcessorId,
                                 "processors", "id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_ProcessorsBySoftwareVariant_SoftwareVariants_SoftwareVariant~",
                                 x => x.SoftwareVariantId, "SoftwareVariants", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("RequiredOperatingSystemsBySofwareVariant", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                OperatingSystemId = table.Column<ulong>(nullable: false),
                SoftwareVariantId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_RequiredOperatingSystemsBySofwareVariant", x => x.Id);

                table.ForeignKey("FK_RequiredOperatingSystemsBySofwareVariant_SoftwareVersion_Ope~",
                                 x => x.OperatingSystemId, "SoftwareVersion", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_RequiredOperatingSystemsBySofwareVariant_SoftwareVariants_So~",
                                 x => x.SoftwareVariantId, "SoftwareVariants", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("RequiredSoftwareBySoftwareVariant", table => new
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
                SoftwareVariantId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_RequiredSoftwareBySoftwareVariant", x => x.Id);

                table.ForeignKey("FK_RequiredSoftwareBySoftwareVariant_SoftwareVariants_SoftwareV~",
                                 x => x.SoftwareVariantId, "SoftwareVariants", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_RequiredSoftwareBySoftwareVariant_SoftwareVersion_SoftwareVe~",
                                 x => x.SoftwareVersionId, "SoftwareVersion", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("SoundBySoftwareVariant", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                SoundSynthId      = table.Column<int>(nullable: false),
                SoftwareVariantId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_SoundBySoftwareVariant", x => x.Id);

                table.ForeignKey("FK_SoundBySoftwareVariant_SoftwareVariants_SoftwareVariantId",
                                 x => x.SoftwareVariantId, "SoftwareVariants", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_SoundBySoftwareVariant_sound_synths_SoundSynthId", x => x.SoundSynthId,
                                 "sound_synths", "id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("StandaloneFiles", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Path              = table.Column<string>(maxLength: 8192, nullable: false),
                Name              = table.Column<string>(maxLength: 255, nullable: false),
                PathSeparator     = table.Column<string>(nullable: false),
                IsDirectory       = table.Column<bool>(nullable: false),
                CreationDate      = table.Column<DateTime>(nullable: true),
                AccessDate        = table.Column<DateTime>(nullable: true),
                StatusChangeDate  = table.Column<DateTime>(nullable: true),
                BackupDate        = table.Column<DateTime>(nullable: true),
                LastWriteDate     = table.Column<DateTime>(nullable: true),
                Attributes        = table.Column<ulong>(nullable: false),
                PosixMode         = table.Column<ushort>(nullable: true),
                DeviceNumber      = table.Column<uint>(nullable: true),
                GroupId           = table.Column<ulong>(nullable: true),
                UserId            = table.Column<ulong>(nullable: true),
                Inode             = table.Column<ulong>(nullable: true),
                Links             = table.Column<ulong>(nullable: true),
                SoftwareVariantId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_StandaloneFiles", x => x.Id);

                table.ForeignKey("FK_StandaloneFiles_SoftwareVariants_SoftwareVariantId", x => x.SoftwareVariantId,
                                 "SoftwareVariants", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("FileDataStreamsByStandaloneFile", table => new
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
                StandaloneFileId = table.Column<ulong>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_FileDataStreamsByStandaloneFile", x => x.Id);

                table.ForeignKey("FK_FileDataStreamsByStandaloneFile_FileDataStreams_FileDataStre~",
                                 x => x.FileDataStreamId, "FileDataStreams", "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_FileDataStreamsByStandaloneFile_StandaloneFiles_StandaloneFi~",
                                 x => x.StandaloneFileId, "StandaloneFiles", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_CompaniesBySoftwareVariant_CompanyId", "CompaniesBySoftwareVariant",
                                         "CompanyId");

            migrationBuilder.CreateIndex("IX_CompaniesBySoftwareVariant_RoleId", "CompaniesBySoftwareVariant",
                                         "RoleId");

            migrationBuilder.CreateIndex("IX_CompaniesBySoftwareVariant_SoftwareVariantId",
                                         "CompaniesBySoftwareVariant", "SoftwareVariantId");

            migrationBuilder.CreateIndex("IX_FileDataStreamsByStandaloneFile_FileDataStreamId",
                                         "FileDataStreamsByStandaloneFile", "FileDataStreamId");

            migrationBuilder.CreateIndex("IX_FileDataStreamsByStandaloneFile_StandaloneFileId",
                                         "FileDataStreamsByStandaloneFile", "StandaloneFileId");

            migrationBuilder.CreateIndex("IX_GpusBySoftwareVariant_GpuId", "GpusBySoftwareVariant", "GpuId");

            migrationBuilder.CreateIndex("IX_GpusBySoftwareVariant_Minimum", "GpusBySoftwareVariant", "Minimum");

            migrationBuilder.CreateIndex("IX_GpusBySoftwareVariant_Recommended", "GpusBySoftwareVariant",
                                         "Recommended");

            migrationBuilder.CreateIndex("IX_GpusBySoftwareVariant_SoftwareVariantId", "GpusBySoftwareVariant",
                                         "SoftwareVariantId");

            migrationBuilder.CreateIndex("IX_InstructionSetsBySoftwareVariant_InstructionSetId",
                                         "InstructionSetsBySoftwareVariant", "InstructionSetId");

            migrationBuilder.CreateIndex("IX_InstructionSetsBySoftwareVariant_SoftwareVariantId",
                                         "InstructionSetsBySoftwareVariant", "SoftwareVariantId");

            migrationBuilder.CreateIndex("IX_LanguagesBySoftwareVariant_LanguageId", "LanguagesBySoftwareVariant",
                                         "LanguageId");

            migrationBuilder.CreateIndex("IX_LanguagesBySoftwareVariant_SoftwareVariantId",
                                         "LanguagesBySoftwareVariant", "SoftwareVariantId");

            migrationBuilder.CreateIndex("IX_MachineFamiliesBySoftwareVariant_MachineFamilyId",
                                         "MachineFamiliesBySoftwareVariant", "MachineFamilyId");

            migrationBuilder.CreateIndex("IX_MachineFamiliesBySoftwareVariant_SoftwareVariantId",
                                         "MachineFamiliesBySoftwareVariant", "SoftwareVariantId");

            migrationBuilder.CreateIndex("IX_MachinesBySoftwareVariant_MachineId", "MachinesBySoftwareVariant",
                                         "MachineId");

            migrationBuilder.CreateIndex("IX_MachinesBySoftwareVariant_SoftwareVariantId", "MachinesBySoftwareVariant",
                                         "SoftwareVariantId");

            migrationBuilder.CreateIndex("IX_MediaBySoftwareVariant_MediaId", "MediaBySoftwareVariant", "MediaId");

            migrationBuilder.CreateIndex("IX_MediaBySoftwareVariant_SoftwareVariantId", "MediaBySoftwareVariant",
                                         "SoftwareVariantId");

            migrationBuilder.CreateIndex("IX_PeopleBySoftwareVariant_PersonId", "PeopleBySoftwareVariant", "PersonId");

            migrationBuilder.CreateIndex("IX_PeopleBySoftwareVariant_RoleId", "PeopleBySoftwareVariant", "RoleId");

            migrationBuilder.CreateIndex("IX_PeopleBySoftwareVariant_SoftwareVariantId", "PeopleBySoftwareVariant",
                                         "SoftwareVariantId");

            migrationBuilder.CreateIndex("IX_ProcessorsBySoftwareVariant_Minimum", "ProcessorsBySoftwareVariant",
                                         "Minimum");

            migrationBuilder.CreateIndex("IX_ProcessorsBySoftwareVariant_ProcessorId", "ProcessorsBySoftwareVariant",
                                         "ProcessorId");

            migrationBuilder.CreateIndex("IX_ProcessorsBySoftwareVariant_Recommended", "ProcessorsBySoftwareVariant",
                                         "Recommended");

            migrationBuilder.CreateIndex("IX_ProcessorsBySoftwareVariant_SoftwareVariantId",
                                         "ProcessorsBySoftwareVariant", "SoftwareVariantId");

            migrationBuilder.CreateIndex("IX_ProcessorsBySoftwareVariant_Speed", "ProcessorsBySoftwareVariant",
                                         "Speed");

            migrationBuilder.CreateIndex("IX_RequiredOperatingSystemsBySofwareVariant_OperatingSystemId",
                                         "RequiredOperatingSystemsBySofwareVariant", "OperatingSystemId");

            migrationBuilder.CreateIndex("IX_RequiredOperatingSystemsBySofwareVariant_SoftwareVariantId",
                                         "RequiredOperatingSystemsBySofwareVariant", "SoftwareVariantId");

            migrationBuilder.CreateIndex("IX_RequiredSoftwareBySoftwareVariant_SoftwareVariantId",
                                         "RequiredSoftwareBySoftwareVariant", "SoftwareVariantId");

            migrationBuilder.CreateIndex("IX_RequiredSoftwareBySoftwareVariant_SoftwareVersionId",
                                         "RequiredSoftwareBySoftwareVariant", "SoftwareVersionId");

            migrationBuilder.CreateIndex("IX_SoftwareVariants_CatalogueNumber", "SoftwareVariants", "CatalogueNumber");

            migrationBuilder.CreateIndex("IX_SoftwareVariants_DistributionMode", "SoftwareVariants",
                                         "DistributionMode");

            migrationBuilder.CreateIndex("IX_SoftwareVariants_Introduced", "SoftwareVariants", "Introduced");

            migrationBuilder.CreateIndex("IX_SoftwareVariants_MinimumMemory", "SoftwareVariants", "MinimumMemory");

            migrationBuilder.CreateIndex("IX_SoftwareVariants_Name", "SoftwareVariants", "Name");

            migrationBuilder.CreateIndex("IX_SoftwareVariants_ParentId", "SoftwareVariants", "ParentId");

            migrationBuilder.CreateIndex("IX_SoftwareVariants_PartNumber", "SoftwareVariants", "PartNumber");

            migrationBuilder.CreateIndex("IX_SoftwareVariants_ProductCode", "SoftwareVariants", "ProductCode");

            migrationBuilder.CreateIndex("IX_SoftwareVariants_RecommendedMemory", "SoftwareVariants",
                                         "RecommendedMemory");

            migrationBuilder.CreateIndex("IX_SoftwareVariants_RequiredStorage", "SoftwareVariants", "RequiredStorage");

            migrationBuilder.CreateIndex("IX_SoftwareVariants_SerialNumber", "SoftwareVariants", "SerialNumber");

            migrationBuilder.CreateIndex("IX_SoftwareVariants_SoftwareVersionId", "SoftwareVariants",
                                         "SoftwareVersionId");

            migrationBuilder.CreateIndex("IX_SoftwareVariants_Version", "SoftwareVariants", "Version");

            migrationBuilder.CreateIndex("IX_SoundBySoftwareVariant_SoftwareVariantId", "SoundBySoftwareVariant",
                                         "SoftwareVariantId");

            migrationBuilder.CreateIndex("IX_SoundBySoftwareVariant_SoundSynthId", "SoundBySoftwareVariant",
                                         "SoundSynthId");

            migrationBuilder.CreateIndex("IX_StandaloneFiles_AccessDate", "StandaloneFiles", "AccessDate");

            migrationBuilder.CreateIndex("IX_StandaloneFiles_BackupDate", "StandaloneFiles", "BackupDate");

            migrationBuilder.CreateIndex("IX_StandaloneFiles_CreationDate", "StandaloneFiles", "CreationDate");

            migrationBuilder.CreateIndex("IX_StandaloneFiles_GroupId", "StandaloneFiles", "GroupId");

            migrationBuilder.CreateIndex("IX_StandaloneFiles_IsDirectory", "StandaloneFiles", "IsDirectory");

            migrationBuilder.CreateIndex("IX_StandaloneFiles_LastWriteDate", "StandaloneFiles", "LastWriteDate");

            migrationBuilder.CreateIndex("IX_StandaloneFiles_Name", "StandaloneFiles", "Name");

            migrationBuilder.CreateIndex("IX_StandaloneFiles_Path", "StandaloneFiles", "Path");

            migrationBuilder.CreateIndex("IX_StandaloneFiles_SoftwareVariantId", "StandaloneFiles",
                                         "SoftwareVariantId");

            migrationBuilder.CreateIndex("IX_StandaloneFiles_StatusChangeDate", "StandaloneFiles", "StatusChangeDate");

            migrationBuilder.CreateIndex("IX_StandaloneFiles_UserId", "StandaloneFiles", "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("CompaniesBySoftwareVariant");

            migrationBuilder.DropTable("FileDataStreamsByStandaloneFile");

            migrationBuilder.DropTable("GpusBySoftwareVariant");

            migrationBuilder.DropTable("InstructionSetsBySoftwareVariant");

            migrationBuilder.DropTable("LanguagesBySoftwareVariant");

            migrationBuilder.DropTable("MachineFamiliesBySoftwareVariant");

            migrationBuilder.DropTable("MachinesBySoftwareVariant");

            migrationBuilder.DropTable("MediaBySoftwareVariant");

            migrationBuilder.DropTable("PeopleBySoftwareVariant");

            migrationBuilder.DropTable("ProcessorsBySoftwareVariant");

            migrationBuilder.DropTable("RequiredOperatingSystemsBySofwareVariant");

            migrationBuilder.DropTable("RequiredSoftwareBySoftwareVariant");

            migrationBuilder.DropTable("SoundBySoftwareVariant");

            migrationBuilder.DropTable("StandaloneFiles");

            migrationBuilder.DropTable("SoftwareVariants");
        }
    }
}
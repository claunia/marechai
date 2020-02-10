using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddOwnedMachines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("OwnedMachines",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<long>()
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.IdentityColumn),
                                             AcquisitionDate = table.Column<DateTime>(),
                                             LostDate        = table.Column<DateTime>(nullable: true),
                                             Status          = table.Column<int>(),
                                             LastStatusDate  = table.Column<DateTime>(nullable: true),
                                             Trade           = table.Column<bool>(),
                                             Boxed           = table.Column<bool>(),
                                             Manuals         = table.Column<bool>(),
                                             SerialNumber    = table.Column<string>(nullable: true),
                                             SerialNumberVisible =
                                                 table.Column<bool>(nullable: false, defaultValue: true),
                                             MachineId = table.Column<int>(),
                                             UserId    = table.Column<string>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_OwnedMachines", x => x.Id);
                                             table.ForeignKey("FK_OwnedMachines_machines_MachineId", x => x.MachineId,
                                                              "machines", "id", onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_OwnedMachines_AspNetUsers_UserId", x => x.UserId,
                                                              "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("GpusByOwnedMachine",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             GpuId          = table.Column<int>(),
                                             OwnedMachineId = table.Column<long>()
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_GpusByOwnedMachine", x => x.Id);
                                             table.ForeignKey("FK_GpusByOwnedMachine_gpus_GpuId", x => x.GpuId, "gpus",
                                                              "id", onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_GpusByOwnedMachine_OwnedMachines_OwnedMachineId",
                                                              x => x.OwnedMachineId, "OwnedMachines", "Id",
                                                              onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("MemoryByOwnedMachine",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             OwnedMachineId = table.Column<long>(),
                                             Type           = table.Column<int>(),
                                             Usage          = table.Column<int>(),
                                             Size           = table.Column<long>(),
                                             Speed          = table.Column<double>()
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_MemoryByOwnedMachine", x => x.Id);
                                             table.ForeignKey("FK_MemoryByOwnedMachine_OwnedMachines_OwnedMachineId",
                                                              x => x.OwnedMachineId, "OwnedMachines", "Id",
                                                              onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("OwnedMachinePhotos",
                                         table => new
                                         {
                                             Id                    = table.Column<Guid>(),
                                             Author                = table.Column<string>(nullable: true),
                                             CameraManufacturer    = table.Column<string>(nullable: true),
                                             CameraModel           = table.Column<string>(nullable: true),
                                             ColorSpace            = table.Column<ushort>(nullable: true),
                                             Comments              = table.Column<string>(nullable: true),
                                             Contrast              = table.Column<ushort>(nullable: true),
                                             CreationDate          = table.Column<DateTime>(nullable: true),
                                             DigitalZoomRatio      = table.Column<double>(nullable: true),
                                             ExifVersion           = table.Column<string>(nullable: true),
                                             Exposure              = table.Column<string>(nullable: true),
                                             ExposureMethod        = table.Column<ushort>(nullable: true),
                                             ExposureProgram       = table.Column<ushort>(nullable: true),
                                             Flash                 = table.Column<ushort>(nullable: true),
                                             Focal                 = table.Column<double>(nullable: true),
                                             FocalLength           = table.Column<double>(nullable: true),
                                             FocalLengthEquivalent = table.Column<ushort>(nullable: true),
                                             HorizontalResolution  = table.Column<double>(nullable: true),
                                             IsoRating             = table.Column<ushort>(nullable: true),
                                             Lens                  = table.Column<string>(nullable: true),
                                             LightSource           = table.Column<ushort>(nullable: true),
                                             MeteringMode          = table.Column<ushort>(nullable: true),
                                             ResolutionUnit        = table.Column<ushort>(nullable: true),
                                             Orientation           = table.Column<ushort>(nullable: true),
                                             Saturation            = table.Column<ushort>(nullable: true),
                                             SceneCaptureType      = table.Column<ushort>(nullable: true),
                                             SensingMethod         = table.Column<ushort>(nullable: true),
                                             Sharpness             = table.Column<ushort>(nullable: true),
                                             SoftwareUsed          = table.Column<string>(nullable: true),
                                             SubjectDistanceRange  = table.Column<ushort>(nullable: true),
                                             UploadDate =
                                                 table.Column<DateTime>()
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.ComputedColumn),
                                             VerticalResolution = table.Column<double>(nullable: true),
                                             WhiteBalance       = table.Column<ushort>(nullable: true),
                                             UserId             = table.Column<string>(nullable: true),
                                             LicenseId          = table.Column<int>(),
                                             OwnedMachineId     = table.Column<long>()
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_OwnedMachinePhotos", x => x.Id);
                                             table.ForeignKey("FK_OwnedMachinePhotos_Licenses_LicenseId",
                                                              x => x.LicenseId, "Licenses", "Id",
                                                              onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_OwnedMachinePhotos_OwnedMachines_OwnedMachineId",
                                                              x => x.OwnedMachineId, "OwnedMachines", "Id",
                                                              onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_OwnedMachinePhotos_AspNetUsers_UserId", x => x.UserId,
                                                              "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("ProcessorsByOwnedMachine",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             ProcessorId    = table.Column<int>(),
                                             OwnedMachineId = table.Column<long>(),
                                             Speed          = table.Column<float>()
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_ProcessorsByOwnedMachine", x => x.Id);
                                             table
                                                .ForeignKey("FK_ProcessorsByOwnedMachine_OwnedMachines_OwnedMachineId",
                                                            x => x.OwnedMachineId, "OwnedMachines", "Id",
                                                            onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_ProcessorsByOwnedMachine_processors_ProcessorId",
                                                              x => x.ProcessorId, "processors", "id",
                                                              onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("SoundByOwnedMachine",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             SoundSynthId   = table.Column<int>(),
                                             OwnedMachineId = table.Column<long>()
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_SoundByOwnedMachine", x => x.Id);
                                             table.ForeignKey("FK_SoundByOwnedMachine_OwnedMachines_OwnedMachineId",
                                                              x => x.OwnedMachineId, "OwnedMachines", "Id",
                                                              onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_SoundByOwnedMachine_sound_synths_SoundSynthId",
                                                              x => x.SoundSynthId, "sound_synths", "id",
                                                              onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateTable("StorageByOwnedMachine",
                                         table => new
                                         {
                                             Id = table.Column<long>()
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             OwnedMachineId = table.Column<long>(),
                                             Type           = table.Column<int>(),
                                             Interface      = table.Column<int>(),
                                             Capacity       = table.Column<long>()
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_StorageByOwnedMachine", x => x.Id);
                                             table.ForeignKey("FK_StorageByOwnedMachine_OwnedMachines_OwnedMachineId",
                                                              x => x.OwnedMachineId, "OwnedMachines", "Id",
                                                              onDelete: ReferentialAction.Cascade);
                                         });

            migrationBuilder.CreateIndex("IX_GpusByOwnedMachine_GpuId", "GpusByOwnedMachine", "GpuId");

            migrationBuilder.CreateIndex("IX_GpusByOwnedMachine_OwnedMachineId", "GpusByOwnedMachine",
                                         "OwnedMachineId");

            migrationBuilder.CreateIndex("IX_MemoryByOwnedMachine_OwnedMachineId", "MemoryByOwnedMachine",
                                         "OwnedMachineId");

            migrationBuilder.CreateIndex("IX_MemoryByOwnedMachine_Size", "MemoryByOwnedMachine", "Size");

            migrationBuilder.CreateIndex("IX_MemoryByOwnedMachine_Speed", "MemoryByOwnedMachine", "Speed");

            migrationBuilder.CreateIndex("IX_MemoryByOwnedMachine_Type", "MemoryByOwnedMachine", "Type");

            migrationBuilder.CreateIndex("IX_MemoryByOwnedMachine_Usage", "MemoryByOwnedMachine", "Usage");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_Author", "OwnedMachinePhotos", "Author");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_CameraManufacturer", "OwnedMachinePhotos",
                                         "CameraManufacturer");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_CameraModel", "OwnedMachinePhotos", "CameraModel");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_ColorSpace", "OwnedMachinePhotos", "ColorSpace");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_Comments", "OwnedMachinePhotos", "Comments");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_Contrast", "OwnedMachinePhotos", "Contrast");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_CreationDate", "OwnedMachinePhotos", "CreationDate");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_DigitalZoomRatio", "OwnedMachinePhotos",
                                         "DigitalZoomRatio");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_ExifVersion", "OwnedMachinePhotos", "ExifVersion");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_Exposure", "OwnedMachinePhotos", "Exposure");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_ExposureMethod", "OwnedMachinePhotos",
                                         "ExposureMethod");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_ExposureProgram", "OwnedMachinePhotos",
                                         "ExposureProgram");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_Flash", "OwnedMachinePhotos", "Flash");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_Focal", "OwnedMachinePhotos", "Focal");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_FocalLength", "OwnedMachinePhotos", "FocalLength");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_FocalLengthEquivalent", "OwnedMachinePhotos",
                                         "FocalLengthEquivalent");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_HorizontalResolution", "OwnedMachinePhotos",
                                         "HorizontalResolution");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_IsoRating", "OwnedMachinePhotos", "IsoRating");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_Lens", "OwnedMachinePhotos", "Lens");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_LicenseId", "OwnedMachinePhotos", "LicenseId");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_LightSource", "OwnedMachinePhotos", "LightSource");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_MeteringMode", "OwnedMachinePhotos", "MeteringMode");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_Orientation", "OwnedMachinePhotos", "Orientation");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_OwnedMachineId", "OwnedMachinePhotos",
                                         "OwnedMachineId");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_ResolutionUnit", "OwnedMachinePhotos",
                                         "ResolutionUnit");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_Saturation", "OwnedMachinePhotos", "Saturation");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_SceneCaptureType", "OwnedMachinePhotos",
                                         "SceneCaptureType");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_SensingMethod", "OwnedMachinePhotos", "SensingMethod");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_Sharpness", "OwnedMachinePhotos", "Sharpness");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_SoftwareUsed", "OwnedMachinePhotos", "SoftwareUsed");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_SubjectDistanceRange", "OwnedMachinePhotos",
                                         "SubjectDistanceRange");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_UploadDate", "OwnedMachinePhotos", "UploadDate");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_UserId", "OwnedMachinePhotos", "UserId");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_VerticalResolution", "OwnedMachinePhotos",
                                         "VerticalResolution");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_WhiteBalance", "OwnedMachinePhotos", "WhiteBalance");

            migrationBuilder.CreateIndex("IX_OwnedMachines_AcquisitionDate", "OwnedMachines", "AcquisitionDate");

            migrationBuilder.CreateIndex("IX_OwnedMachines_Boxed", "OwnedMachines", "Boxed");

            migrationBuilder.CreateIndex("IX_OwnedMachines_LastStatusDate", "OwnedMachines", "LastStatusDate");

            migrationBuilder.CreateIndex("IX_OwnedMachines_LostDate", "OwnedMachines", "LostDate");

            migrationBuilder.CreateIndex("IX_OwnedMachines_MachineId", "OwnedMachines", "MachineId");

            migrationBuilder.CreateIndex("IX_OwnedMachines_Manuals", "OwnedMachines", "Manuals");

            migrationBuilder.CreateIndex("IX_OwnedMachines_SerialNumber", "OwnedMachines", "SerialNumber");

            migrationBuilder.CreateIndex("IX_OwnedMachines_SerialNumberVisible", "OwnedMachines",
                                         "SerialNumberVisible");

            migrationBuilder.CreateIndex("IX_OwnedMachines_Status", "OwnedMachines", "Status");

            migrationBuilder.CreateIndex("IX_OwnedMachines_Trade", "OwnedMachines", "Trade");

            migrationBuilder.CreateIndex("IX_OwnedMachines_UserId", "OwnedMachines", "UserId");

            migrationBuilder.CreateIndex("IX_ProcessorsByOwnedMachine_OwnedMachineId", "ProcessorsByOwnedMachine",
                                         "OwnedMachineId");

            migrationBuilder.CreateIndex("IX_ProcessorsByOwnedMachine_ProcessorId", "ProcessorsByOwnedMachine",
                                         "ProcessorId");

            migrationBuilder.CreateIndex("IX_ProcessorsByOwnedMachine_Speed", "ProcessorsByOwnedMachine", "Speed");

            migrationBuilder.CreateIndex("IX_SoundByOwnedMachine_OwnedMachineId", "SoundByOwnedMachine",
                                         "OwnedMachineId");

            migrationBuilder.CreateIndex("IX_SoundByOwnedMachine_SoundSynthId", "SoundByOwnedMachine", "SoundSynthId");

            migrationBuilder.CreateIndex("IX_StorageByOwnedMachine_Capacity", "StorageByOwnedMachine", "Capacity");

            migrationBuilder.CreateIndex("IX_StorageByOwnedMachine_Interface", "StorageByOwnedMachine", "Interface");

            migrationBuilder.CreateIndex("IX_StorageByOwnedMachine_OwnedMachineId", "StorageByOwnedMachine",
                                         "OwnedMachineId");

            migrationBuilder.CreateIndex("IX_StorageByOwnedMachine_Type", "StorageByOwnedMachine", "Type");

            migrationBuilder.Sql(@"SELECT Id INTO @userId FROM AspNetUsers LIMIT 1;
INSERT INTO OwnedMachines (AcquisitionDate, Status, Trade, Boxed, Manuals, MachineId, UserId)
SELECT a.date, a.status, a.trade, a.boxed, a.manuals, a.db_id, @userId FROM owned_computers AS a;");

            migrationBuilder.DropTable("owned_computers");

            migrationBuilder.Sql(@"SELECT Id INTO @userId FROM AspNetUsers LIMIT 1;
INSERT INTO OwnedMachines (AcquisitionDate, Status, Trade, Boxed, Manuals, MachineId, UserId)
SELECT a.date, a.status, a.trade, a.boxed, a.manuals, a.db_id + 356, @userId FROM owned_consoles AS a;");

            migrationBuilder.DropTable("owned_consoles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("GpusByOwnedMachine");

            migrationBuilder.DropTable("MemoryByOwnedMachine");

            migrationBuilder.DropTable("OwnedMachinePhotos");

            migrationBuilder.DropTable("ProcessorsByOwnedMachine");

            migrationBuilder.DropTable("SoundByOwnedMachine");

            migrationBuilder.DropTable("StorageByOwnedMachine");

            migrationBuilder.DropTable("OwnedMachines");

            migrationBuilder.CreateTable("owned_computers",
                                         table => new
                                         {
                                             id =
                                                 table.Column<int>("int(11)")
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.IdentityColumn),
                                             boxed =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             cap1 =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             cap2 =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             cpu1 =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             cpu2 =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             date =
                                                 table.Column<string>("varchar(20)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             db_id =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             disk1 =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             disk2 =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             manuals =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             mhz1 =
                                                 table.Column<decimal>("decimal(10,0)", nullable: false,
                                                                       defaultValueSql: "'0'"),
                                             mhz2 =
                                                 table.Column<decimal>("decimal(10,0)", nullable: false,
                                                                       defaultValueSql: "'0'"),
                                             ram =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             rigid =
                                                 table.Column<string>("varchar(64)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             status =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             trade = table.Column<int>("int(11)", nullable: false,
                                                                       defaultValueSql: "'0'"),
                                             vram = table.Column<int>("int(11)", nullable: false,
                                                                      defaultValueSql: "'0'")
                                         },
                                         constraints: table => { table.PrimaryKey("PK_owned_computers", x => x.id); });

            migrationBuilder.CreateTable("owned_consoles",
                                         table => new
                                         {
                                             id =
                                                 table.Column<int>("int(11)")
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.IdentityColumn),
                                             boxed =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             date =
                                                 table.Column<string>("char(20)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             db_id =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             manuals =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             status =
                                                 table.Column<int>("int(11)", nullable: false,
                                                                   defaultValueSql: "'0'"),
                                             trade = table.Column<int>("int(11)", nullable: false,
                                                                       defaultValueSql: "'0'")
                                         },
                                         constraints: table => { table.PrimaryKey("PK_owned_consoles", x => x.id); });

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
        }
    }
}
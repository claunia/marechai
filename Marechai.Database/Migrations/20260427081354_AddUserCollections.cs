using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GpusByOwnedMachine");

            migrationBuilder.DropTable(
                name: "MemoryByOwnedMachine");

            migrationBuilder.DropTable(
                name: "OwnedMachinePhotos");

            migrationBuilder.DropTable(
                name: "ProcessorsByOwnedMachine");

            migrationBuilder.DropTable(
                name: "SoundByOwnedMachine");

            migrationBuilder.DropTable(
                name: "StorageByOwnedMachine");

            migrationBuilder.CreateTable(
                name: "CollectedBooks",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BookId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectedBooks", x => new { x.UserId, x.BookId });
                    table.ForeignKey(
                        name: "FK_CollectedBooks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectedBooks_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CollectedDocuments",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectedDocuments", x => new { x.UserId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_CollectedDocuments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectedDocuments_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CollectedSoftwareReleases",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareReleaseId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectedSoftwareReleases", x => new { x.UserId, x.SoftwareReleaseId });
                    table.ForeignKey(
                        name: "FK_CollectedSoftwareReleases_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectedSoftwareReleases_SoftwareReleases_SoftwareReleaseId",
                        column: x => x.SoftwareReleaseId,
                        principalTable: "SoftwareReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CollectedBooks_BookId",
                table: "CollectedBooks",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectedDocuments_DocumentId",
                table: "CollectedDocuments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectedSoftwareReleases_SoftwareReleaseId",
                table: "CollectedSoftwareReleases",
                column: "SoftwareReleaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectedBooks");

            migrationBuilder.DropTable(
                name: "CollectedDocuments");

            migrationBuilder.DropTable(
                name: "CollectedSoftwareReleases");

            migrationBuilder.CreateTable(
                name: "GpusByOwnedMachine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GpuId = table.Column<int>(type: "int(11)", nullable: false),
                    OwnedMachineId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GpusByOwnedMachine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GpusByOwnedMachine_OwnedMachines_OwnedMachineId",
                        column: x => x.OwnedMachineId,
                        principalTable: "OwnedMachines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GpusByOwnedMachine_gpus_GpuId",
                        column: x => x.GpuId,
                        principalTable: "gpus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MemoryByOwnedMachine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OwnedMachineId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Speed = table.Column<double>(type: "double", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    Usage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoryByOwnedMachine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemoryByOwnedMachine_OwnedMachines_OwnedMachineId",
                        column: x => x.OwnedMachineId,
                        principalTable: "OwnedMachines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OwnedMachinePhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LicenseId = table.Column<int>(type: "int", nullable: false),
                    OwnedMachineId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Aperture = table.Column<double>(type: "double", nullable: true),
                    Author = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CameraManufacturer = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CameraModel = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ColorSpace = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Comments = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Contrast = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DigitalZoomRatio = table.Column<double>(type: "double", nullable: true),
                    ExifVersion = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExposureMethod = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    ExposureProgram = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    ExposureTime = table.Column<double>(type: "double", nullable: true),
                    Flash = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Focal = table.Column<double>(type: "double", nullable: true),
                    FocalLength = table.Column<double>(type: "double", nullable: true),
                    FocalLengthEquivalent = table.Column<double>(type: "double", nullable: true),
                    HorizontalResolution = table.Column<double>(type: "double", nullable: true),
                    IsoRating = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Lens = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LightSource = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    MeteringMode = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Orientation = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    OriginalExtension = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResolutionUnit = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Saturation = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    SceneCaptureType = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    SensingMethod = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Sharpness = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    SoftwareUsed = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubjectDistanceRange = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    UploadDate = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false),
                    VerticalResolution = table.Column<double>(type: "double", nullable: true),
                    WhiteBalance = table.Column<ushort>(type: "smallint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnedMachinePhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OwnedMachinePhotos_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OwnedMachinePhotos_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OwnedMachinePhotos_OwnedMachines_OwnedMachineId",
                        column: x => x.OwnedMachineId,
                        principalTable: "OwnedMachines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProcessorsByOwnedMachine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OwnedMachineId = table.Column<long>(type: "bigint", nullable: false),
                    ProcessorId = table.Column<int>(type: "int(11)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Speed = table.Column<float>(type: "float", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessorsByOwnedMachine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessorsByOwnedMachine_OwnedMachines_OwnedMachineId",
                        column: x => x.OwnedMachineId,
                        principalTable: "OwnedMachines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessorsByOwnedMachine_processors_ProcessorId",
                        column: x => x.ProcessorId,
                        principalTable: "processors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoundByOwnedMachine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OwnedMachineId = table.Column<long>(type: "bigint", nullable: false),
                    SoundSynthId = table.Column<int>(type: "int(11)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoundByOwnedMachine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoundByOwnedMachine_OwnedMachines_OwnedMachineId",
                        column: x => x.OwnedMachineId,
                        principalTable: "OwnedMachines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoundByOwnedMachine_sound_synths_SoundSynthId",
                        column: x => x.SoundSynthId,
                        principalTable: "sound_synths",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StorageByOwnedMachine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OwnedMachineId = table.Column<long>(type: "bigint", nullable: false),
                    Capacity = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Interface = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageByOwnedMachine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageByOwnedMachine_OwnedMachines_OwnedMachineId",
                        column: x => x.OwnedMachineId,
                        principalTable: "OwnedMachines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GpusByOwnedMachine_GpuId",
                table: "GpusByOwnedMachine",
                column: "GpuId");

            migrationBuilder.CreateIndex(
                name: "IX_GpusByOwnedMachine_OwnedMachineId",
                table: "GpusByOwnedMachine",
                column: "OwnedMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryByOwnedMachine_OwnedMachineId",
                table: "MemoryByOwnedMachine",
                column: "OwnedMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryByOwnedMachine_Size",
                table: "MemoryByOwnedMachine",
                column: "Size");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryByOwnedMachine_Speed",
                table: "MemoryByOwnedMachine",
                column: "Speed");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryByOwnedMachine_Type",
                table: "MemoryByOwnedMachine",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryByOwnedMachine_Usage",
                table: "MemoryByOwnedMachine",
                column: "Usage");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_Aperture",
                table: "OwnedMachinePhotos",
                column: "Aperture");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_Author",
                table: "OwnedMachinePhotos",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_CameraManufacturer",
                table: "OwnedMachinePhotos",
                column: "CameraManufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_CameraModel",
                table: "OwnedMachinePhotos",
                column: "CameraModel");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_ColorSpace",
                table: "OwnedMachinePhotos",
                column: "ColorSpace");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_Comments",
                table: "OwnedMachinePhotos",
                column: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_Contrast",
                table: "OwnedMachinePhotos",
                column: "Contrast");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_CreationDate",
                table: "OwnedMachinePhotos",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_DigitalZoomRatio",
                table: "OwnedMachinePhotos",
                column: "DigitalZoomRatio");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_ExifVersion",
                table: "OwnedMachinePhotos",
                column: "ExifVersion");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_ExposureMethod",
                table: "OwnedMachinePhotos",
                column: "ExposureMethod");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_ExposureProgram",
                table: "OwnedMachinePhotos",
                column: "ExposureProgram");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_ExposureTime",
                table: "OwnedMachinePhotos",
                column: "ExposureTime");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_Flash",
                table: "OwnedMachinePhotos",
                column: "Flash");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_Focal",
                table: "OwnedMachinePhotos",
                column: "Focal");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_FocalLength",
                table: "OwnedMachinePhotos",
                column: "FocalLength");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_FocalLengthEquivalent",
                table: "OwnedMachinePhotos",
                column: "FocalLengthEquivalent");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_HorizontalResolution",
                table: "OwnedMachinePhotos",
                column: "HorizontalResolution");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_IsoRating",
                table: "OwnedMachinePhotos",
                column: "IsoRating");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_Lens",
                table: "OwnedMachinePhotos",
                column: "Lens");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_LicenseId",
                table: "OwnedMachinePhotos",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_LightSource",
                table: "OwnedMachinePhotos",
                column: "LightSource");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_MeteringMode",
                table: "OwnedMachinePhotos",
                column: "MeteringMode");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_Orientation",
                table: "OwnedMachinePhotos",
                column: "Orientation");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_OwnedMachineId",
                table: "OwnedMachinePhotos",
                column: "OwnedMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_ResolutionUnit",
                table: "OwnedMachinePhotos",
                column: "ResolutionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_Saturation",
                table: "OwnedMachinePhotos",
                column: "Saturation");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_SceneCaptureType",
                table: "OwnedMachinePhotos",
                column: "SceneCaptureType");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_SensingMethod",
                table: "OwnedMachinePhotos",
                column: "SensingMethod");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_Sharpness",
                table: "OwnedMachinePhotos",
                column: "Sharpness");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_SoftwareUsed",
                table: "OwnedMachinePhotos",
                column: "SoftwareUsed");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_SubjectDistanceRange",
                table: "OwnedMachinePhotos",
                column: "SubjectDistanceRange");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_UploadDate",
                table: "OwnedMachinePhotos",
                column: "UploadDate");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_UserId",
                table: "OwnedMachinePhotos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_VerticalResolution",
                table: "OwnedMachinePhotos",
                column: "VerticalResolution");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedMachinePhotos_WhiteBalance",
                table: "OwnedMachinePhotos",
                column: "WhiteBalance");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorsByOwnedMachine_OwnedMachineId",
                table: "ProcessorsByOwnedMachine",
                column: "OwnedMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorsByOwnedMachine_ProcessorId",
                table: "ProcessorsByOwnedMachine",
                column: "ProcessorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorsByOwnedMachine_Speed",
                table: "ProcessorsByOwnedMachine",
                column: "Speed");

            migrationBuilder.CreateIndex(
                name: "IX_SoundByOwnedMachine_OwnedMachineId",
                table: "SoundByOwnedMachine",
                column: "OwnedMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_SoundByOwnedMachine_SoundSynthId",
                table: "SoundByOwnedMachine",
                column: "SoundSynthId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageByOwnedMachine_Capacity",
                table: "StorageByOwnedMachine",
                column: "Capacity");

            migrationBuilder.CreateIndex(
                name: "IX_StorageByOwnedMachine_Interface",
                table: "StorageByOwnedMachine",
                column: "Interface");

            migrationBuilder.CreateIndex(
                name: "IX_StorageByOwnedMachine_OwnedMachineId",
                table: "StorageByOwnedMachine",
                column: "OwnedMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageByOwnedMachine_Type",
                table: "StorageByOwnedMachine",
                column: "Type");
        }
    }
}

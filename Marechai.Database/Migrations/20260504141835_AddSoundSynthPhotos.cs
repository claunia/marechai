using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoundSynthPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessorPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Source = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProcessorId = table.Column<int>(type: "int(11)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
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
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DigitalZoomRatio = table.Column<double>(type: "double", nullable: true),
                    ExifVersion = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExposureTime = table.Column<double>(type: "double", nullable: true),
                    ExposureMethod = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    ExposureProgram = table.Column<ushort>(type: "smallint unsigned", nullable: true),
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
                    ResolutionUnit = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Orientation = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Saturation = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    SceneCaptureType = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    SensingMethod = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Sharpness = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    SoftwareUsed = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubjectDistanceRange = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    UploadDate = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false),
                    VerticalResolution = table.Column<double>(type: "double", nullable: true),
                    WhiteBalance = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    OriginalExtension = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LicenseId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessorPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessorPhotos_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProcessorPhotos_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessorPhotos_processors_ProcessorId",
                        column: x => x.ProcessorId,
                        principalTable: "processors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoundSynthPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Source = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoundSynthId = table.Column<int>(type: "int(11)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
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
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DigitalZoomRatio = table.Column<double>(type: "double", nullable: true),
                    ExifVersion = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExposureTime = table.Column<double>(type: "double", nullable: true),
                    ExposureMethod = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    ExposureProgram = table.Column<ushort>(type: "smallint unsigned", nullable: true),
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
                    ResolutionUnit = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Orientation = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Saturation = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    SceneCaptureType = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    SensingMethod = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Sharpness = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    SoftwareUsed = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubjectDistanceRange = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    UploadDate = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false),
                    VerticalResolution = table.Column<double>(type: "double", nullable: true),
                    WhiteBalance = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    OriginalExtension = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LicenseId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoundSynthPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoundSynthPhotos_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SoundSynthPhotos_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoundSynthPhotos_sound_synths_SoundSynthId",
                        column: x => x.SoundSynthId,
                        principalTable: "sound_synths",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_Aperture",
                table: "ProcessorPhotos",
                column: "Aperture");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_Author",
                table: "ProcessorPhotos",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_CameraManufacturer",
                table: "ProcessorPhotos",
                column: "CameraManufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_CameraModel",
                table: "ProcessorPhotos",
                column: "CameraModel");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_ColorSpace",
                table: "ProcessorPhotos",
                column: "ColorSpace");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_Comments",
                table: "ProcessorPhotos",
                column: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_Contrast",
                table: "ProcessorPhotos",
                column: "Contrast");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_CreationDate",
                table: "ProcessorPhotos",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_DigitalZoomRatio",
                table: "ProcessorPhotos",
                column: "DigitalZoomRatio");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_ExifVersion",
                table: "ProcessorPhotos",
                column: "ExifVersion");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_ExposureMethod",
                table: "ProcessorPhotos",
                column: "ExposureMethod");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_ExposureProgram",
                table: "ProcessorPhotos",
                column: "ExposureProgram");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_ExposureTime",
                table: "ProcessorPhotos",
                column: "ExposureTime");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_Flash",
                table: "ProcessorPhotos",
                column: "Flash");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_Focal",
                table: "ProcessorPhotos",
                column: "Focal");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_FocalLength",
                table: "ProcessorPhotos",
                column: "FocalLength");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_FocalLengthEquivalent",
                table: "ProcessorPhotos",
                column: "FocalLengthEquivalent");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_HorizontalResolution",
                table: "ProcessorPhotos",
                column: "HorizontalResolution");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_IsoRating",
                table: "ProcessorPhotos",
                column: "IsoRating");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_Lens",
                table: "ProcessorPhotos",
                column: "Lens");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_LicenseId",
                table: "ProcessorPhotos",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_LightSource",
                table: "ProcessorPhotos",
                column: "LightSource");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_MeteringMode",
                table: "ProcessorPhotos",
                column: "MeteringMode");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_Orientation",
                table: "ProcessorPhotos",
                column: "Orientation");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_ProcessorId",
                table: "ProcessorPhotos",
                column: "ProcessorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_ResolutionUnit",
                table: "ProcessorPhotos",
                column: "ResolutionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_Saturation",
                table: "ProcessorPhotos",
                column: "Saturation");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_SceneCaptureType",
                table: "ProcessorPhotos",
                column: "SceneCaptureType");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_SensingMethod",
                table: "ProcessorPhotos",
                column: "SensingMethod");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_Sharpness",
                table: "ProcessorPhotos",
                column: "Sharpness");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_SoftwareUsed",
                table: "ProcessorPhotos",
                column: "SoftwareUsed");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_SubjectDistanceRange",
                table: "ProcessorPhotos",
                column: "SubjectDistanceRange");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_UploadDate",
                table: "ProcessorPhotos",
                column: "UploadDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_UserId",
                table: "ProcessorPhotos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_VerticalResolution",
                table: "ProcessorPhotos",
                column: "VerticalResolution");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_WhiteBalance",
                table: "ProcessorPhotos",
                column: "WhiteBalance");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_Aperture",
                table: "SoundSynthPhotos",
                column: "Aperture");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_Author",
                table: "SoundSynthPhotos",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_CameraManufacturer",
                table: "SoundSynthPhotos",
                column: "CameraManufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_CameraModel",
                table: "SoundSynthPhotos",
                column: "CameraModel");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_ColorSpace",
                table: "SoundSynthPhotos",
                column: "ColorSpace");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_Comments",
                table: "SoundSynthPhotos",
                column: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_Contrast",
                table: "SoundSynthPhotos",
                column: "Contrast");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_CreationDate",
                table: "SoundSynthPhotos",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_DigitalZoomRatio",
                table: "SoundSynthPhotos",
                column: "DigitalZoomRatio");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_ExifVersion",
                table: "SoundSynthPhotos",
                column: "ExifVersion");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_ExposureMethod",
                table: "SoundSynthPhotos",
                column: "ExposureMethod");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_ExposureProgram",
                table: "SoundSynthPhotos",
                column: "ExposureProgram");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_ExposureTime",
                table: "SoundSynthPhotos",
                column: "ExposureTime");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_Flash",
                table: "SoundSynthPhotos",
                column: "Flash");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_Focal",
                table: "SoundSynthPhotos",
                column: "Focal");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_FocalLength",
                table: "SoundSynthPhotos",
                column: "FocalLength");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_FocalLengthEquivalent",
                table: "SoundSynthPhotos",
                column: "FocalLengthEquivalent");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_HorizontalResolution",
                table: "SoundSynthPhotos",
                column: "HorizontalResolution");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_IsoRating",
                table: "SoundSynthPhotos",
                column: "IsoRating");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_Lens",
                table: "SoundSynthPhotos",
                column: "Lens");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_LicenseId",
                table: "SoundSynthPhotos",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_LightSource",
                table: "SoundSynthPhotos",
                column: "LightSource");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_MeteringMode",
                table: "SoundSynthPhotos",
                column: "MeteringMode");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_Orientation",
                table: "SoundSynthPhotos",
                column: "Orientation");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_ResolutionUnit",
                table: "SoundSynthPhotos",
                column: "ResolutionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_Saturation",
                table: "SoundSynthPhotos",
                column: "Saturation");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_SceneCaptureType",
                table: "SoundSynthPhotos",
                column: "SceneCaptureType");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_SensingMethod",
                table: "SoundSynthPhotos",
                column: "SensingMethod");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_Sharpness",
                table: "SoundSynthPhotos",
                column: "Sharpness");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_SoftwareUsed",
                table: "SoundSynthPhotos",
                column: "SoftwareUsed");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_SoundSynthId",
                table: "SoundSynthPhotos",
                column: "SoundSynthId");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_SubjectDistanceRange",
                table: "SoundSynthPhotos",
                column: "SubjectDistanceRange");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_UploadDate",
                table: "SoundSynthPhotos",
                column: "UploadDate");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_UserId",
                table: "SoundSynthPhotos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_VerticalResolution",
                table: "SoundSynthPhotos",
                column: "VerticalResolution");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_WhiteBalance",
                table: "SoundSynthPhotos",
                column: "WhiteBalance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessorPhotos");

            migrationBuilder.DropTable(
                name: "SoundSynthPhotos");
        }
    }
}

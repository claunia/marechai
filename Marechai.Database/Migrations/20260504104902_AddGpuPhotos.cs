using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddGpuPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GpuPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Source = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GpuId = table.Column<int>(type: "int(11)", nullable: false),
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
                    table.PrimaryKey("PK_GpuPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GpuPhotos_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GpuPhotos_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GpuPhotos_gpus_GpuId",
                        column: x => x.GpuId,
                        principalTable: "gpus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_Aperture",
                table: "GpuPhotos",
                column: "Aperture");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_Author",
                table: "GpuPhotos",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_CameraManufacturer",
                table: "GpuPhotos",
                column: "CameraManufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_CameraModel",
                table: "GpuPhotos",
                column: "CameraModel");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_ColorSpace",
                table: "GpuPhotos",
                column: "ColorSpace");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_Comments",
                table: "GpuPhotos",
                column: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_Contrast",
                table: "GpuPhotos",
                column: "Contrast");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_CreationDate",
                table: "GpuPhotos",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_DigitalZoomRatio",
                table: "GpuPhotos",
                column: "DigitalZoomRatio");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_ExifVersion",
                table: "GpuPhotos",
                column: "ExifVersion");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_ExposureMethod",
                table: "GpuPhotos",
                column: "ExposureMethod");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_ExposureProgram",
                table: "GpuPhotos",
                column: "ExposureProgram");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_ExposureTime",
                table: "GpuPhotos",
                column: "ExposureTime");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_Flash",
                table: "GpuPhotos",
                column: "Flash");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_Focal",
                table: "GpuPhotos",
                column: "Focal");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_FocalLength",
                table: "GpuPhotos",
                column: "FocalLength");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_FocalLengthEquivalent",
                table: "GpuPhotos",
                column: "FocalLengthEquivalent");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_GpuId",
                table: "GpuPhotos",
                column: "GpuId");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_HorizontalResolution",
                table: "GpuPhotos",
                column: "HorizontalResolution");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_IsoRating",
                table: "GpuPhotos",
                column: "IsoRating");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_Lens",
                table: "GpuPhotos",
                column: "Lens");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_LicenseId",
                table: "GpuPhotos",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_LightSource",
                table: "GpuPhotos",
                column: "LightSource");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_MeteringMode",
                table: "GpuPhotos",
                column: "MeteringMode");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_Orientation",
                table: "GpuPhotos",
                column: "Orientation");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_ResolutionUnit",
                table: "GpuPhotos",
                column: "ResolutionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_Saturation",
                table: "GpuPhotos",
                column: "Saturation");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_SceneCaptureType",
                table: "GpuPhotos",
                column: "SceneCaptureType");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_SensingMethod",
                table: "GpuPhotos",
                column: "SensingMethod");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_Sharpness",
                table: "GpuPhotos",
                column: "Sharpness");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_SoftwareUsed",
                table: "GpuPhotos",
                column: "SoftwareUsed");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_SubjectDistanceRange",
                table: "GpuPhotos",
                column: "SubjectDistanceRange");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_UploadDate",
                table: "GpuPhotos",
                column: "UploadDate");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_UserId",
                table: "GpuPhotos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_VerticalResolution",
                table: "GpuPhotos",
                column: "VerticalResolution");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_WhiteBalance",
                table: "GpuPhotos",
                column: "WhiteBalance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GpuPhotos");
        }
    }
}

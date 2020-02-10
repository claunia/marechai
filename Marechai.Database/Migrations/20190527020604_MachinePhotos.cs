using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class MachinePhotos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>("Discriminator", "AspNetUsers", nullable: false, defaultValue: "");

            migrationBuilder.CreateTable("MachinePhotos",
                                         table => new
                                         {
                                             Id                    = table.Column<Guid>(),
                                             Author                = table.Column<string>(nullable: true),
                                             CameraManufacturer    = table.Column<string>(nullable: true),
                                             CameraModel           = table.Column<string>(nullable: true),
                                             ColorSpace            = table.Column<string>(nullable: true),
                                             Comments              = table.Column<string>(nullable: true),
                                             Contrast              = table.Column<string>(nullable: true),
                                             CreationDate          = table.Column<DateTime>(nullable: true),
                                             DigitalZoomRatio      = table.Column<double>(nullable: true),
                                             ExifVersion           = table.Column<string>(nullable: true),
                                             Exposure              = table.Column<double>(nullable: true),
                                             ExposureMethod        = table.Column<string>(nullable: true),
                                             ExposureProgram       = table.Column<string>(nullable: true),
                                             Flash                 = table.Column<string>(nullable: true),
                                             Focal                 = table.Column<int>(nullable: true),
                                             FocalLength           = table.Column<int>(nullable: true),
                                             FocalLengthEquivalent = table.Column<string>(nullable: true),
                                             HorizontalResolution  = table.Column<int>(nullable: true),
                                             IsoRating             = table.Column<int>(nullable: true),
                                             Lens                  = table.Column<string>(nullable: true),
                                             License               = table.Column<string>(nullable: true),
                                             LightSource           = table.Column<string>(nullable: true),
                                             MeteringMode          = table.Column<string>(nullable: true),
                                             Orientation           = table.Column<string>(nullable: true),
                                             PixelComposition      = table.Column<string>(nullable: true),
                                             Saturation            = table.Column<string>(nullable: true),
                                             SceneCaptureType      = table.Column<string>(nullable: true),
                                             SceneControl          = table.Column<string>(nullable: true),
                                             SensingMethod         = table.Column<string>(nullable: true),
                                             Sharpness             = table.Column<string>(nullable: true),
                                             SoftwareUsed          = table.Column<string>(nullable: true),
                                             SubjectDistanceRange  = table.Column<string>(nullable: true),
                                             UploadDate =
                                                 table.Column<DateTime>()
                                                      .Annotation("MySql:ValueGenerationStrategy",
                                                                  MySqlValueGenerationStrategy.ComputedColumn),
                                             VerticalResolution = table.Column<int>(nullable: true),
                                             WhiteBalance       = table.Column<string>(nullable: true),
                                             UserId             = table.Column<string>(nullable: true),
                                             MachineId          = table.Column<int>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_MachinePhotos", x => x.Id);
                                             table.ForeignKey("FK_MachinePhotos_machines_MachineId", x => x.MachineId,
                                                              "machines", "id", onDelete: ReferentialAction.Cascade);
                                             table.ForeignKey("FK_MachinePhotos_AspNetUsers_UserId", x => x.UserId,
                                                              "AspNetUsers", "Id", onDelete: ReferentialAction.SetNull);
                                         });

            migrationBuilder.CreateIndex("IX_MachinePhotos_Author", "MachinePhotos", "Author");

            migrationBuilder.CreateIndex("IX_MachinePhotos_CameraManufacturer", "MachinePhotos", "CameraManufacturer");

            migrationBuilder.CreateIndex("IX_MachinePhotos_CameraModel", "MachinePhotos", "CameraModel");

            migrationBuilder.CreateIndex("IX_MachinePhotos_ColorSpace", "MachinePhotos", "ColorSpace");

            migrationBuilder.CreateIndex("IX_MachinePhotos_Comments", "MachinePhotos", "Comments");

            migrationBuilder.CreateIndex("IX_MachinePhotos_Contrast", "MachinePhotos", "Contrast");

            migrationBuilder.CreateIndex("IX_MachinePhotos_CreationDate", "MachinePhotos", "CreationDate");

            migrationBuilder.CreateIndex("IX_MachinePhotos_DigitalZoomRatio", "MachinePhotos", "DigitalZoomRatio");

            migrationBuilder.CreateIndex("IX_MachinePhotos_ExifVersion", "MachinePhotos", "ExifVersion");

            migrationBuilder.CreateIndex("IX_MachinePhotos_Exposure", "MachinePhotos", "Exposure");

            migrationBuilder.CreateIndex("IX_MachinePhotos_ExposureMethod", "MachinePhotos", "ExposureMethod");

            migrationBuilder.CreateIndex("IX_MachinePhotos_ExposureProgram", "MachinePhotos", "ExposureProgram");

            migrationBuilder.CreateIndex("IX_MachinePhotos_Flash", "MachinePhotos", "Flash");

            migrationBuilder.CreateIndex("IX_MachinePhotos_Focal", "MachinePhotos", "Focal");

            migrationBuilder.CreateIndex("IX_MachinePhotos_FocalLength", "MachinePhotos", "FocalLength");

            migrationBuilder.CreateIndex("IX_MachinePhotos_FocalLengthEquivalent", "MachinePhotos",
                                         "FocalLengthEquivalent");

            migrationBuilder.CreateIndex("IX_MachinePhotos_HorizontalResolution", "MachinePhotos",
                                         "HorizontalResolution");

            migrationBuilder.CreateIndex("IX_MachinePhotos_IsoRating", "MachinePhotos", "IsoRating");

            migrationBuilder.CreateIndex("IX_MachinePhotos_Lens", "MachinePhotos", "Lens");

            migrationBuilder.CreateIndex("IX_MachinePhotos_License", "MachinePhotos", "License");

            migrationBuilder.CreateIndex("IX_MachinePhotos_LightSource", "MachinePhotos", "LightSource");

            migrationBuilder.CreateIndex("IX_MachinePhotos_MachineId", "MachinePhotos", "MachineId");

            migrationBuilder.CreateIndex("IX_MachinePhotos_MeteringMode", "MachinePhotos", "MeteringMode");

            migrationBuilder.CreateIndex("IX_MachinePhotos_Orientation", "MachinePhotos", "Orientation");

            migrationBuilder.CreateIndex("IX_MachinePhotos_PixelComposition", "MachinePhotos", "PixelComposition");

            migrationBuilder.CreateIndex("IX_MachinePhotos_Saturation", "MachinePhotos", "Saturation");

            migrationBuilder.CreateIndex("IX_MachinePhotos_SceneCaptureType", "MachinePhotos", "SceneCaptureType");

            migrationBuilder.CreateIndex("IX_MachinePhotos_SceneControl", "MachinePhotos", "SceneControl");

            migrationBuilder.CreateIndex("IX_MachinePhotos_SensingMethod", "MachinePhotos", "SensingMethod");

            migrationBuilder.CreateIndex("IX_MachinePhotos_Sharpness", "MachinePhotos", "Sharpness");

            migrationBuilder.CreateIndex("IX_MachinePhotos_SoftwareUsed", "MachinePhotos", "SoftwareUsed");

            migrationBuilder.CreateIndex("IX_MachinePhotos_SubjectDistanceRange", "MachinePhotos",
                                         "SubjectDistanceRange");

            migrationBuilder.CreateIndex("IX_MachinePhotos_UploadDate", "MachinePhotos", "UploadDate");

            migrationBuilder.CreateIndex("IX_MachinePhotos_UserId", "MachinePhotos", "UserId");

            migrationBuilder.CreateIndex("IX_MachinePhotos_VerticalResolution", "MachinePhotos", "VerticalResolution");

            migrationBuilder.CreateIndex("IX_MachinePhotos_WhiteBalance", "MachinePhotos", "WhiteBalance");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("MachinePhotos");

            migrationBuilder.DropColumn("Discriminator", "AspNetUsers");
        }
    }
}
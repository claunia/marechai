using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class ChangeMachinePhotoFieldTypesToExifTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_MachinePhotos_PixelComposition", "MachinePhotos");

            migrationBuilder.DropIndex("IX_MachinePhotos_SceneControl", "MachinePhotos");

            migrationBuilder.DropColumn("PixelComposition", "MachinePhotos");

            migrationBuilder.DropColumn("SceneControl", "MachinePhotos");

            migrationBuilder.AlterColumn<ushort>("WhiteBalance", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<double>("VerticalResolution", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(int), oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("SubjectDistanceRange", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("SensingMethod", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("SceneCaptureType", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("Saturation", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("Orientation", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("MeteringMode", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("LightSource", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("IsoRating", "MachinePhotos", nullable: true, oldClrType: typeof(int),
                                                 oldNullable: true);

            migrationBuilder.AlterColumn<double>("HorizontalResolution", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(int), oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("FocalLengthEquivalent", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<double>("FocalLength", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(int), oldNullable: true);

            migrationBuilder.AlterColumn<double>("Focal", "MachinePhotos", nullable: true, oldClrType: typeof(int),
                                                 oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("Flash", "MachinePhotos", nullable: true, oldClrType: typeof(string),
                                                 oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("ExposureProgram", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("ExposureMethod", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<string>("Exposure", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(double), oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("Contrast", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("ColorSpace", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AddColumn<ushort>("ResolutionUnit", "MachinePhotos", nullable: true);

            migrationBuilder.CreateIndex("IX_MachinePhotos_ResolutionUnit", "MachinePhotos", "ResolutionUnit");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_MachinePhotos_ResolutionUnit", "MachinePhotos");

            migrationBuilder.DropColumn("ResolutionUnit", "MachinePhotos");

            migrationBuilder.AlterColumn<string>("WhiteBalance", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldNullable: true);

            migrationBuilder.AlterColumn<int>("VerticalResolution", "MachinePhotos", nullable: true,
                                              oldClrType: typeof(double), oldNullable: true);

            migrationBuilder.AlterColumn<string>("SubjectDistanceRange", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldNullable: true);

            migrationBuilder.AlterColumn<string>("SensingMethod", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldNullable: true);

            migrationBuilder.AlterColumn<string>("SceneCaptureType", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldNullable: true);

            migrationBuilder.AlterColumn<string>("Saturation", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldNullable: true);

            migrationBuilder.AlterColumn<string>("Orientation", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldNullable: true);

            migrationBuilder.AlterColumn<string>("MeteringMode", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldNullable: true);

            migrationBuilder.AlterColumn<string>("LightSource", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldNullable: true);

            migrationBuilder.AlterColumn<int>("IsoRating", "MachinePhotos", nullable: true, oldClrType: typeof(ushort),
                                              oldNullable: true);

            migrationBuilder.AlterColumn<int>("HorizontalResolution", "MachinePhotos", nullable: true,
                                              oldClrType: typeof(double), oldNullable: true);

            migrationBuilder.AlterColumn<string>("FocalLengthEquivalent", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldNullable: true);

            migrationBuilder.AlterColumn<int>("FocalLength", "MachinePhotos", nullable: true,
                                              oldClrType: typeof(double), oldNullable: true);

            migrationBuilder.AlterColumn<int>("Focal", "MachinePhotos", nullable: true, oldClrType: typeof(double),
                                              oldNullable: true);

            migrationBuilder.AlterColumn<string>("Flash", "MachinePhotos", nullable: true, oldClrType: typeof(ushort),
                                                 oldNullable: true);

            migrationBuilder.AlterColumn<string>("ExposureProgram", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldNullable: true);

            migrationBuilder.AlterColumn<string>("ExposureMethod", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldNullable: true);

            migrationBuilder.AlterColumn<double>("Exposure", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

            migrationBuilder.AlterColumn<string>("Contrast", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldNullable: true);

            migrationBuilder.AlterColumn<string>("ColorSpace", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldNullable: true);

            migrationBuilder.AddColumn<string>("PixelComposition", "MachinePhotos", nullable: true);

            migrationBuilder.AddColumn<string>("SceneControl", "MachinePhotos", nullable: true);

            migrationBuilder.CreateIndex("IX_MachinePhotos_PixelComposition", "MachinePhotos", "PixelComposition");

            migrationBuilder.CreateIndex("IX_MachinePhotos_SceneControl", "MachinePhotos", "SceneControl");
        }
    }
}
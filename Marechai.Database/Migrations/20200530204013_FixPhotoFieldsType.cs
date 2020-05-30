using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class FixPhotoFieldsType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_OwnedMachinePhotos_Exposure", "OwnedMachinePhotos");

            migrationBuilder.DropIndex("IX_MachinePhotos_Exposure", "MachinePhotos");

            migrationBuilder.DropColumn("Exposure", "OwnedMachinePhotos");

            migrationBuilder.DropColumn("Exposure", "MachinePhotos");

            migrationBuilder.AddColumn<double>("Aperture", "OwnedMachinePhotos", nullable: true);

            migrationBuilder.AddColumn<double>("ExposureTime", "OwnedMachinePhotos", nullable: true);

            migrationBuilder.AddColumn<string>("OriginalExtension", "OwnedMachinePhotos", nullable: true);

            migrationBuilder.AddColumn<double>("Aperture", "MachinePhotos", nullable: true);

            migrationBuilder.AddColumn<double>("ExposureTime", "MachinePhotos", nullable: true);

            migrationBuilder.AddColumn<string>("OriginalExtension", "MachinePhotos", nullable: true);

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_Aperture", "OwnedMachinePhotos", "Aperture");

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_ExposureTime", "OwnedMachinePhotos", "ExposureTime");

            migrationBuilder.CreateIndex("IX_MachinePhotos_Aperture", "MachinePhotos", "Aperture");

            migrationBuilder.CreateIndex("IX_MachinePhotos_ExposureTime", "MachinePhotos", "ExposureTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_OwnedMachinePhotos_Aperture", "OwnedMachinePhotos");

            migrationBuilder.DropIndex("IX_OwnedMachinePhotos_ExposureTime", "OwnedMachinePhotos");

            migrationBuilder.DropIndex("IX_MachinePhotos_Aperture", "MachinePhotos");

            migrationBuilder.DropIndex("IX_MachinePhotos_ExposureTime", "MachinePhotos");

            migrationBuilder.DropColumn("Aperture", "OwnedMachinePhotos");

            migrationBuilder.DropColumn("ExposureTime", "OwnedMachinePhotos");

            migrationBuilder.DropColumn("OriginalExtension", "OwnedMachinePhotos");

            migrationBuilder.DropColumn("Aperture", "MachinePhotos");

            migrationBuilder.DropColumn("ExposureTime", "MachinePhotos");

            migrationBuilder.DropColumn("OriginalExtension", "MachinePhotos");

            migrationBuilder.AddColumn<string>("Exposure", "OwnedMachinePhotos", "varchar(255) CHARACTER SET utf8mb4",
                                               nullable: true);

            migrationBuilder.AddColumn<string>("Exposure", "MachinePhotos", "varchar(255) CHARACTER SET utf8mb4",
                                               nullable: true);

            migrationBuilder.CreateIndex("IX_OwnedMachinePhotos_Exposure", "OwnedMachinePhotos", "Exposure");

            migrationBuilder.CreateIndex("IX_MachinePhotos_Exposure", "MachinePhotos", "Exposure");
        }
    }
}
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class UseLicenseInMachinePhoto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_MachinePhotos_License", "MachinePhotos");

            migrationBuilder.DropColumn("License", "MachinePhotos");

            migrationBuilder.AddColumn<int>("LicenseId", "MachinePhotos", nullable: false, defaultValue: 0);

            migrationBuilder.CreateIndex("IX_MachinePhotos_LicenseId", "MachinePhotos", "LicenseId");

            migrationBuilder.AddForeignKey("FK_MachinePhotos_Licenses_LicenseId", "MachinePhotos", "LicenseId",
                                           "Licenses", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_MachinePhotos_Licenses_LicenseId", "MachinePhotos");

            migrationBuilder.DropIndex("IX_MachinePhotos_LicenseId", "MachinePhotos");

            migrationBuilder.DropColumn("LicenseId", "MachinePhotos");

            migrationBuilder.AddColumn<string>("License", "MachinePhotos", nullable: true);

            migrationBuilder.CreateIndex("IX_MachinePhotos_License", "MachinePhotos", "License");
        }
    }
}
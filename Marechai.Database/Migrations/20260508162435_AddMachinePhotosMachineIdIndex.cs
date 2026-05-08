using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMachinePhotosMachineIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_MachinePhotos_MachineId",
                table: "MachinePhotos",
                newName: "idx_machine_photos_machine");

            migrationBuilder.CreateIndex(
                name: "idx_machine_photos_machine_created",
                table: "MachinePhotos",
                columns: new[] { "MachineId", "CreatedOn", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_machine_photos_machine_created",
                table: "MachinePhotos");

            migrationBuilder.RenameIndex(
                name: "idx_machine_photos_machine",
                table: "MachinePhotos",
                newName: "IX_MachinePhotos_MachineId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddGpuPhotosGpuIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_GpuPhotos_GpuId",
                table: "GpuPhotos",
                newName: "idx_gpu_photos_gpu");

            migrationBuilder.CreateIndex(
                name: "idx_gpu_photos_gpu_created",
                table: "GpuPhotos",
                columns: new[] { "GpuId", "CreatedOn", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_gpu_photos_gpu_created",
                table: "GpuPhotos");

            migrationBuilder.RenameIndex(
                name: "idx_gpu_photos_gpu",
                table: "GpuPhotos",
                newName: "IX_GpuPhotos_GpuId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessorPhotosProcessorIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_ProcessorPhotos_ProcessorId",
                table: "ProcessorPhotos",
                newName: "idx_processor_photos_processor");

            migrationBuilder.CreateIndex(
                name: "idx_processor_photos_processor_created",
                table: "ProcessorPhotos",
                columns: new[] { "ProcessorId", "CreatedOn", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_processor_photos_processor_created",
                table: "ProcessorPhotos");

            migrationBuilder.RenameIndex(
                name: "idx_processor_photos_processor",
                table: "ProcessorPhotos",
                newName: "IX_ProcessorPhotos_ProcessorId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class WidenPhotoCommentsToLongtext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SoundSynthPhotos_Comments",
                table: "SoundSynthPhotos");

            migrationBuilder.DropIndex(
                name: "IX_ProcessorPhotos_Comments",
                table: "ProcessorPhotos");

            migrationBuilder.DropIndex(
                name: "IX_MachinePhotos_Comments",
                table: "MachinePhotos");

            migrationBuilder.DropIndex(
                name: "IX_GpuPhotos_Comments",
                table: "GpuPhotos");

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "SoundSynthPhotos",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "ProcessorPhotos",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "MachinePhotos",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "GpuPhotos",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "SoundSynthPhotos",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "ProcessorPhotos",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "MachinePhotos",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "GpuPhotos",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthPhotos_Comments",
                table: "SoundSynthPhotos",
                column: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorPhotos_Comments",
                table: "ProcessorPhotos",
                column: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_MachinePhotos_Comments",
                table: "MachinePhotos",
                column: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPhotos_Comments",
                table: "GpuPhotos",
                column: "Comments");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class ChangeMachinePhotoSharpnessTypeToExifType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.AlterColumn<ushort>("Sharpness", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(string), oldNullable: true);

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.AlterColumn<string>("Sharpness", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldNullable: true);
    }
}
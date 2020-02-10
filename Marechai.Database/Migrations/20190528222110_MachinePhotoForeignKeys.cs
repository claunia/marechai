using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class MachinePhotoForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>("MachineId", "MachinePhotos", nullable: false, oldClrType: typeof(int),
                                              oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>("MachineId", "MachinePhotos", nullable: true, oldClrType: typeof(int));
        }
    }
}
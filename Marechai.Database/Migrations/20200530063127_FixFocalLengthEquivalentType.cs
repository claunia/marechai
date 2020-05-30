using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class FixFocalLengthEquivalentType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>("FocalLengthEquivalent", "OwnedMachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldType: "smallint unsigned",
                                                 oldNullable: true);

            migrationBuilder.AlterColumn<double>("FocalLengthEquivalent", "MachinePhotos", nullable: true,
                                                 oldClrType: typeof(ushort), oldType: "smallint unsigned",
                                                 oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ushort>("FocalLengthEquivalent", "OwnedMachinePhotos", "smallint unsigned",
                                                 nullable: true, oldClrType: typeof(double), oldNullable: true);

            migrationBuilder.AlterColumn<ushort>("FocalLengthEquivalent", "MachinePhotos", "smallint unsigned",
                                                 nullable: true, oldClrType: typeof(double), oldNullable: true);
        }
    }
}
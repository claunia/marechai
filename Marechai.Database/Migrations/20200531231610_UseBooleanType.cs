using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class UseBooleanType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>("Grayscale", "resolutions", nullable: false, oldClrType: typeof(sbyte),
                                               oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>("Ongoing", "PeopleByCompany", nullable: false, oldClrType: typeof(sbyte),
                                               oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>("Trade", "OwnedMachines", nullable: false, oldClrType: typeof(sbyte),
                                               oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>("SerialNumberVisible", "OwnedMachines", nullable: false,
                                               defaultValue: true, oldClrType: typeof(sbyte), oldType: "tinyint(1)",
                                               oldDefaultValue: (sbyte)1);

            migrationBuilder.AlterColumn<bool>("Manuals", "OwnedMachines", nullable: false, oldClrType: typeof(sbyte),
                                               oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>("Boxed", "OwnedMachines", nullable: false, oldClrType: typeof(sbyte),
                                               oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>("OsiApproved", "Licenses", nullable: false, oldClrType: typeof(sbyte),
                                               oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>("FsfApproved", "Licenses", nullable: false, oldClrType: typeof(sbyte),
                                               oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>("Enabled", "DocumentRoles", nullable: false, defaultValue: true,
                                               oldClrType: typeof(sbyte), oldType: "tinyint(1)",
                                               oldDefaultValue: (sbyte)1);

            migrationBuilder.AlterColumn<bool>("TwoFactorEnabled", "AspNetUsers", nullable: false,
                                               oldClrType: typeof(sbyte), oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>("PhoneNumberConfirmed", "AspNetUsers", nullable: false,
                                               oldClrType: typeof(sbyte), oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>("LockoutEnabled", "AspNetUsers", nullable: false,
                                               oldClrType: typeof(sbyte), oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>("EmailConfirmed", "AspNetUsers", nullable: false,
                                               oldClrType: typeof(sbyte), oldType: "tinyint(1)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<sbyte>("Grayscale", "resolutions", "tinyint(1)", nullable: false,
                                                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<sbyte>("Ongoing", "PeopleByCompany", "tinyint(1)", nullable: false,
                                                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<sbyte>("Trade", "OwnedMachines", "tinyint(1)", nullable: false,
                                                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<sbyte>("SerialNumberVisible", "OwnedMachines", "tinyint(1)", nullable: false,
                                                defaultValue: (sbyte)1, oldClrType: typeof(bool),
                                                oldDefaultValue: true);

            migrationBuilder.AlterColumn<sbyte>("Manuals", "OwnedMachines", "tinyint(1)", nullable: false,
                                                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<sbyte>("Boxed", "OwnedMachines", "tinyint(1)", nullable: false,
                                                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<sbyte>("OsiApproved", "Licenses", "tinyint(1)", nullable: false,
                                                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<sbyte>("FsfApproved", "Licenses", "tinyint(1)", nullable: false,
                                                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<sbyte>("Enabled", "DocumentRoles", "tinyint(1)", nullable: false,
                                                defaultValue: (sbyte)1, oldClrType: typeof(bool),
                                                oldDefaultValue: true);

            migrationBuilder.AlterColumn<sbyte>("TwoFactorEnabled", "AspNetUsers", "tinyint(1)", nullable: false,
                                                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<sbyte>("PhoneNumberConfirmed", "AspNetUsers", "tinyint(1)", nullable: false,
                                                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<sbyte>("LockoutEnabled", "AspNetUsers", "tinyint(1)", nullable: false,
                                                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<sbyte>("EmailConfirmed", "AspNetUsers", "tinyint(1)", nullable: false,
                                                oldClrType: typeof(bool));
        }
    }
}
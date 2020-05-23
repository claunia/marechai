using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class ExtendIdentityRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Discriminator", "AspNetUsers");

            migrationBuilder.AlterColumn<DateTimeOffset>("updated", "marechai_db", "datetime", nullable: true,
                                                         defaultValueSql: "CURRENT_TIMESTAMP",
                                                         oldClrType: typeof(DateTimeOffset), oldType: "timestamp",
                                                         oldNullable: true, oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>("Name", "AspNetUserTokens", nullable: false,
                                                 oldClrType: typeof(string), oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>("LoginProvider", "AspNetUserTokens", nullable: false,
                                                 oldClrType: typeof(string), oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>("ProviderKey", "AspNetUserLogins", nullable: false,
                                                 oldClrType: typeof(string), oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>("LoginProvider", "AspNetUserLogins", nullable: false,
                                                 oldClrType: typeof(string), oldMaxLength: 128);

            migrationBuilder.AddColumn<DateTime>("Created", "AspNetRoles", nullable: false,
                                                 defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0,
                                                                            DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>("Description", "AspNetRoles", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Created", "AspNetRoles");

            migrationBuilder.DropColumn("Description", "AspNetRoles");

            migrationBuilder.AlterColumn<DateTimeOffset>("updated", "marechai_db", "timestamp", nullable: true,
                                                         defaultValueSql: "CURRENT_TIMESTAMP",
                                                         oldClrType: typeof(DateTimeOffset), oldType: "datetime",
                                                         oldNullable: true, oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>("Name", "AspNetUserTokens", maxLength: 128, nullable: false,
                                                 oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>("LoginProvider", "AspNetUserTokens", maxLength: 128, nullable: false,
                                                 oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>("Discriminator", "AspNetUsers", nullable: false, defaultValue: "");

            migrationBuilder.AlterColumn<string>("ProviderKey", "AspNetUserLogins", maxLength: 128, nullable: false,
                                                 oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>("LoginProvider", "AspNetUserLogins", maxLength: 128, nullable: false,
                                                 oldClrType: typeof(string));
        }
    }
}
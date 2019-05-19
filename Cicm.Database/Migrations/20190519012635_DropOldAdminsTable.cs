using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cicm.Database.Migrations
{
    public partial class DropOldAdminsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("admins");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("admins",
                                         table => new
                                         {
                                             id = table.Column<int>("int(11)")
                                                       .Annotation("MySql:ValueGenerationStrategy",
                                                                   MySqlValueGenerationStrategy.IdentityColumn),
                                             password =
                                                 table.Column<string>("char(50)", nullable: false,
                                                                      defaultValueSql: "''"),
                                             user = table.Column<string>("char(50)", nullable: false,
                                                                         defaultValueSql: "''")
                                         }, constraints: table => { table.PrimaryKey("PK_admins", x => x.id); });

            migrationBuilder.CreateIndex("idx_admins_user", "admins", "user");
        }
    }
}
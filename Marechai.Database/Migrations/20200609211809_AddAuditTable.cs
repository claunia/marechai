using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddAuditTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Audit", table => new
            {
                Id = table.Column<long>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Type = table.Column<byte>(nullable: false), UserId = table.Column<string>(nullable: false),
                Table = table.Column<string>(nullable: true), Keys = table.Column<string>(nullable: true),
                OldValues = table.Column<string>(nullable: true), NewValues = table.Column<string>(nullable: true),
                AffectedColumns = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Audit", x => x.Id);

                table.ForeignKey("FK_Audit_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_Audit_Table", "Audit", "Table");

            migrationBuilder.CreateIndex("IX_Audit_Type", "Audit", "Type");

            migrationBuilder.CreateIndex("IX_Audit_UserId", "Audit", "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("Audit");
    }
}
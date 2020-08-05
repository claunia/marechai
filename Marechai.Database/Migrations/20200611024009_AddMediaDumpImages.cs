using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddMediaDumpImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("MediaDumpImages", table => new
            {
                Id = table.Column<ulong>(nullable: false).
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                MediaDumpId = table.Column<ulong>(nullable: false),
                Size        = table.Column<ulong>(nullable: false),
                Md5         = table.Column<byte[]>("binary(16)", nullable: true),
                Sha1        = table.Column<byte[]>("binary(20)", nullable: true),
                Sha256      = table.Column<byte[]>("binary(32)", nullable: true),
                Sha3        = table.Column<byte[]>("binary(64)", nullable: true),
                Spamsum     = table.Column<string>(nullable: true),
                AccoustId   = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_MediaDumpImages", x => x.Id);

                table.ForeignKey("FK_MediaDumpImages_MediaDumps_MediaDumpId", x => x.MediaDumpId, "MediaDumps", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_MediaDumpImages_AccoustId", "MediaDumpImages", "AccoustId");

            migrationBuilder.CreateIndex("IX_MediaDumpImages_Md5", "MediaDumpImages", "Md5");

            migrationBuilder.CreateIndex("IX_MediaDumpImages_MediaDumpId", "MediaDumpImages", "MediaDumpId",
                                         unique: true);

            migrationBuilder.CreateIndex("IX_MediaDumpImages_Sha1", "MediaDumpImages", "Sha1");

            migrationBuilder.CreateIndex("IX_MediaDumpImages_Sha256", "MediaDumpImages", "Sha256");

            migrationBuilder.CreateIndex("IX_MediaDumpImages_Sha3", "MediaDumpImages", "Sha3");

            migrationBuilder.CreateIndex("IX_MediaDumpImages_Size", "MediaDumpImages", "Size");

            migrationBuilder.CreateIndex("IX_MediaDumpImages_Spamsum", "MediaDumpImages", "Spamsum");
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable("MediaDumpImages");
    }
}
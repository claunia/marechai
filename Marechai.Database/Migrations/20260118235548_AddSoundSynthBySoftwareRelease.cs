using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoundSynthBySoftwareRelease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SoundSynthBySoftwareRelease",
                columns: table => new
                {
                    ReleaseId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    SoundSynthId = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoundSynthBySoftwareRelease", x => new { x.ReleaseId, x.SoundSynthId });
                    table.ForeignKey(
                        name: "FK_SoundSynthBySoftwareRelease_SoftwareReleases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "SoftwareReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoundSynthBySoftwareRelease_sound_synths_SoundSynthId",
                        column: x => x.SoundSynthId,
                        principalTable: "sound_synths",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthBySoftwareRelease_SoundSynthId",
                table: "SoundSynthBySoftwareRelease",
                column: "SoundSynthId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SoundSynthBySoftwareRelease");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoundSynthDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SoundSynthDescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SoundSynthId = table.Column<int>(type: "int(11)", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Text = table.Column<string>(type: "longtext", maxLength: 262144, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Html = table.Column<string>(type: "longtext", maxLength: 262144, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoundSynthDescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoundSynthDescriptions_sound_synths_SoundSynthId",
                        column: x => x.SoundSynthId,
                        principalTable: "sound_synths",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_sound_synth_descriptions_language",
                        column: x => x.LanguageCode,
                        principalTable: "ISO_639-3",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_sound_synth_descriptions_sound_synth_language",
                table: "SoundSynthDescriptions",
                columns: new[] { "SoundSynthId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthDescriptions_LanguageCode",
                table: "SoundSynthDescriptions",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_SoundSynthDescriptions_Text",
                table: "SoundSynthDescriptions",
                column: "Text")
                .Annotation("MySql:FullTextIndex", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SoundSynthDescriptions");
        }
    }
}

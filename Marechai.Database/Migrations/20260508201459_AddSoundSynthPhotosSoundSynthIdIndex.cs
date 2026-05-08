using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoundSynthPhotosSoundSynthIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_SoundSynthPhotos_SoundSynthId",
                table: "SoundSynthPhotos",
                newName: "idx_sound_synth_photos_sound_synth");

            migrationBuilder.CreateIndex(
                name: "idx_sound_synth_photos_sound_synth_created",
                table: "SoundSynthPhotos",
                columns: new[] { "SoundSynthId", "CreatedOn", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_sound_synth_photos_sound_synth_created",
                table: "SoundSynthPhotos");

            migrationBuilder.RenameIndex(
                name: "idx_sound_synth_photos_sound_synth",
                table: "SoundSynthPhotos",
                newName: "IX_SoundSynthPhotos_SoundSynthId");
        }
    }
}

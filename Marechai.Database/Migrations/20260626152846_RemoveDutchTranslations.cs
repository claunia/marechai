using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDutchTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dutch ("nld") is no longer a supported UI translation language. Delete any
            // auto-generated/suggested Dutch content from the per-entity translation tables.
            // LanguageBySoftwareRelease is intentionally excluded: it records the language a
            // release was actually published in, not a UI translation, so a release that is
            // genuinely Dutch-language must keep its row.
            string[] tables =
            [
                "BookSynopses", "CompanyDescriptions", "DocumentSynopses", "GpuDescriptions",
                "MachineDescriptions", "MagazineSynopses", "PeopleBySoftwareRoleTranslations",
                "PersonDescriptions", "ProcessorDescriptions",
                "SoftwareAlternativeTitleCommentTranslations", "SoftwareAttributeStringTranslations",
                "SoftwareCoverCaptionTranslations", "SoftwareDescriptions", "SoftwareGenreTranslations",
                "SoftwarePromoArtGroupTranslations", "SoftwareScreenshotCaptionTranslations",
                "SoftwareScreenshotGroupTranslations", "SoundSynthDescriptions"
            ];

            foreach(string table in tables)
                migrationBuilder.Sql($"DELETE FROM `{table}` WHERE `LanguageCode` = 'nld';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Deleted translation content cannot be reconstructed.
        }
    }
}

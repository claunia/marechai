using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLatinTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Description tables
            migrationBuilder.Sql("DELETE FROM `CompanyDescriptions` WHERE `LanguageCode` = 'lat';");
            migrationBuilder.Sql("DELETE FROM `GpuDescriptions` WHERE `LanguageCode` = 'lat';");
            migrationBuilder.Sql("DELETE FROM `MachineDescriptions` WHERE `LanguageCode` = 'lat';");
            migrationBuilder.Sql("DELETE FROM `PersonDescriptions` WHERE `LanguageCode` = 'lat';");
            migrationBuilder.Sql("DELETE FROM `ProcessorDescriptions` WHERE `LanguageCode` = 'lat';");
            migrationBuilder.Sql("DELETE FROM `SoftwareDescriptions` WHERE `LanguageCode` = 'lat';");
            migrationBuilder.Sql("DELETE FROM `SoundSynthDescriptions` WHERE `LanguageCode` = 'lat';");

            // Synopsis tables
            migrationBuilder.Sql("DELETE FROM `BookSynopses` WHERE `LanguageCode` = 'lat';");
            migrationBuilder.Sql("DELETE FROM `DocumentSynopses` WHERE `LanguageCode` = 'lat';");
            migrationBuilder.Sql("DELETE FROM `MagazineSynopses` WHERE `LanguageCode` = 'lat';");

            // Translation tables
            migrationBuilder.Sql("DELETE FROM `SoftwareGenreTranslations` WHERE `LanguageCode` = 'lat';");
            migrationBuilder.Sql("DELETE FROM `SoftwareAttributeStringTranslations` WHERE `LanguageCode` = 'lat';");
            migrationBuilder.Sql("DELETE FROM `SoftwareCoverCaptionTranslations` WHERE `LanguageCode` = 'lat';");
            migrationBuilder.Sql("DELETE FROM `SoftwarePromoArtGroupTranslations` WHERE `LanguageCode` = 'lat';");
            migrationBuilder.Sql("DELETE FROM `SoftwareScreenshotCaptionTranslations` WHERE `LanguageCode` = 'lat';");
            migrationBuilder.Sql("DELETE FROM `SoftwareScreenshotGroupTranslations` WHERE `LanguageCode` = 'lat';");
            migrationBuilder.Sql("DELETE FROM `PeopleBySoftwareRoleTranslations` WHERE `LanguageCode` = 'lat';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Latin translations were AI-generated and can be re-created if Latin is ever re-added.
        }
    }
}

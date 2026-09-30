using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class BackfillSoftwareCoverOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Covers created by the admin batch upload or by approving a cover suggestion were
            // only attributed to a release, leaving their owner empty, so the per-software and
            // per-compilation cover listings never returned them. Fill the owner in from the release.
            migrationBuilder.Sql("""
                                 UPDATE SoftwareCovers c
                                   JOIN SoftwareReleases r ON c.SoftwareReleaseId = r.Id
                                   LEFT JOIN SoftwareVersions v ON r.SoftwareVersionId = v.Id
                                    SET c.SoftwareId = COALESCE(r.SoftwareId, v.SoftwareId)
                                  WHERE c.SoftwareId IS NULL
                                    AND c.SoftwareCompilationId IS NULL
                                    AND COALESCE(r.SoftwareId, v.SoftwareId) IS NOT NULL;
                                 """);

            migrationBuilder.Sql("""
                                 UPDATE SoftwareCovers c
                                   JOIN SoftwareReleases r ON c.SoftwareReleaseId = r.Id
                                    SET c.SoftwareCompilationId = r.SoftwareCompilationId
                                  WHERE c.SoftwareId IS NULL
                                    AND c.SoftwareCompilationId IS NULL
                                    AND r.SoftwareCompilationId IS NOT NULL;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data-only backfill: the filled-in owners are correct and indistinguishable from
            // rows that already had them, so there is nothing to revert.
        }
    }
}

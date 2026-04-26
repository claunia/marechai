using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixSoftwareVersionNewsLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // News entries for software versions incorrectly stored the version's ID
            // in AddedId instead of the parent software's ID. Fix by joining against
            // software_versions to get the correct SoftwareId.
            // NewsType 20 = NewSoftwareVersionInDb, 21 = UpdatedSoftwareVersionInDb
            migrationBuilder.Sql("""
                UPDATE news
                INNER JOIN SoftwareVersions ON news.added_id = SoftwareVersions.Id
                SET news.added_id = SoftwareVersions.SoftwareId
                WHERE news.type IN (20, 21)
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not reversible — we no longer know which version ID was originally stored.
        }
    }
}

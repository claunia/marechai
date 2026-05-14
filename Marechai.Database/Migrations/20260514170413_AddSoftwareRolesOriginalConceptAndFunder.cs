using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations;

/// <summary>
///     Seeds the <c>SoftwareRoles</c> table with two new entries used by the
///     MobyGames importer to map the company-role labels
///     <c>"Original Concept by"</c> → <c>ocp</c> and <c>"Funded by"</c> →
///     <c>fnd</c>. No schema change.
///
///     Inserts are idempotent (gated by <c>WHERE NOT EXISTS</c> on the primary
///     key) so re-running the migration on a partially-seeded DB does not
///     fail with a duplicate-key error.
/// </summary>
public partial class AddSoftwareRolesOriginalConceptAndFunder : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
INSERT INTO `SoftwareRoles` (`Id`, `Name`, `Enabled`) SELECT 'ocp', 'Original Concept', 1 WHERE NOT EXISTS (SELECT 1 FROM `SoftwareRoles` WHERE `Id`='ocp');
INSERT INTO `SoftwareRoles` (`Id`, `Name`, `Enabled`) SELECT 'fnd', 'Funder', 1 WHERE NOT EXISTS (SELECT 1 FROM `SoftwareRoles` WHERE `Id`='fnd');
");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
DELETE FROM `SoftwareRoles` WHERE `Id` IN ('ocp', 'fnd');
");
    }
}

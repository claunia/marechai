using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations;

/// <summary>
///     Seeds one additional <c>SoftwareGenreType.Category</c> (= 4) entry:
///     "Disc Mastering &amp; Burning".  Idempotent via <c>WHERE NOT EXISTS</c>.
/// </summary>
public partial class AddDiscMasteringBurningCategory : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Disc Mastering & Burning', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Disc Mastering & Burning' AND `Type`=4);
");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM `SoftwareGenres` WHERE `Type` = 4 AND `Name` = 'Disc Mastering & Burning';");
    }
}

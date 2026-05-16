using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <summary>
    /// Create <c>NormalizeForMatch</c> — the prefix-comparison helper used by
    /// the /admin/software/orphan-addons page. Equivalent to
    /// <c>NormalizeForDuplicate</c> but additionally strips every
    /// non-alphanumeric / non-whitespace character (including colons and
    /// dashes), so <c>EyePet: Hospital</c> normalizes to <c>eyepet hospital</c>
    /// and can be prefix-matched against the leading token <c>eyepet</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Pinned to <c>utf8mb4 / utf8mb4_general_ci</c> on the return type and
    /// local variable for the same reason
    /// <see cref="FixNormalizeForDuplicateCollation"/> does: MariaDB 11.5+
    /// servers default the session collation to a UCA1400 variant which
    /// otherwise produces <c>Illegal mix of collations</c> errors when the
    /// function value is compared against parameter-bound prefixes inside a
    /// <c>LIKE</c> predicate.
    /// </para>
    /// </remarks>
    public partial class AddNormalizeForMatchFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS NormalizeForMatch");

            migrationBuilder.Sql("""
                CREATE FUNCTION NormalizeForMatch(val VARCHAR(255))
                RETURNS VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci
                DETERMINISTIC
                NO SQL
                BEGIN
                    DECLARE result VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

                    IF val IS NULL THEN
                        RETURN NULL;
                    END IF;

                    -- Strip every parenthesised group (e.g. "(Limited Edition)") and every
                    -- bracketed group (e.g. "[NTSC]"). Mirrors NormalizeForDuplicate so the
                    -- two functions agree on which trailing-tag content is irrelevant.
                    SET result = val;
                    SET result = REGEXP_REPLACE(result, '[[:space:]]*\\([^)]*\\)', '');
                    SET result = REGEXP_REPLACE(result, '[[:space:]]*\\[[^]]*]', '');

                    -- Replace every non-alphanumeric, non-whitespace character with a space.
                    -- This is the difference vs NormalizeForDuplicate: we want colons,
                    -- dashes, slashes, etc. treated as word separators so prefix matching
                    -- against the first N tokens is reliable.
                    SET result = REGEXP_REPLACE(result, '[^[:alnum:][:space:]]+', ' ');

                    -- Collapse runs of whitespace into single spaces.
                    SET result = REGEXP_REPLACE(result, '[[:space:]]+', ' ');

                    -- Trim and lowercase so the prefix LIKE comparison is case-insensitive.
                    RETURN LOWER(TRIM(result));
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS NormalizeForMatch");
        }
    }
}

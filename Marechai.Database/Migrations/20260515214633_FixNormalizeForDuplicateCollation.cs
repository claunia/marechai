using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <summary>
    /// Pin <c>NormalizeForDuplicate</c>'s return-type collation to
    /// <c>utf8mb4_general_ci</c> explicitly, defending against MariaDB 11.5+
    /// servers whose <c>character_set_collations</c> compiled-in default maps
    /// utf8mb4 → <c>utf8mb4_uca1400_ai_ci</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// MariaDB 11.5 added a global <c>character_set_collations</c> variable;
    /// MariaDB 12 ships with it set to <c>utf8mb4=utf8mb4_uca1400_ai_ci, …</c>
    /// by default. The previous migration created the function without an
    /// explicit <c>COLLATE</c> on the return type, so on any environment
    /// running migrations before <c>MariaDb12CollationInterceptor</c> takes
    /// effect (or without the matching <c>my.cnf</c> override in place) the
    /// function would return <c>uca1400_ai_ci</c> and fail with
    /// <c>Illegal mix of collations</c> when EF Core composed a
    /// <c>WHERE NormalizeForDuplicate(s.Name) IN (@p1,@p2)</c> against parameter
    /// values bound under the session's other collation.
    /// </para>
    /// <para>
    /// This migration does NOT touch any existing column collations — the
    /// production server may have different per-column collations than dev,
    /// and ALTER'ing them blindly could fail or destabilise existing data.
    /// The fix is purely at the function-definition layer.
    /// </para>
    /// </remarks>
    public partial class FixNormalizeForDuplicateCollation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS NormalizeForDuplicate");

            // The explicit `CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci`
            // on the RETURNS clause and on the local `result` variable is the
            // defensive part — it pins the function output to general_ci
            // regardless of whatever the session default happens to be when
            // the function is invoked, eliminating the collation-mismatch
            // class of failures permanently.
            migrationBuilder.Sql("""
                CREATE FUNCTION NormalizeForDuplicate(val VARCHAR(255))
                RETURNS VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci
                DETERMINISTIC
                NO SQL
                BEGIN
                    DECLARE result VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

                    IF val IS NULL THEN
                        RETURN NULL;
                    END IF;

                    -- Strip every parenthesised group (e.g. "(Limited Edition)") and every
                    -- bracketed group (e.g. "[NTSC]"). Repeat until no more matches so that
                    -- chained suffixes like "Game (Limited Edition) [NTSC]" collapse fully.
                    -- POSIX classes used instead of \s / \[ etc. so backslash escaping does
                    -- not collide between the C# raw-string literal and MariaDB string parsing.
                    SET result = val;
                    SET result = REGEXP_REPLACE(result, '[[:space:]]*\\([^)]*\\)', '');
                    SET result = REGEXP_REPLACE(result, '[[:space:]]*\\[[^]]*]', '');

                    -- Collapse runs of whitespace into single spaces.
                    SET result = REGEXP_REPLACE(result, '[[:space:]]+', ' ');

                    -- Trim and lowercase so the grouping key is case-insensitive.
                    RETURN LOWER(TRIM(result));
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the body shipped by 20260515211053_AddNormalizeForDuplicateFunction
            // (no explicit collation on the return type).
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS NormalizeForDuplicate");

            migrationBuilder.Sql("""
                CREATE FUNCTION NormalizeForDuplicate(val VARCHAR(255))
                RETURNS VARCHAR(255)
                DETERMINISTIC
                NO SQL
                BEGIN
                    DECLARE result VARCHAR(255);

                    IF val IS NULL THEN
                        RETURN NULL;
                    END IF;

                    SET result = val;
                    SET result = REGEXP_REPLACE(result, '[[:space:]]*\\([^)]*\\)', '');
                    SET result = REGEXP_REPLACE(result, '[[:space:]]*\\[[^]]*]', '');
                    SET result = REGEXP_REPLACE(result, '[[:space:]]+', ' ');

                    RETURN LOWER(TRIM(result));
                END
                """);
        }
    }
}

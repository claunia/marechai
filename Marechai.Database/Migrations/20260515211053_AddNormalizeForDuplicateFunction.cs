using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizeForDuplicateFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS NormalizeForDuplicate");
        }
    }
}

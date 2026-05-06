using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNaturalSortKeyFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE FUNCTION NaturalSortKey(val VARCHAR(255))
                RETURNS VARCHAR(1024)
                DETERMINISTIC
                NO SQL
                BEGIN
                    DECLARE result VARCHAR(1024) DEFAULT '';
                    DECLARE i INT DEFAULT 1;
                    DECLARE len INT;
                    DECLARE ch CHAR(1);
                    DECLARE numStr VARCHAR(64);

                    IF val IS NULL THEN
                        RETURN NULL;
                    END IF;

                    SET len = CHAR_LENGTH(val);

                    WHILE i <= len DO
                        SET ch = SUBSTRING(val, i, 1);

                        IF ch BETWEEN '0' AND '9' THEN
                            SET numStr = '';

                            WHILE i <= len AND SUBSTRING(val, i, 1) BETWEEN '0' AND '9' DO
                                SET numStr = CONCAT(numStr, SUBSTRING(val, i, 1));
                                SET i = i + 1;
                            END WHILE;

                            SET result = CONCAT(result, LPAD(numStr, 20, '0'));
                        ELSE
                            SET result = CONCAT(result, UPPER(ch));
                            SET i = i + 1;
                        END IF;
                    END WHILE;

                    RETURN result;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS NaturalSortKey");
        }
    }
}

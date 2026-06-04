using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <summary>
    ///     Corrects the previous <c>MergeNbspSoftwareGenreDuplicates</c> migration
    ///     which was a silent no-op: <c>CHAR(160)</c> in MariaDB returns a single
    ///     binary byte <c>0xA0</c>, NOT the UTF-8 encoding <c>0xC2A0</c> that
    ///     <c>utf8mb4</c> columns actually store for U+00A0 (NBSP). Every
    ///     <c>REPLACE()</c> and <c>INSTR()</c> call matched zero rows.
    ///     <c>CHAR(160 USING utf8mb4)</c> also fails — it returns NULL because
    ///     the single byte <c>0xA0</c> is not a valid utf8mb4 sequence.
    ///
    ///     This migration is identical in logic but uses the charset introducer
    ///     literal <c>_utf8mb4 X'C2A0'</c> throughout, which directly specifies
    ///     the two-byte UTF-8 encoding of U+00A0.
    /// </summary>
    public partial class FixNbspCharsetInGenreMerge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- Build (loser_id -> winner_id) map for every (Type, normalized_name) group
-- with > 1 row.  Uses _utf8mb4 X'C2A0' — the charset introducer literal for
-- U+00A0 (NBSP) — so the two-byte sequence actually matches the column data.
-- Previous attempts with CHAR(160) (returns binary 0xA0, doesn't match utf8mb4
-- C2A0) and CHAR(160 USING utf8mb4) (returns NULL) were both no-ops.
CREATE TEMPORARY TABLE _genre_merge (
    loser_id  INT NOT NULL PRIMARY KEY,
    winner_id INT NOT NULL,
    KEY (winner_id)
);

INSERT INTO _genre_merge (loser_id, winner_id)
SELECT g.`Id` AS loser_id, w.winner_id
FROM `SoftwareGenres` g
JOIN (
    SELECT `Type`,
           TRIM(REPLACE(`Name`, _utf8mb4 X'C2A0', ' '))                        AS norm,
           COALESCE(
               MIN(CASE WHEN INSTR(`Name`, _utf8mb4 X'C2A0') = 0
                         AND `Name` = TRIM(`Name`)                THEN `Id` END),
               MIN(`Id`)
           )                                                                   AS winner_id
    FROM `SoftwareGenres`
    GROUP BY `Type`, TRIM(REPLACE(`Name`, _utf8mb4 X'C2A0', ' '))
    HAVING COUNT(*) > 1
) w
  ON w.`Type` = g.`Type`
 AND w.norm   = TRIM(REPLACE(g.`Name`, _utf8mb4 X'C2A0', ' '))
WHERE g.`Id` <> w.winner_id;

-- 1. GenresBySoftware: composite PK (SoftwareId, GenreId).
--    Drop loser-side rows whose SoftwareId already has a winner-side row,
--    then repoint the rest.
DELETE gs
FROM `GenresBySoftware` gs
JOIN `_genre_merge`     m  ON m.loser_id  = gs.`GenreId`
JOIN `GenresBySoftware` gw ON gw.`SoftwareId` = gs.`SoftwareId`
                          AND gw.`GenreId`    = m.winner_id;

UPDATE `GenresBySoftware` gs
JOIN `_genre_merge`       m ON m.loser_id = gs.`GenreId`
SET gs.`GenreId` = m.winner_id;

-- 2. SoftwareGenreTranslations: unique (GenreId, LanguageCode).
--    Winner-side translations win on conflict (older / hand-curated).
DELETE t
FROM `SoftwareGenreTranslations` t
JOIN `_genre_merge`              m  ON m.loser_id     = t.`GenreId`
JOIN `SoftwareGenreTranslations` tw ON tw.`GenreId`   = m.winner_id
                                   AND tw.`LanguageCode` = t.`LanguageCode`;

UPDATE `SoftwareGenreTranslations` t
JOIN `_genre_merge`                m ON m.loser_id = t.`GenreId`
SET t.`GenreId` = m.winner_id;

-- 3. RankingDefinitions: polymorphic id, unique (Dimension, DimensionId).
--    Dimension = 1 is Genre.  RankingEntries cascades on RankingDefinitionId
--    and is rebuilt by MarechaiRankingsWorker on its next 24 h tick.
DELETE rd
FROM `RankingDefinitions` rd
JOIN `_genre_merge`       m ON m.loser_id = rd.`DimensionId`
WHERE rd.`Dimension` = 1
  AND EXISTS (
      SELECT 1
      FROM `RankingDefinitions` rw
      WHERE rw.`Dimension`   = 1
        AND rw.`DimensionId` = m.winner_id
  );

UPDATE `RankingDefinitions` rd
JOIN `_genre_merge`         m ON m.loser_id = rd.`DimensionId`
SET rd.`DimensionId` = m.winner_id
WHERE rd.`Dimension` = 1;

-- 4. Delete loser SoftwareGenres rows (now fully orphaned).
DELETE g
FROM `SoftwareGenres` g
JOIN `_genre_merge`   m ON m.loser_id = g.`Id`;

-- 5. Normalise survivors that still carry NBSP and/or leading/trailing
--    whitespace.  Safe — step 4 removed every row that could collide on the
--    unique (Name, Type) index after normalisation.
UPDATE `SoftwareGenres`
SET `Name`      = TRIM(REPLACE(`Name`, _utf8mb4 X'C2A0', ' ')),
    `UpdatedOn` = UTC_TIMESTAMP(6)
WHERE INSTR(`Name`, _utf8mb4 X'C2A0') > 0
   OR `Name` <> TRIM(`Name`);

DROP TEMPORARY TABLE `_genre_merge`;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Destructive merge: loser rows and their child references have been
            // deleted or overwritten.  Down() cannot resurrect them.
        }
    }
}

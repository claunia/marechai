using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <summary>
    ///     Merges <c>SoftwareGenres</c> rows that differ only by U+00A0 (non-breaking
    ///     space) and/or surrounding whitespace. Produced by a regression in the
    ///     MobyGames importer (<c>Marechai.MobyGames/Services/ImportService.cs</c>)
    ///     that looked up genres via <c>g.Name == genre.Name</c> without normalising
    ///     <c>&amp;nbsp;</c> first, so cells like <c>"Action\u00A0Adventure"</c>
    ///     bypassed the existing <c>"Action Adventure"</c> row and inserted a
    ///     duplicate. Every sister matcher in that project already does
    ///     <c>.Replace("\u00a0", " ").Trim()</c>; the genre import path was the
    ///     lone outlier (now fixed at the same commit).
    ///
    ///     For each <c>(Type, TRIM(REPLACE(Name, CHAR(160), ' ')))</c> group with
    ///     more than one row, the winner is the lowest-<c>Id</c> row that already
    ///     has a clean canonical name (no NBSP, no leading/trailing whitespace);
    ///     ties / all-dirty groups fall back to the lowest <c>Id</c> in the group.
    ///     All references in <c>GenresBySoftware</c>, <c>SoftwareGenreTranslations</c>
    ///     and <c>RankingDefinitions</c> (<c>Dimension = 1</c>, Genre) are repointed
    ///     onto the winner — dedupe-before-repoint to avoid composite-PK / unique
    ///     index collisions. <c>RankingEntries</c> cascades on
    ///     <c>RankingDefinitionId</c> and is rebuilt every 24 h by
    ///     <c>MarechaiRankingsWorker</c>, so any cached <c>GenreName</c> self-heals.
    ///     Loser <c>SoftwareGenres</c> rows are then deleted, and any survivors
    ///     still carrying NBSP / leading-trailing whitespace get their <c>Name</c>
    ///     normalised in place — safe at that point because all (Name, Type)
    ///     collisions have already been removed.
    ///
    ///     <c>Down()</c> is a no-op: merging is destructive and cannot be reversed.
    /// </summary>
    public partial class MergeNbspSoftwareGenreDuplicates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- Build (loser_id -> winner_id) map for every (Type, normalized_name) group
-- with > 1 row.
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
           TRIM(REPLACE(`Name`, CHAR(160), ' '))                                AS norm,
           COALESCE(
               MIN(CASE WHEN INSTR(`Name`, CHAR(160)) = 0
                         AND `Name` = TRIM(`Name`)                THEN `Id` END),
               MIN(`Id`)
           )                                                                   AS winner_id
    FROM `SoftwareGenres`
    GROUP BY `Type`, TRIM(REPLACE(`Name`, CHAR(160), ' '))
    HAVING COUNT(*) > 1
) w
  ON w.`Type` = g.`Type`
 AND w.norm   = TRIM(REPLACE(g.`Name`, CHAR(160), ' '))
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
--    Dimension = 1 is Genre. RankingEntries cascades on RankingDefinitionId
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
--    whitespace. Safe — step 4 removed every row that could collide on the
--    unique (Name, Type) index after normalisation.
UPDATE `SoftwareGenres`
SET `Name`      = TRIM(REPLACE(`Name`, CHAR(160), ' ')),
    `UpdatedOn` = UTC_TIMESTAMP(6)
WHERE INSTR(`Name`, CHAR(160)) > 0
   OR `Name` <> TRIM(`Name`);

DROP TEMPORARY TABLE `_genre_merge`;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Destructive merge: loser rows and their child references have been
            // deleted or overwritten. Down() cannot resurrect them.
        }
    }
}

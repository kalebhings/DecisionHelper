using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecisionHelper.Migrations
{
    /// <inheritdoc />
    public partial class AddGuildIsolation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_NormalizedName_Kind",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_People_DiscordUserId",
                table: "People");

            migrationBuilder.DropIndex(
                name: "IX_Movies_NormalizedTitle_ReleaseYear",
                table: "Movies");

            migrationBuilder.AddColumn<string>(
                name: "GuildId",
                table: "Tags",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GuildId",
                table: "People",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GuildId",
                table: "Movies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                INSERT INTO MovieWatchStatuses
                    (MovieId, PersonId, HasSeen, WatchedAtUtc)
                SELECT
                    (SELECT MIN(keeper.Id)
                     FROM Movies AS keeper
                     WHERE keeper.NormalizedTitle = duplicate.NormalizedTitle
                       AND keeper.ReleaseYear IS NULL),
                    status.PersonId,
                    status.HasSeen,
                    status.WatchedAtUtc
                FROM MovieWatchStatuses AS status
                INNER JOIN Movies AS duplicate ON duplicate.Id = status.MovieId
                WHERE duplicate.ReleaseYear IS NULL
                  AND duplicate.Id != (
                      SELECT MIN(keeper.Id)
                      FROM Movies AS keeper
                      WHERE keeper.NormalizedTitle = duplicate.NormalizedTitle
                        AND keeper.ReleaseYear IS NULL)
                ON CONFLICT(MovieId, PersonId) DO UPDATE SET
                    HasSeen = MAX(MovieWatchStatuses.HasSeen, excluded.HasSeen),
                    WatchedAtUtc = CASE
                        WHEN excluded.HasSeen = 1
                        THEN COALESCE(
                            excluded.WatchedAtUtc,
                            MovieWatchStatuses.WatchedAtUtc)
                        ELSE MovieWatchStatuses.WatchedAtUtc
                    END;

                INSERT OR IGNORE INTO MovieTags (MovieId, TagId)
                SELECT
                    (SELECT MIN(keeper.Id)
                     FROM Movies AS keeper
                     WHERE keeper.NormalizedTitle = duplicate.NormalizedTitle
                       AND keeper.ReleaseYear IS NULL),
                    movieTag.TagId
                FROM MovieTags AS movieTag
                INNER JOIN Movies AS duplicate ON duplicate.Id = movieTag.MovieId
                WHERE duplicate.ReleaseYear IS NULL
                  AND duplicate.Id != (
                      SELECT MIN(keeper.Id)
                      FROM Movies AS keeper
                      WHERE keeper.NormalizedTitle = duplicate.NormalizedTitle
                        AND keeper.ReleaseYear IS NULL);

                DELETE FROM MovieWatchStatuses
                WHERE MovieId IN (
                    SELECT duplicate.Id
                    FROM Movies AS duplicate
                    WHERE duplicate.ReleaseYear IS NULL
                      AND duplicate.Id != (
                          SELECT MIN(keeper.Id)
                          FROM Movies AS keeper
                          WHERE keeper.NormalizedTitle = duplicate.NormalizedTitle
                            AND keeper.ReleaseYear IS NULL));

                DELETE FROM MovieTags
                WHERE MovieId IN (
                    SELECT duplicate.Id
                    FROM Movies AS duplicate
                    WHERE duplicate.ReleaseYear IS NULL
                      AND duplicate.Id != (
                          SELECT MIN(keeper.Id)
                          FROM Movies AS keeper
                          WHERE keeper.NormalizedTitle = duplicate.NormalizedTitle
                            AND keeper.ReleaseYear IS NULL));

                DELETE FROM Movies
                WHERE ReleaseYear IS NULL
                  AND Id != (
                      SELECT MIN(keeper.Id)
                      FROM Movies AS keeper
                      WHERE keeper.NormalizedTitle = Movies.NormalizedTitle
                        AND keeper.ReleaseYear IS NULL);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_GuildId_NormalizedName_Kind",
                table: "Tags",
                columns: new[] { "GuildId", "NormalizedName", "Kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_People_GuildId_DiscordUserId",
                table: "People",
                columns: new[] { "GuildId", "DiscordUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movies_GuildId_NormalizedTitle_NoReleaseYear",
                table: "Movies",
                columns: new[] { "GuildId", "NormalizedTitle" },
                unique: true,
                filter: "ReleaseYear IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_GuildId_NormalizedTitle_ReleaseYear",
                table: "Movies",
                columns: new[] { "GuildId", "NormalizedTitle", "ReleaseYear" },
                unique: true,
                filter: "ReleaseYear IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException(
                "Guild data may diverge after this migration and cannot be safely merged during downgrade.");
        }
    }
}

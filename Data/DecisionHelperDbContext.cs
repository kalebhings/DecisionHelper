using DecisionHelper.Models;
using Microsoft.EntityFrameworkCore;

namespace DecisionHelper.Data;

public class DecisionHelperDbContext : DbContext
{
    public DecisionHelperDbContext(
        DbContextOptions<DecisionHelperDbContext> options)
        : base(options)
    {
    }

    public DbSet<Person> People => Set<Person>();

    public DbSet<Movie> Movies => Set<Movie>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<MovieTag> MovieTags => Set<MovieTag>();

    public DbSet<MovieWatchStatus> MovieWatchStatuses =>
        Set<MovieWatchStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>()
            .HasIndex(person => new
            {
                person.GuildId,
                person.DiscordUserId
            })
            .IsUnique();

        modelBuilder.Entity<Movie>()
            .HasIndex(movie => new
            {
                movie.GuildId,
                movie.NormalizedTitle,
                movie.ReleaseYear
            })
            .HasDatabaseName(
                "IX_Movies_GuildId_NormalizedTitle_ReleaseYear")
            .HasFilter("ReleaseYear IS NOT NULL")
            .IsUnique();

        modelBuilder.Entity<Movie>()
            .HasIndex(movie => new
            {
                movie.GuildId,
                movie.NormalizedTitle
            })
            .HasDatabaseName(
                "IX_Movies_GuildId_NormalizedTitle_NoReleaseYear")
            .HasFilter("ReleaseYear IS NULL")
            .IsUnique();

        modelBuilder.Entity<Tag>()
            .HasIndex(tag => new
            {
                tag.GuildId,
                tag.NormalizedName,
                tag.Kind
            })
            .IsUnique();

        modelBuilder.Entity<MovieTag>()
            .HasKey(movieTag => new
            {
                movieTag.MovieId,
                movieTag.TagId
            });

        modelBuilder.Entity<MovieWatchStatus>()
            .HasKey(status => new
            {
                status.MovieId,
                status.PersonId
            });

        modelBuilder.Entity<Movie>()
            .HasOne(movie => movie.AddedBy)
            .WithMany(person => person.MoviesAdded)
            .HasForeignKey(movie => movie.AddedByPersonId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovieTag>()
            .HasOne(movieTag => movieTag.Movie)
            .WithMany(movie => movie.MovieTags)
            .HasForeignKey(movieTag => movieTag.MovieId);

        modelBuilder.Entity<MovieTag>()
            .HasOne(movieTag => movieTag.Tag)
            .WithMany(tag => tag.MovieTags)
            .HasForeignKey(movieTag => movieTag.TagId);

        modelBuilder.Entity<MovieWatchStatus>()
            .HasOne(status => status.Movie)
            .WithMany(movie => movie.WatchStatuses)
            .HasForeignKey(status => status.MovieId);

        modelBuilder.Entity<MovieWatchStatus>()
            .HasOne(status => status.Person)
            .WithMany(person => person.MovieWatchStatuses)
            .HasForeignKey(status => status.PersonId);
    }
}

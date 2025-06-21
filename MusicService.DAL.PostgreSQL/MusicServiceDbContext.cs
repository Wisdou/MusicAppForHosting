using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MusicService.DAL.PostgreSQL.Extensions;
using MusicService.Domain.Entities;

namespace MusicService.DAL.PostgreSQL;

public class MusicServiceDbContext(DbContextOptions options) : DbContext(options)
{
    internal const string CaseInsensitiveCollation = "case_insensitive_collation";
    
    public DbSet<User> Users { get; set; }
    
    public DbSet<Track> Tracks { get; set; }
    
    public DbSet<Playlist> Playlists { get; set; }
    
    public DbSet<PlaylistTrack> PlaylistTracks { get; set; }
    
    public DbSet<FavoriteTrack> FavoriteTracks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasCollation(CaseInsensitiveCollation, locale: "und-u-ks-level1", provider: "icu", deterministic: false);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.Seed();
    }
}
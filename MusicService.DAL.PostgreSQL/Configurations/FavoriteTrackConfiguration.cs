using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicService.Domain.Entities;

namespace MusicService.DAL.PostgreSQL.Configurations;

public class FavoriteTrackConfiguration : IEntityTypeConfiguration<FavoriteTrack>
{
    public void Configure(EntityTypeBuilder<FavoriteTrack> builder)
    {
        builder.HasKey(e => new { e.TrackId, e.UserId });
        
        builder
            .HasOne(e => e.Track)
            .WithMany()
            .HasForeignKey(e => e.TrackId);
        
        builder
            .HasOne(e => e.User)
            .WithMany(e => e.Favorites)
            .HasForeignKey(e => e.UserId);
    }
}
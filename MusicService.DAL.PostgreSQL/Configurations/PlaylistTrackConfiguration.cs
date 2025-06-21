using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicService.Domain.Entities;

namespace MusicService.DAL.PostgreSQL.Configurations;

public class PlaylistTrackConfiguration : IEntityTypeConfiguration<PlaylistTrack>
{
    public void Configure(EntityTypeBuilder<PlaylistTrack> builder)
    {
        builder.HasKey(e => new { e.PlaylistId, e.TrackId });
        
        builder
            .HasOne(e => e.Playlist)
            .WithMany(e => e.Tracks)
            .HasForeignKey(e => e.PlaylistId);
        
        builder
            .HasOne(e => e.Track)
            .WithMany()
            .HasForeignKey(e => e.TrackId);
    }
}
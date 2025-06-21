using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicService.Domain.Constants;
using MusicService.Domain.Entities;

namespace MusicService.DAL.PostgreSQL.Configurations;

public class PlaylistConfiguration : IEntityTypeConfiguration<Playlist>
{
    public void Configure(EntityTypeBuilder<Playlist> builder)
    {
        builder.HasKey(e => e.Id);

        builder
            .Property(e => e.Title)
            .UseCollation(MusicServiceDbContext.CaseInsensitiveCollation)
            .HasMaxLength(Constraints.PlaylistTitleMaxLength);
        
        builder
            .HasIndex(e => new { e.UserId, e.Title })
            .IsUnique();
        
        builder
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);
    }
}
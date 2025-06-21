using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicService.Domain.Constants;
using MusicService.Domain.Entities;

namespace MusicService.DAL.PostgreSQL.Configurations;

public class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder
            .Property(e => e.Title)
            .UseCollation(MusicServiceDbContext.CaseInsensitiveCollation)
            .HasMaxLength(Constraints.TrackTitleMaxLength);

        builder
            .Property(e => e.Genre)
            .HasConversion(e => e.ToString(), e => Enum.Parse<TrackGenre>(e));

        builder
            .Property(e => e.Performer)
            .UseCollation(MusicServiceDbContext.CaseInsensitiveCollation)
            .HasMaxLength(Constraints.TrackPerformerMaxLength);
    }
}
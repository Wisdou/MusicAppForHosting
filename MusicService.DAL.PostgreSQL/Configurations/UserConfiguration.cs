using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicService.Domain.Constants;
using MusicService.Domain.Entities;

namespace MusicService.DAL.PostgreSQL.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder
            .HasIndex(x => x.Email)
            .UseCollation(MusicServiceDbContext.CaseInsensitiveCollation)
            .IsUnique();
        
        builder
            .Property(x => x.Username)
            .UseCollation(MusicServiceDbContext.CaseInsensitiveCollation)
            .HasMaxLength(Constraints.UsernameMaxLength);
        
        builder
            .HasIndex(x => x.Username)
            .IsUnique();
    }
}
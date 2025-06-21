namespace MusicService.Domain.Entities;

public class Playlist : Entity
{
    public required DateTimeOffset CreationDateTime { get; init; }
    
    public required string Title { get; init; }
    
    public required Guid UserId { get; init; }
    
    #region Navigation

    public User User { get; init; } = null!;
    
    public ICollection<PlaylistTrack> Tracks { get; init; } = new List<PlaylistTrack>();
    
    #endregion
}
namespace MusicService.Domain.Entities;

public class FavoriteTrack
{
    public required DateTimeOffset CreationDateTime { get; init; }
    
    public required Guid UserId { get; init; }
    
    public required Guid TrackId { get; init; }
    
    public User User { get; init; } = null!;
    
    public Track Track { get; init; } = null!;
}
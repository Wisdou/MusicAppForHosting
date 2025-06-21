namespace MusicService.Domain.Entities;

public class PlaylistTrack
{
    public required DateTimeOffset CreationDateTime { get; set; }
    
    public required Guid PlaylistId { get; init; }
    
    public required Guid TrackId { get; init; }
    
    public Track Track { get; init; } = null!;
    
    public Playlist Playlist { get; init; } = null!;
}
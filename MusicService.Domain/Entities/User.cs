namespace MusicService.Domain.Entities;

public class User : Entity
{
    public required string Username { get; init; }
    
    public required string Email { get; init; }
    
    public required string PasswordHash { get; init; }
    
    public required Role Role { get; init; }

    #region Navigation

    public ICollection<FavoriteTrack> Favorites { get; init; } = new List<FavoriteTrack>();

    #endregion
}

public enum Role
{
    Admin,
    User
}

namespace MusicService.Domain.Entities;

public abstract class Entity
{
    public required Guid Id { get; init; } = Guid.NewGuid();
}
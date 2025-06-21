using MusicService.Domain.Entities;

namespace MusicService.WebApi.Contracts.Responses;

public record TrackResponse(
    Guid Id,
    string Title,
    TrackGenre Genre,
    TimeSpan Duration,
    string Performer,
    bool IsInFavorites);
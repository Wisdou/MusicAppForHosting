using System.Text.Json.Serialization;
using MusicService.Domain.Entities;

namespace MusicService.WebApi.Contracts.Requests;

public record TrackAddRequest(
    string Title,
    [property: JsonConverter(typeof(JsonStringEnumConverter<TrackGenre>))]
    TrackGenre Genre,
    string Performer);
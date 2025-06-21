using System.IO;
using System.Net.Http;
using MusicService.WebApi.Contracts.Responses;
using Refit;

namespace MusicService.WPF.Services;

public interface ITracksEndpoints
{
    [Get("/tracks?pageIndex={pageIndex}&pageSize={pageSize}")]
    Task<ApiResponse<IEnumerable<TrackResponse>>> GetTracksAsync([Authorize] string accessToken, int pageIndex = 1, int pageSize = 1000);
    
    [Get("/tracks/favorites?pageIndex={pageIndex}&pageSize={pageSize}")]
    Task<ApiResponse<IEnumerable<TrackResponse>>> GetFavoritesAsync([Authorize] string accessToken, int pageIndex = 1, int pageSize = 1000);

    [Get("/tracks/{id}/cover")]
    Task<ApiResponse<Stream>> LoadCover([Authorize] string accessToken, Guid id);
    
    [Get("/tracks/{id}/audio")]
    Task<ApiResponse<Stream>> LoadAudio([Authorize] string accessToken, Guid id);
    
    [Multipart]
    [Post("/tracks")]
    Task<HttpResponseMessage> AddTrack([Authorize] string accessToken, FileInfoPart? cover, FileInfoPart audio, string jsonPart);
    
    [Delete("/tracks/{id}")]
    Task<HttpResponseMessage> DeleteTrack([Authorize] string accessToken, Guid id);
    
    [Post("/tracks/{id}/favorites")]
    Task<HttpResponseMessage> AddToFavorites([Authorize] string accessToken, Guid id);
    
    [Delete("/tracks/{id}/favorites")]
    Task<HttpResponseMessage> RemoveFromFavorites([Authorize] string accessToken, Guid id);
}
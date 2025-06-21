using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicService.DAL.PostgreSQL;
using MusicService.Domain.Entities;
using MusicService.WebApi.Contracts.Requests;
using MusicService.WebApi.Contracts.Responses;

namespace MusicService.WebApi.Controllers;

[Authorize(AuthenticationSchemes = $"{CookieAuthenticationDefaults.AuthenticationScheme},{JwtBearerDefaults.AuthenticationScheme}")]
public class PlaylistsController(AuthOptions authOptions, MusicServiceDbContext dbContext) : BaseController(authOptions)
{
    [HttpPost]
    public async Task<IActionResult> CreatePlaylist(CreatePlaylistRequest request)
    {
        var playlist = new Playlist
        {
            Title = request.Title,
            CreationDateTime = DateTimeOffset.UtcNow,
            UserId = (Guid)GetCurrentUserId()!,
            Id = Guid.NewGuid(),
        };
        
        dbContext.Playlists.Add(playlist);
        await dbContext.SaveChangesAsync();
        
        return Ok();
    }

    [HttpDelete("{playlistId}")]
    public async Task<IActionResult> DeletePlaylist(Guid playlistId)
    {
        _ = await dbContext.Playlists.Where(e => e.Id == playlistId).ExecuteDeleteAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetPlaylists()
    {
        var currentUserId = (Guid)GetCurrentUserId()!;
        var result = await dbContext
            .Playlists
            .Where(p => p.UserId == currentUserId)
            .OrderBy(e => e.Title)
            .ThenBy(e => e.CreationDateTime)
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.CreationDateTime,
                TracksCount = e.Tracks.Count,
            })
            .ToListAsync();
        
        return Ok(result);
    }

    [HttpGet("{playlistId}")]
    public async Task<IActionResult> GetPlaylist(Guid playlistId)
    {
        var p = await dbContext
            .Playlists
            .Where(p => p.Id == playlistId)
            .Select(e => new
            {
                e.Title,
                e.CreationDateTime,
                e.Id,
                Tracks = e.Tracks.OrderByDescending(t => t.CreationDateTime).Select(t => new TrackResponse(
                    t.TrackId, 
                    t.Track.Title, 
                    t.Track.Genre,
                    t.Track.Duration,
                    t.Track.Performer,
                    false)).ToList(), // false because it doesn't matter for playlist.
            })
            .FirstAsync();
        
        return Ok(p);
    }

    [HttpPut("{playlistId}/tracks/swap")]
    public async Task<IActionResult> SwapTracks(Guid playlistId, [FromQuery] Guid firstId, [FromQuery] Guid secondId)
    {
        var first = await dbContext
            .PlaylistTracks
            .Where(pt => pt.PlaylistId == playlistId && pt.TrackId == firstId)
            .FirstAsync();
        
        var second = await dbContext
            .PlaylistTracks
            .Where(pt => pt.PlaylistId == playlistId && pt.TrackId == secondId)
            .FirstAsync();

        (second.CreationDateTime, first.CreationDateTime) = (first.CreationDateTime, second.CreationDateTime);

        await dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{playlistId}/tracks/{trackId}")]
    public async Task<IActionResult> AddToPlaylist(Guid playlistId, Guid trackId)
    {
        if (await dbContext.PlaylistTracks.AnyAsync(t => t.PlaylistId == playlistId && t.TrackId == trackId))
        {
            return Ok();
        }
        
        var e = new PlaylistTrack
        {
            CreationDateTime = DateTimeOffset.UtcNow,
            PlaylistId = playlistId,
            TrackId = trackId,
        };
        
        dbContext.PlaylistTracks.Add(e);
        await dbContext.SaveChangesAsync();
        
        return Ok();
    }

    [HttpDelete("{playlistId}/tracks/{trackId}")]
    public async Task<IActionResult> RemoveFromPlaylist(Guid playlistId, Guid trackId)
    {
        _ = await dbContext
            .PlaylistTracks
            .Where(e => e.PlaylistId == playlistId && e.TrackId == trackId)
            .ExecuteDeleteAsync();
        
        return Ok();
    }
}
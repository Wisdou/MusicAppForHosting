using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FluentValidation;
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
public class TracksController(
    AuthOptions authOptions,
    MusicServiceDbContext dbContext,
    TimeProvider timeProvider,
    ILogger<TracksController> logger) : BaseController(authOptions: authOptions)
{
    private static readonly string[] ValidImageMimeTypes =
    [
        "image/jpeg",
        "image/png"
    ];

    private static readonly string[] ValidAudioMimeTypes =
    [
        "audio/mpeg",
    ];

    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public record TrackAddForm(
        IFormFile? Cover,
        IFormFile Audio,
        string JsonPart);

    [HttpPost]
    public async Task<IActionResult> Add(
        [FromForm] TrackAddForm form,
        [FromServices] IValidator<TrackAddRequest> validator)
    {
        if (string.IsNullOrWhiteSpace(form.JsonPart))
        {
            return BadRequest();
        }
        
        var request = JsonSerializer.Deserialize<TrackAddRequest>(form.JsonPart, JsonSerializerOptions);
        if (request is null)
        {
            return BadRequest();
        }
        
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.First().ErrorMessage);
        }

        if (!ValidateAudio(form.Audio))
        {
            return BadRequest("Невалидный аудио файл.");
        }

        if (form.Cover is not null && !ValidateCover(form.Cover))
        {
            return BadRequest("Невалидный файл изображения.");
        }
        
        var track = new Track
        {
            CreationDateTime = timeProvider.GetUtcNow(),
            Title = request.Title.Trim(),
            Genre = request.Genre,
            Audio = await ConvertToByteArrayAsync(form.Audio),
            Duration = GetDuration(form.Audio),
            Performer = request.Performer.Trim(),
            Id = Guid.NewGuid(),
            Cover = form.Cover is null ? null : await ConvertToByteArrayAsync(form.Cover),
        };

        dbContext.Tracks.Add(track);
        await dbContext.SaveChangesAsync();
        
        return Ok(track.Id);
    }

    [HttpGet]
    public async Task<IActionResult> Page(
        [Range(1, int.MaxValue)]int pageIndex = 1, 
        [Range(1, int.MaxValue)]int pageSize = 10)
    {
        if (GetCurrentUserId() is not { } userId)
        {
            return Unauthorized();
        }
        
        var result = await dbContext
            .Tracks
            .OrderBy(t => t.CreationDateTime)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new TrackResponse(
                e.Id,
                e.Title,
                e.Genre,
                e.Duration,
                e.Performer,
                dbContext.FavoriteTracks
                    .Any(ft => ft.UserId == userId && ft.TrackId == e.Id)
            ))
            .ToListAsync();
        
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            if (!await dbContext.Tracks.AnyAsync(t => t.Id == id))
            {
                return BadRequest("Трек не найден. Возможно он был удален ранее.");
            }
            
            await dbContext.Database.ExecuteSqlAsync($"DELETE FROM \"Tracks\" WHERE \"Id\" = {id}");
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Что-то пошло не так во время удаления трека.");
            await transaction.RollbackAsync();
            return BadRequest();
        }
        
        return Ok();
    }

    [HttpGet("favorites")]
    public async Task<IActionResult> FavoritesPage(
        [Range(1, int.MaxValue)]int pageIndex = 1, 
        [Range(1, int.MaxValue)]int pageSize = 10)
    {
        if (GetCurrentUserId() is not { } userId)
        {
            return Unauthorized();
        }
        
        var result = await dbContext
            .FavoriteTracks
            .OrderBy(t => t.CreationDateTime)
            .Where(t => t.UserId == userId)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new TrackResponse(e.Track.Id, e.Track.Title, e.Track.Genre, e.Track.Duration, e.Track.Performer, true))
            .ToListAsync();
        
        return Ok(result);
    }

    [HttpPost("{id}/favorites")]
    public async Task<IActionResult> AddToFavorites(Guid id)
    {
        if (GetCurrentUserId() is not { } userId)
        {
            return Unauthorized();
        }

        if (await dbContext.FavoriteTracks.AnyAsync(t => t.UserId == userId && t.TrackId == id))
        {
            return Ok();
        }

        if (!await dbContext.Tracks.AnyAsync(t => t.Id == id))
        {
            return BadRequest("Выбранный трек не найден.");
        }

        dbContext.FavoriteTracks.Add(new FavoriteTrack
        {
            UserId = userId,
            TrackId = id,
            CreationDateTime = timeProvider.GetUtcNow(),
        });
        await dbContext.SaveChangesAsync();
        
        return Ok();
    }
    
    [HttpDelete("{id}/favorites")]
    public async Task<IActionResult> RemoveFromFavorites(Guid id)
    {
        if (GetCurrentUserId() is not { } userId)
        {
            return Unauthorized();
        }

        var target = await dbContext.FavoriteTracks.FirstOrDefaultAsync(t => t.UserId == userId && t.TrackId == id);
        if (target is null)
        {
            return BadRequest("Выбранный трек не найден.");
        }
        
        dbContext.FavoriteTracks.Remove(target);
        await dbContext.SaveChangesAsync();
        
        return Ok();
    }
    
    [HttpGet("{id}/cover")]
    public async Task<IActionResult> LoadCover(Guid id)
    {
        var cover = await dbContext
            .Tracks
            .Where(t => t.Id == id)
            .Select(t => t.Cover)
            .FirstOrDefaultAsync();

        if (cover is null)
        {
            return NotFound();
        }

        return File(cover, "application/octet-stream");
    }
    
    [HttpGet("{id}/audio")]
    public async Task<IActionResult> LoadAudio(Guid id)
    {
        var audio = await dbContext
            .Tracks
            .Where(t => t.Id == id)
            .Select(t => t.Audio)
            .FirstOrDefaultAsync();

        if (audio is null)
        {
            return NotFound();
        }

        return File(audio, "audio/mpeg", enableRangeProcessing: true);
    }

    private static TimeSpan GetDuration(IFormFile audio)
    {
        using var stream = audio.OpenReadStream();
        var track = new ATL.Track(stream, audio.ContentType);
        return TimeSpan.FromSeconds(track.Duration);
    }
    
    private static async Task<byte[]> ConvertToByteArrayAsync(IFormFile formFile)
    {
        using var memoryStream = new MemoryStream();
        await formFile.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
    
    private static bool ValidateAudio(IFormFile audio)
    {
        return ValidAudioMimeTypes.Contains(audio.ContentType) && audio.Length > 0;
    }

    private static bool ValidateCover(IFormFile cover)
    {
        return ValidImageMimeTypes.Contains(cover.ContentType) && cover.Length > 0;
    }
}
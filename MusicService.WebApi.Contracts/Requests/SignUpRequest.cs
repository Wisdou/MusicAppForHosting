namespace MusicService.WebApi.Contracts.Requests;

public record SignUpRequest(
    string Username,
    string Email, 
    string Password);
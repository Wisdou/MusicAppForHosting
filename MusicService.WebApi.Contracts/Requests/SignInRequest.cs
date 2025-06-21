namespace MusicService.WebApi.Contracts.Requests;

public record SignInRequest(string EmailOrUsername, string Password);
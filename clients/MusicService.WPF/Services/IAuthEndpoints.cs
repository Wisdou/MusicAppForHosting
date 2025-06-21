using System.Net.Http;
using MusicService.WebApi.Contracts.Requests;
using MusicService.WebApi.Contracts.Responses;
using Refit;

namespace MusicService.WPF.Services;

public interface IAuthEndpoints
{
    [Post("/auth/sign-in?useCookies=false&rememberMe=true")]
    Task<ApiResponse<TokensResponse>> SignIn(SignInRequest request);
    
    [Post("/auth/sign-up")]
    Task<HttpResponseMessage> SignUp(SignUpRequest request);
    
    [Get("/auth/me")]
    Task<ApiResponse<CurrentUser>> GetMe([Authorize] string accessToken);
}
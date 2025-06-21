namespace MusicService.WebApi;

public class AuthOptions
{
    public required string Audience { get; init; }
    
    public required string Issuer { get; init; }
    
    public required string SecretKey { get; init; }
    
    public required TimeSpan ClockSkew { get; init; }
    
    public required string UsernameClaimName { get; init; }
    
    public required string RoleClaimName { get; init; }
    
    public required string UserIdClaimName { get; init; }
    
    public required string EmailClaimName { get; init; }
    
    public required TimeSpan AccessTokenLifetime { get; init; }
}
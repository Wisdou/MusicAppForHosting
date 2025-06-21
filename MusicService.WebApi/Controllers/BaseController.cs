using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace MusicService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseController(AuthOptions authOptions) : ControllerBase
{
    protected Guid? GetCurrentUserId()
    {
        if (!Guid.TryParse(HttpContext.User.FindFirstValue(authOptions.UserIdClaimName), out var userId))
        {
            return null;
        }
        
        return userId;
    }
}

using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using skillseek.Models;

namespace skillseek.Controllers;

public class BaseController : ControllerBase
{
    protected int GetUserId()
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found or invalid.");
        }

        return userId;
    }
}


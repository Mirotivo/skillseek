using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
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

    public string SanitizeFileName(string fileName)
    {
        // Remove invalid characters
        fileName = Regex.Replace(fileName, @"[^a-zA-Z0-9_\.\-]", "_");

        // Ensure a unique file name by appending a GUID
        string extension = Path.GetExtension(fileName);
        string sanitizedBaseName = Path.GetFileNameWithoutExtension(fileName);
        string uniqueName = $"{sanitizedBaseName}_{Guid.NewGuid()}{extension}";

        return uniqueName;
    }

}


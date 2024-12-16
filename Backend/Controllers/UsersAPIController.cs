using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using skillseek.Controllers;

[Route("api/users")]
[ApiController]
public class UsersAPIController : BaseController
{


    private readonly IUserService _userService;

    public UsersAPIController(
        IUserService userService
    )
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!await _userService.RegisterUser(model))
        {
            return BadRequest("A user with this email address already exists.");
        }

        return Ok();
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var result = await _userService.LoginUser(model);
        if (result == null)
        {
            return Unauthorized();
        }

        return Ok(new { token = result.Value.Token, roles = result.Value.Roles });
    }


    [HttpGet("me")]
    public async Task<IActionResult> GetAsync()
    {
        var userId = GetUserId();
        var user = await _userService.GetUser(userId);

        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(user);
    }

    [HttpGet("by-token/{recommendationToken}")]
    public async Task<IActionResult> GetUserByToken(string recommendationToken)
    {
        var user = await _userService.GetUserByToken(recommendationToken);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(user);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateAsync([FromBody] UserDto updatedUser)
    {
        var userId = GetUserId();
        if (!await _userService.UpdateUser(userId, updatedUser))
        {
            return NotFound("User not found.");
        }

        return Ok(new { success = true, message = "Profile updated successfully." });
    }

}

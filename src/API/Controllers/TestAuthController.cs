using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[Route("auth")]
[ApiController]
public class TestAuthController : ControllerBase
{
    [HttpGet("require-login")]
    [Authorize]
    public IActionResult CheckAuthenticated()
    {
        return Ok("You are authorized!");
    }

    [HttpGet("require-admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult CheckRoleAdmin()
    {
        return Ok("You are an admin!");
    }

    [HttpGet("require-permission")]
    [Authorize(Policy = "CreateProduct")]
    public IActionResult CheckPermission()
    {
        return Ok("You have the required permission!");
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Wonga.Server.interfaces;
using Wonga.Server.Models;

namespace Wonga.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly WongaDbContext _db;
        private readonly ITokenService _tokens;
        private readonly IConfiguration _config;

        public AuthController(WongaDbContext db, ITokenService tokens, IConfiguration config)
        {
            _db = db;
            _tokens = tokens;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(userDTO dto)
        {
            var exists = await _db.UserAccounts.AnyAsync(u => u.Email == dto.Email);
            if (exists) return BadRequest("Email already registered.");

            var user = new UserAccount
            {
                Name = dto.Name,
                Surname = dto.Surname,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };
            _db.UserAccounts.Add(user);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _db.UserAccounts.Include(u => u.RefreshTokens).SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized("Invalid credentials");

            var accessToken = _tokens.CreateAccessToken(user, out var accessExpiresAt);
            var refreshDays = int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "14");
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var refreshToken = _tokens.CreateRefreshToken(ip, user.Id, DateTime.UtcNow, refreshDays);
            user.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.None,
                Expires = refreshToken.ExpiresAt,
                Path = "/"
            };
            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

            return Ok(new AuthResponseDto(accessToken, accessExpiresAt, user.Email, user.Name, user.Surname));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var token)) return Unauthorized();

            var stored = await _db.RefreshTokens.Include(rt => rt.User).SingleOrDefaultAsync(rt => rt.Token == token);
            if (stored == null || stored.ExpiresAt <= DateTime.UtcNow || stored.Revoked) return Unauthorized();

            var user = stored.User!;
            var accessToken = _tokens.CreateAccessToken(user, out var accessExpiresAt);

            stored.Revoked = true;
            var refreshDays = int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "14");
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var newRt = _tokens.CreateRefreshToken(ip, user.Id, DateTime.UtcNow, refreshDays);
            user.RefreshTokens.Add(newRt);
            await _db.SaveChangesAsync();

            Response.Cookies.Append("refreshToken", newRt.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = newRt.ExpiresAt,
                Path = "/api/auth"
            });

            return Ok(new AuthResponseDto(accessToken, accessExpiresAt, user.Email, user.Name,user.Surname));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var user = await _db.UserAccounts
                .Where(x => x.Id == userId)
                .Select(x => new {
                    name = x.Name,
                    surname = x.Surname,
                    email = x.Email
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            return Ok(user);
        }

        public record AuthResponseDto(string AccessToken, DateTime ExpiresAt, string Email, string Name, string Surname);
    }
}

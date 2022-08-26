using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using ApiAuthDemo.Infrastructure.Jwt;
using ApiAuthDemo.Services;
using GuildDataApi.Models;
using GuildDataApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ApiAuthDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticateController : ControllerBase
    {
        private readonly ILogger<AuthenticateController> _logger;
        private readonly IUserService _userService;
        private readonly IPasswordService _passwordService;
        private readonly TokenManagement _tokenManagement;
        public AuthenticateController(ILogger<AuthenticateController> logger, IUserService userService, IPasswordService passwordService, TokenManagement tokenManagement)
        {
            _logger = logger;
            _userService = userService;
            _passwordService = passwordService;
            _tokenManagement = tokenManagement;
        }

        [AllowAnonymous, HttpPost("LoginUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResult))]
        public ActionResult LoginUser([FromBody] LoginRequest request)
        {
            User? user = _userService.GetUser(request.Username);
            if (user == null) return BadRequest($"Benutzer '{request.Username}' nicht bekannt");
            if (!_passwordService.ValidatePasswordForUser(user, request.Password)) return BadRequest("Das Passwort ist falsch");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, request.Username)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenManagement.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(
                _tokenManagement.Issuer,
                _tokenManagement.Audience,
                claims,
                //expires: DateTime.Now.AddMinutes(_tokenManagement.AccessExpiration),
                signingCredentials: credentials);
            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            _logger.LogInformation($"Benutzer '{request.Username}' hat sich eingeloggt.");
            return Ok(new LoginResult
            {
                User = user,
                JwtToken = token
            });
        }
    }
}

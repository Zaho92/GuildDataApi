using ApiAuthDemo.Services;
using GuildDataApi.Data;
using GuildDataApi.Models;
using GuildDataApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuildDataApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        private readonly IPasswordService _passwordService;
        private readonly IRightsService _rightsService;

        public UserController(ILogger<UserController> logger, IUserService userService, IPasswordService passwordService, IRightsService rightsService)
        {
            _logger = logger;
            _userService = userService;
            _passwordService = passwordService;
            _rightsService = rightsService;
        }

        [Authorize, HttpGet, Route("GetUsers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<User>))]
        public IActionResult GetUsers()
        {
            return Ok(_userService.GetAllUsers());
        }

        [Authorize, HttpGet, Route("GetUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ObjectResult GetUser(int idUser)
        {
            User? userEntity = _userService.GetUser(idUser);
            if (userEntity == null) return NotFound($"Der Benutzer mit der ID '{idUser}' existiert nicht.");

            return Ok(userEntity);
        }

        [Authorize, HttpPost, Route("AddUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ObjectResult AddUser(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(user.Username)) return BadRequest("Der Benutzername darf nicht leer sein.");
            if (string.IsNullOrWhiteSpace(password)) return BadRequest("Das Passwort darf nicht leer sein.");
            if (string.IsNullOrWhiteSpace(user.Firstname)) return BadRequest("Der Vorname darf nicht leer sein.");
            if (string.IsNullOrWhiteSpace(user.Lastname)) return BadRequest("Der Nachname darf nicht leer sein.");
            if (_userService.UserExists(user.Username)) return BadRequest("Dieser Benutzername existiert bereits.");

            return Ok(_userService.AddUser(user, password));
        }

        [Authorize, HttpPut, Route("EditUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ObjectResult EditUser(User user)
        {
            if (!_userService.UserExists(user.IdUser)) return NotFound($"Der Benutzer mit der ID '{user.IdUser}' existiert nicht.");
            if (_userService.UserExists(user.Username)) return BadRequest($"Der Benutzer '{user.Username}' ist schon vergeben.");
            if (!_rightsService.RightsTemplateExists(user.FkRightsTemplatesNavigation.IdRightsTemplate)) return BadRequest("Das Rechtetemplate existiert nicht.");

            return Ok(_userService.EditUser(user));
        }


        [Authorize, HttpPut, Route("ChangePassword")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ObjectResult ChangePassword(int idUser, string oldPassword, string newPassword)
        {
            User? userEntity = _userService.GetUser(idUser);
            if (userEntity == null) return NotFound($"Der Benutzer mit der ID '{idUser}' existiert nicht.");
            if (!_passwordService.ValidatePasswordForUser(userEntity, oldPassword)) return BadRequest("Das Passwort ist falsch.");

            return Ok(_userService.ChangePassword(idUser, oldPassword, newPassword));
        }

        [HttpDelete]
        [Route("DeleteUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ObjectResult DeleteUser(int idUser)
        {
            if (!_userService.UserExists(idUser)) return NotFound($"Der Benutzer mit der ID '{idUser}' existiert nicht.");

            return Ok(_userService.DeleteUser(idUser));
        }
    }
}
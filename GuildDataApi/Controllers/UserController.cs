﻿using GuildDataApi.Data;
using GuildDataApi.Models;
using GuildDataApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuildDataApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("GetUsers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<User>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ObjectResult GetUsers()
        {
            GuildDataBaseContext guildDataBaseContext = new GuildDataBaseContext();
            if (!guildDataBaseContext.User.Any()) return NotFound("Keine Benutzer vorhanden.");
            Console.WriteLine($"UserController Method 'Get()' was called");
            return Ok(guildDataBaseContext.User.Include(user => user.FkRightsTemplatesNavigation).ToList());
        }

        [HttpGet]
        [Route("GetUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ObjectResult GetUser(int idUser)
        {
            GuildDataBaseContext guildDataBaseContext = new GuildDataBaseContext();
            User? userEntity = guildDataBaseContext.User
                .Include(u => u.FkRightsTemplatesNavigation)
                .FirstOrDefault(u => u.IdUser == idUser);
            if (userEntity == null) return NotFound("Kein Benutzer mit dieser ID vorhanden.");
            return Ok(userEntity);
        }

        [HttpGet]
        [Route("LoginUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ObjectResult LoginUser(string username, string password)
        {
            GuildDataBaseContext guildDataBaseContext = new GuildDataBaseContext();
            User? userEntity = guildDataBaseContext.User
                .Include(u => u.FkRightsTemplatesNavigation)
                .FirstOrDefault(user => user.Username == username);
            if (userEntity == null) return NotFound("Benutzername nicht gefunden.");
            if (!PasswordService.ValidatePasswordForUser(userEntity, password)) return BadRequest("Das Passwort ist falsch.");
            return Ok(userEntity);
        }

        [HttpPost]
        [Route("AddUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ObjectResult AddUser(User user)
        {
            if (String.IsNullOrWhiteSpace(user.Username)) return BadRequest("Der Benutzername darf nicht leer sein.");
            if (String.IsNullOrWhiteSpace(user.Password)) return BadRequest("Das Passwort darf nicht leer sein.");
            if (String.IsNullOrWhiteSpace(user.Firstname)) return BadRequest("Der Vorname darf nicht leer sein.");
            if (String.IsNullOrWhiteSpace(user.Lastname)) return BadRequest("Der Nachname darf nicht leer sein.");
            GuildDataBaseContext guildDataBaseContext = new GuildDataBaseContext();
            User? userEntity = guildDataBaseContext.User.FirstOrDefault(user => user.Username == user.Username);
            if (userEntity != null) return BadRequest("Dieser Benutzername exisitiert bereits.");
            string salt = PasswordService.GenerateRandomSalt();
            User newUser = new User()
            {
                Username = user.Username,
                Password = PasswordService.GetHashedPassword(salt, user.Password),
                Salt = salt,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Phonenumber = user.Phonenumber,
                FkRightsTemplates = RightsService.GetStandardUserTemplate()
            };
            guildDataBaseContext.User.Add(newUser);
            guildDataBaseContext.SaveChanges();
            return Ok(newUser);
        }

        [HttpPut]
        [Route("EditUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ObjectResult EditUser(User user)
        {
            GuildDataBaseContext guildDataBaseContext = new GuildDataBaseContext();
            User? userEntity = guildDataBaseContext.User.Find(user.IdUser);
            if (userEntity == null) return BadRequest("Der Benutzer exisitiert nicht.");
            RightsTemplate? templateEntity = guildDataBaseContext.RightsTemplate.Find(user.FkRightsTemplates);
            if (templateEntity == null) return BadRequest("Das Template exisitiert nicht.");
            userEntity = user;
            guildDataBaseContext.SaveChanges();
            return Ok(userEntity);
        }

        [HttpDelete]
        [Route("DeleteUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ObjectResult DeleteUser(int idUser)
        {
            GuildDataBaseContext guildDataBaseContext = new GuildDataBaseContext();
            User? userEntity = guildDataBaseContext.User.Find(idUser);
            if (userEntity == null) return BadRequest("Der Benutzer exisitiert nicht.");
            guildDataBaseContext.User.Remove(userEntity);
            guildDataBaseContext.SaveChanges();
            return Ok(true);
        }
    }
}
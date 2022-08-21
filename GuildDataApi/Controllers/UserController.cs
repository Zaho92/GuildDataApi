using GuildDataApi.Data;
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
        public ObjectResult Get()
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
        public ObjectResult Get(int idUser)
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
        public ObjectResult Login(string username, string password)
        {
            GuildDataBaseContext guildDataBaseContext = new GuildDataBaseContext();
            User? userEntity = guildDataBaseContext.User
                .Include(u => u.FkRightsTemplatesNavigation)
                .FirstOrDefault(user => user.Username == username);
            if (userEntity == null) return NotFound("Benutzername nicht gefunden.");
            if (!PasswordService.ValidatePasswordForUser(userEntity, password)) return BadRequest("Das Passwort ist falsch.");
            return Ok(userEntity);
        }

        [HttpPut]
        [Route("AddUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ObjectResult Add(string username, string password, string firstname, string lastname, string? phonenumber = null)
        {
            if (String.IsNullOrWhiteSpace(username)) return BadRequest("Der Benutzername darf nicht leer sein.");
            if (String.IsNullOrWhiteSpace(password)) return BadRequest("Das Passwort darf nicht leer sein.");
            if (String.IsNullOrWhiteSpace(firstname)) return BadRequest("Der Vorname darf nicht leer sein.");
            if (String.IsNullOrWhiteSpace(lastname)) return BadRequest("Der Nachname darf nicht leer sein.");
            GuildDataBaseContext guildDataBaseContext = new GuildDataBaseContext();
            User? userEntity = guildDataBaseContext.User.FirstOrDefault(user => user.Username == username);
            if (userEntity != null) return BadRequest("Dieser Benutzername exisitiert bereits.");
            string salt = PasswordService.GenerateRandomSalt();
            User newUser = new User()
            {
                Username = username,
                Password = PasswordService.GetHashedPassword(salt,password),
                Salt = salt,
                Firstname = firstname,
                Lastname = lastname,
                Phonenumber = phonenumber ?? "",
                FkRightsTemplates = RightsService.GetStandardUserTemplate()
            };
            guildDataBaseContext.User.Add(newUser);
            guildDataBaseContext.SaveChanges();
            return Ok(newUser);
        }

        [HttpPost]
        [Route("ChangeRightsTemplate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ObjectResult Add(int idUser, int idRightsTemplate)
        {
            GuildDataBaseContext guildDataBaseContext = new GuildDataBaseContext();
            User? userEntity = guildDataBaseContext.User.Find(idUser);
            if (userEntity == null) return BadRequest("Der Benutzer exisitiert nicht.");
            RightsTemplate? templateEntity = guildDataBaseContext.RightsTemplate.Find(idRightsTemplate);
            if (templateEntity == null) return BadRequest("Das Template exisitiert nicht.");

            userEntity.FkRightsTemplates = idRightsTemplate;
            guildDataBaseContext.SaveChanges();
            return Ok(userEntity);
        }
    }
}
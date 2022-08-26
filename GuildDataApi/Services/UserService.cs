using GuildDataApi.Data;
using GuildDataApi.Models;
using GuildDataApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiAuthDemo.Services
{
    public interface IUserService
    {
        bool UserExists(int idUser);
        bool UserExists(string username);
        User? GetUser(int idUser);
        User? GetUser(string username);
        IEnumerable<User> GetAllUsers();
        User? AddUser(User user, string password);
        User? EditUser(User user);
        bool ChangePassword(int idUser, string oldPassword, string newPassword);
        bool DeleteUser(int idUser);
    }

    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IPasswordService _passwordService;
        private readonly IRightsService _rightsService;
        private readonly GuildDataBaseContext _dbContext;

        public UserService(ILogger<UserService> logger, IPasswordService passwordService, IRightsService rightsService)
        {
            _logger = logger;
            _passwordService = passwordService;
            _rightsService = rightsService;
            _dbContext = new GuildDataBaseContext();
        }

        public bool UserExists(int idUser)
        {
            return _dbContext.User.Any(u => u.IdUser == idUser);
        }

        public bool UserExists(string username)
        {
            return _dbContext.User.Any(u => u.Username == username);
        }

        public User? GetUser(int idUser)
        {
            User? user = _dbContext.User.Include(u => u.FkRightsTemplatesNavigation).FirstOrDefault(u => u.IdUser == idUser);
            if (user == null) _logger.LogWarning($"Benutzer mit der ID '{idUser}' konnte nicht gefunden werden.");
            else _logger.LogInformation($"Benutzer über die ID '{idUser}' abgefragt.");
            return user;
        }

        public User? GetUser(string username)
        {
            User? user = _dbContext.User.Include(u => u.FkRightsTemplatesNavigation).FirstOrDefault(u => u.Username == username);
            if (user == null) _logger.LogWarning($"Benutzer mit dem Benutzernamen '{username}' konnte nicht gefunden werden.");
            else _logger.LogInformation($"Benutzer über dem Benutzernamen '{username}' abgefragt.");
            return user;
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _dbContext.User.ToList();
        }

        public User? AddUser(User newUser, string password)
        {
            User? existingUser = GetUser(newUser.Username);
            if (existingUser != null) return null;

            string salt = _passwordService.GenerateRandomSalt();
            User user = new User()
            {
                Username = newUser.Username,
                Password = _passwordService.GetHashedPassword(salt, password),
                Salt = salt,
                Firstname = newUser.Firstname,
                Lastname = newUser.Lastname,
                Phonenumber = newUser.Phonenumber,
                FkRightsTemplates = _rightsService.GetStandardUserTemplate()
            };
            _dbContext.User.Add(user);
            _dbContext.SaveChanges();
            return user;
        }

        public User? EditUser(User changedUser)
        {
            User? user = GetUser(changedUser.IdUser);
            if (user == null) return null;

            user.Username = changedUser.Username;
            user.Firstname = changedUser.Firstname;
            user.Lastname = changedUser.Lastname;
            user.Phonenumber = changedUser.Phonenumber;
            user.FkRightsTemplates = changedUser.FkRightsTemplatesNavigation.IdRightsTemplate;
            _dbContext.SaveChanges();
            return user;
        }

        public bool ChangePassword(int idUser, string oldPassword, string newPassword)
        {
            User? user = GetUser(idUser);
            if (user == null) return false;

            if (!_passwordService.ValidatePasswordForUser(user, oldPassword)) return false;
            string salt = _passwordService.GenerateRandomSalt();
            user.Password = _passwordService.GetHashedPassword(salt, newPassword);
            user.Salt = salt;
            _dbContext.SaveChanges();
            return true;
        }

        public bool DeleteUser(int idUser)
        {
            User? user = GetUser(idUser);
            if (user == null) return false;

            _dbContext.User.Remove(user);
            _dbContext.SaveChanges();
            return true;
        }
    }
}

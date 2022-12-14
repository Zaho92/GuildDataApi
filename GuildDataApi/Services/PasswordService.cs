using GuildDataApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace GuildDataApi.Services
{
    public interface IPasswordService
    {
        bool ValidatePasswordForUser(User userEntity, string password);
        string GenerateRandomSalt(byte maximumSaltLength = 32);
        string GetHashedPassword(string salt, string password);
    }

    public class PasswordService : IPasswordService
    {
        public bool ValidatePasswordForUser(User userEntity, string password)
        {
            string hashedPassword = GetHashedPassword(userEntity.Salt, password);
            return userEntity.Password == hashedPassword;
        }

        public string GetHashedPassword(string salt, string password)
        {
            byte[] byteSalt = Convert.FromBase64String(salt);
            byte[] bytePassword = Encoding.UTF8.GetBytes(password);
            byte[] saltedHash = GenerateSaltedPasswordHash(byteSalt, bytePassword);
            return Convert.ToBase64String(saltedHash);
        }

        private byte[] GenerateSaltedPasswordHash(byte[] salt, byte[] password)
        {
            byte[] combined = salt.Concat(password).ToArray();
            using SHA256 cryptSha256 = SHA256.Create();
            return cryptSha256.ComputeHash(combined);
        }

        public string GenerateRandomSalt(byte maximumSaltLength = 32)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(maximumSaltLength);
            return Convert.ToBase64String(salt);
        }
    }
}
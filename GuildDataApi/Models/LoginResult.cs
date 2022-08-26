namespace GuildDataApi.Models
{
    public class LoginResult
    {
        public User User { get; set; }
        public string JwtToken { get; set; }
    }
}

using GuildDataApi.Data;
using GuildDataApi.Models;
using Microsoft.AspNetCore.SignalR;

namespace GuildDataApi.Services
{
    public static class RightsService
    {
        public static int GetStandardUserTemplate()
        {
            CheckOrCreateStandardTemplates();
            GuildDataBaseContext guildDataBaseContext = new GuildDataBaseContext();
            return guildDataBaseContext.RightsTemplate.First(rt => rt.IsInitialUser).IdRightsTemplate;
        }

        private static void CheckOrCreateStandardTemplates()
        {
            GuildDataBaseContext guildDataBaseContext = new GuildDataBaseContext();
            if (!guildDataBaseContext.RightsTemplate.Any(rt=>rt.IsAdmin))
            {
                guildDataBaseContext.RightsTemplate.Attach(new RightsTemplate()
                {
                    Description = "Administrator",
                    IsAdmin = true,
                    IsInitialUser = false
                });
                guildDataBaseContext.SaveChanges();
            }
            if (!guildDataBaseContext.RightsTemplate.Any(rt => rt.IsInitialUser))
            {
                guildDataBaseContext.RightsTemplate.Attach(new RightsTemplate()
                {
                    Description = "Benutzer",
                    IsAdmin = false,
                    IsInitialUser = true
                });
                guildDataBaseContext.SaveChanges();
            }
        }
    }
}

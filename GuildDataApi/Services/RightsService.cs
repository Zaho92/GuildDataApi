using GuildDataApi.Data;
using GuildDataApi.Models;
using Microsoft.AspNetCore.SignalR;

namespace GuildDataApi.Services
{
    public interface IRightsService
    {
        RightsTemplate? CurrentRightsTemplate { get; set; }
        bool RightsTemplateExists(int idTemplate);
        RightsTemplate? GetRightsTemplateById(int idTemplate);
        IEnumerable<RightsTemplate> GetRightsTemplates();
        int GetStandardUserTemplate();
    }

    public  class RightsService : IRightsService
    {
        private readonly ILogger<RightsService> _logger;
        private readonly GuildDataBaseContext _dbContext;

        public RightsTemplate? CurrentRightsTemplate { get; set; }

        public RightsService(ILogger<RightsService> logger)
        {
            _logger = logger;
            _dbContext = new GuildDataBaseContext();
        }

        public bool RightsTemplateExists(int idTemplate)
        {
            return _dbContext.RightsTemplate.Any(u => u.IdRightsTemplate == idTemplate);
        }

        public RightsTemplate? GetRightsTemplateById(int idTemplate)
        {
            CurrentRightsTemplate = _dbContext.RightsTemplate.FirstOrDefault(u => u.IdRightsTemplate == idTemplate);
            if (CurrentRightsTemplate == null) _logger.LogWarning($"Berechtigungstemplate mit der ID '{idTemplate}' konnte nicht gefunden werden.");
            else _logger.LogInformation($"Berechtigungstemplate über die ID '{idTemplate}' abgefragt.");

            return CurrentRightsTemplate;
        }

        public IEnumerable<RightsTemplate> GetRightsTemplates()
        {
            return _dbContext.RightsTemplate.ToList();
        }

        public int GetStandardUserTemplate()
        {
            CheckOrCreateStandardTemplates();
            GuildDataBaseContext guildDataBaseContext = new GuildDataBaseContext();
            return guildDataBaseContext.RightsTemplate.First(rt => rt.IsInitialUser).IdRightsTemplate;
        }

        private void CheckOrCreateStandardTemplates()
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

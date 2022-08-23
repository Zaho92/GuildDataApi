using GuildDataApi.Data;
using GuildDataApi.Models;
using GuildDataApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuildDataApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RightsTemplatesController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public RightsTemplatesController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("GetRightsTemplates")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RightsTemplate>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ObjectResult GetRightsTemplates()
        {
            GuildDataBaseContext guildDataBaseContext = new GuildDataBaseContext();
            if (!guildDataBaseContext.RightsTemplate.Any()) return NotFound("Keine Templates vorhanden.");
            return Ok(guildDataBaseContext.RightsTemplate.ToList());
        }
    }
}

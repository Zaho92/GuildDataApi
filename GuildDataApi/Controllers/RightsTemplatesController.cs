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
    public class RightsTemplatesController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IRightsService _rightsService;

        public RightsTemplatesController(ILogger<UserController> logger, IRightsService rightsService)
        {
            _logger = logger;
            _rightsService = rightsService;
        }

        [Authorize, HttpGet, Route("GetRightsTemplates")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RightsTemplate>))]
        public ObjectResult GetRightsTemplates()
        {
            return Ok(_rightsService.GetRightsTemplates());
        }
    }
}
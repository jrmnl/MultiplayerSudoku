using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace MultiplayerSudoku.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<Dictionary<string, int>> Get()
        {
            // TODO
            return new Dictionary<string, int>
            {
                { "Vasya", 123 },
                { "Qwerty", 54545 },
                { "Qwertyaaa", 54 },
                { "Qwertyaaav", 54 }
            };
        }
    }
}
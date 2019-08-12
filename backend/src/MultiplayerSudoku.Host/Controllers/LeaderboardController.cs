using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace MultiplayerSudoku.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        private readonly Leaderboard _leaderboard;

        public LeaderboardController(Leaderboard leaderboard)
        {
            _leaderboard = leaderboard ?? throw new ArgumentNullException(nameof(leaderboard));
        }

        [HttpGet]
        public ActionResult<Dictionary<string, int>> Get()
        {
            return _leaderboard.GetWins();
        }
    }
}
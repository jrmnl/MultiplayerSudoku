using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MultiplayerSudoku.Application.Contract;

namespace MultiplayerSudoku.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboard;

        public LeaderboardController(ILeaderboardService leaderboard)
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
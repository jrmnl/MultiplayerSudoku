using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MultiplayerSudoku.Application.Contract;

namespace MultiplayerSudoku.Application
{
    public class Leaderboard : ILeaderboardService
    {
        private readonly ConcurrentDictionary<string, int> _leaders;

        public Leaderboard()
        {
            _leaders = new ConcurrentDictionary<string, int>();
        }

        public void AddWin(string username)
        {
            _leaders.AddOrUpdate(username, 1, (_, win) => win + 1);
        }

        public Dictionary<string, int> GetWins()
        {
            return _leaders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}

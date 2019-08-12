using System.Collections.Generic;

namespace MultiplayerSudoku.Application.Contract
{
    public interface ILeaderboardService
    {
        void AddWin(string username);
        Dictionary<string, int> GetWins();
    }
}

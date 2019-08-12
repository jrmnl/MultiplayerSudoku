using MultiplayerSudoku.Application.Contract.Messages;

namespace MultiplayerSudoku.Application.Contract
{
    public interface IGameAgent
    {
        void Post(GameMessage msg);
    }
}

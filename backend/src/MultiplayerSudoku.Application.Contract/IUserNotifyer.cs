using MultiplayerSudoku.Application.Contract.Messages;

namespace MultiplayerSudoku.Application.Contract
{
    public interface IUserNotifyer
    {
        void Post(UserMessage message);
        void Complete();
    }
}

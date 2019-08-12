using System;

namespace MultiplayerSudoku.Application.Contract.Messages
{
    public abstract class GameMessage
    {
        protected GameMessage(Guid peerId)
        {
            PeerId = peerId;
        }

        public Guid PeerId { get; }

        public class NewPeer : GameMessage
        {
            public NewPeer(Guid peerId, string username, IUserNotifyer notifyer) : base(peerId)
            {
                Username = username ?? throw new ArgumentNullException(nameof(username));
                Notifyer = notifyer ?? throw new ArgumentNullException(nameof(notifyer));
            }

            public string Username { get; }
            public IUserNotifyer Notifyer { get; }
        }

        public class LostConnection : GameMessage
        {
            public LostConnection(Guid peerId) : base(peerId)
            {
            }
        }

        public class Update : GameMessage
        {
            public Update(Guid peerId, int row, int column, int value) : base(peerId)
            {
                Row = row;
                Column = column;
                Value = value;
            }

            public int Row { get; }
            public int Column { get; }
            public int Value { get; }
        }
    }
}

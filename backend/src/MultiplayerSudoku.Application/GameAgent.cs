using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using MultiplayerSudoku.Application.Contract;
using MultiplayerSudoku.Application.Contract.Messages;
using MultiplayerSudoku.Logic;

namespace MultiplayerSudoku.Application
{
    public class GameAgent : IGameAgent
    {
        private const int MAX_MISSING_CELLS_IN_ROW = 1;

        private SudokuBoard _board;
        private readonly ILeaderboardService _leaderboard;
        private readonly Dictionary<Guid, (string Name, IUserNotifyer Subscription)> _participants;
        private readonly ActionBlock<GameMessage> _agent;

        public GameAgent(ILeaderboardService leaderboard)
        {
            _leaderboard = leaderboard ?? throw new ArgumentNullException(nameof(leaderboard));

            _board = new SudokuBoard(MAX_MISSING_CELLS_IN_ROW);
            _participants = new Dictionary<Guid, (string Name, IUserNotifyer Subscription)>();
            _agent = new ActionBlock<GameMessage>(ProcessMessage);
        }

        public void Post(GameMessage msg)
        {
            _agent.Post(msg);
        }

        public void ProcessMessage(GameMessage msg)
        {
            switch (msg)
            {
                case GameMessage.LostConnection lostConn:
                    HandleLostConnection(lostConn);
                    break;


                case GameMessage.NewPeer subscriber:
                    HandleNewPeer(subscriber);
                    break;

                case GameMessage.Update updation:
                    HandleUpdate(updation);

                    break;

                default:
                    throw new InvalidOperationException($"Not supported message {msg.GetType().Name}");
            }
        }

        private void HandleUpdate(GameMessage.Update updation)
        {
            if (!_participants.ContainsKey(updation.PeerId))
            {
                return;
            }

            if (_board.TryUpdateAt(updation.Row, updation.Column, updation.Value))
            {
                var updateMsg = new UserMessage.NewUpdate(updation.Row, updation.Column, updation.Value);
                SendMessageForAll(updateMsg);

                if (_board.Status != SudokuBoard.GameStatus.InProgress)
                {
                    var winner = _board.Status == SudokuBoard.GameStatus.Correct
                        ? _participants[updation.PeerId].Name
                        : null;

                    if (winner != null)
                    {
                        _leaderboard.AddWin(winner);
                    }

                    var gameEndMsg = new UserMessage.GameEnd(winner);
                    SendMessageForAll(gameEndMsg);

                    Reset();
                }
            }
        }

        private void HandleNewPeer(GameMessage.NewPeer subscriber)
        {
            if (_participants.ContainsKey(subscriber.PeerId))
            {
                var conflictMsg = new UserMessage.NameConflict(subscriber.Username);
                subscriber.Notifyer.Post(conflictMsg);
            }
            else
            {
                _participants.Add(subscriber.PeerId, (subscriber.Username, subscriber.Notifyer));
                var stateMsg = new UserMessage.CurrentState(_board.CurrentState);
                subscriber.Notifyer.Post(stateMsg);
            }
        }

        private void HandleLostConnection(GameMessage.LostConnection lostConn)
        {
            if (_participants.TryGetValue(lostConn.PeerId, out var info))
            {
                info.Subscription.Complete();
                _participants.Remove(lostConn.PeerId);
            }
        }

        private void SendMessageForAll(UserMessage message)
        {
            foreach(var participant in _participants.Select(kvp => kvp.Value.Subscription))
            {
                participant.Post(message);
            }
        }

        private void Reset()
        {
            foreach (var participant in _participants.Select(kvp => kvp.Value.Subscription))
            {
                participant.Complete();
            }
            _participants.Clear();
            _board = new SudokuBoard(MAX_MISSING_CELLS_IN_ROW);
        }
    }
}

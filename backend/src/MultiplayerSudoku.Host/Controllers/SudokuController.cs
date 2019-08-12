using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using MultiplayerSudoku.Logic;
using Newtonsoft.Json;

namespace MultiplayerSudoku.Host.Controllers
{
    [Route("api/[controller]")]
    public class SudokuController : Controller
    {
        private readonly GameAgent _gameAgent;

        public SudokuController(GameAgent gameAgent)
        {
            _gameAgent = gameAgent ?? throw new ArgumentNullException(nameof(gameAgent));
        }

        [HttpGet("start")]
        public async Task ConnectGame()
        {
            var context = ControllerContext.HttpContext;
            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            var upgradeFeature = context.Features.OfType<IHttpUpgradeFeature>().SingleOrDefault();
            if (isSocketRequest)
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                while(socket.State == WebSocketState.Open)
                {
                    var msg = await socket.GetMessage<InMessage1>();
                    switch(msg)
                    {
                        case InMessage1.Connect cnct:
                            {
                                await _gameAgent.ProcessMessage(
                                    new InMessage.NewSubscriber(cnct.Username, socket));
                            }
                            break;

                        case InMessage1.Update upd:
                            {
                                await _gameAgent.ProcessMessage(
                                    new InMessage.Update(
                                        upd.Username,
                                        upd.Row,
                                        upd.Column,
                                        upd.Value));
                            }
                            break;

                        default: throw new InvalidOperationException();
                    }
                }
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
    }

    public abstract class OutMessage
    {
        public class CurrentState : OutMessage
        {
            public CurrentState(int[,] sudokuBoard)
            {
                SudokuBoard = sudokuBoard ?? throw new ArgumentNullException(nameof(sudokuBoard));
            }

            public int[,] SudokuBoard { get; }
        }

        public class GameEnd : OutMessage
        {
            public GameEnd(string winner)
            {
                Winner = winner;
            }

            public string Winner { get; }
        }

        public class NewUpdate : OutMessage
        {
            public NewUpdate(int row, int column, int value)
            {
                Row = row;
                Column = column;
                Value = value;
            }

            public int Row { get; }
            public int Column { get; }
            public int Value { get; }
        }

        public class WrongUpdate : OutMessage
        {
            public WrongUpdate(int row, int column, int value)
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

    public abstract class InMessage1
    {
        protected InMessage1(string username)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
        }

        public string Username { get; }

        public class Connect : InMessage1
        {
            public Connect(string username) : base(username)
            {
            }
        }

        public class Update : InMessage1
        {
            public Update(string username, int row, int column, int value) : base(username)
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

    public abstract class InMessage
    {
        protected InMessage(string username)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
        }

        public string Username { get; }

        public class NewSubscriber : InMessage
        {
            public NewSubscriber(string username, WebSocket socket) : base(username)
            {
                Socket = socket ?? throw new ArgumentNullException(nameof(socket));
            }

            public WebSocket Socket { get; }
        }

        public class Update : InMessage
        {
            public Update(string username, int row, int column, int value) : base(username)
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

    // create agent for each socket
    public class GameAgent
    {
        private readonly Leaderboard _leaderboard;
        private SudokuBoard _board;
        private readonly Dictionary<string, WebSocket> _participants;

        public GameAgent(Leaderboard leaderboard)
        {
            _board = new SudokuBoard(5);
            _leaderboard = leaderboard ?? throw new ArgumentNullException(nameof(leaderboard));
            _participants = new Dictionary<string, WebSocket>();
        }

        public async Task ProcessMessage(InMessage msg)
        {
            switch (msg)
            {
                case InMessage.NewSubscriber subscriber:
                    {
                        _participants.Add(subscriber.Username, subscriber.Socket);
                        await subscriber.Socket.SendMessage(_board.CurrentState, CancellationToken.None);
                    }
                    break;

                case InMessage.Update updation:
                    if (_board.TryUpdateAt(updation.Row, updation.Column, updation.Value))
                    {
                        var updateMsg = new OutMessage.NewUpdate(updation.Row, updation.Column, updation.Value);
                        await _participants
                            .Select(kvp => kvp.Value)
                            .SendMessageForAll(updateMsg, CancellationToken.None);

                        if (_board.Status != SudokuBoard.GameStatus.InProgress)
                        {
                            if(_board.Status == SudokuBoard.GameStatus.Correct)
                            {
                                _leaderboard.AddWin(updation.Username);
                            }

                            var gameEndMsg = new OutMessage.GameEnd(
                                _board.Status == SudokuBoard.GameStatus.Correct
                                    ? updation.Username
                                    : null);
                            await _participants
                                .Select(kvp => kvp.Value)
                                .SendMessageForAll(gameEndMsg, CancellationToken.None);
                            //TODO: close
                            _participants.Clear();
                            _board = new SudokuBoard(5);
                        }
                    }
                    else
                    {
                        var wrongUpdateMsg = new OutMessage.WrongUpdate(updation.Row, updation.Column, updation.Value);
                        await _participants[updation.Username]
                            .SendMessage(wrongUpdateMsg, CancellationToken.None);
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Not supported message {msg.GetType().Name}");
            }
        }
    }

    internal static class SocketExtensions
    {
        public static Task SendMessageForAll<T>(this IEnumerable<WebSocket> sockets, T message, CancellationToken token)
        {
            var tasks = new List<Task>();
            foreach(var socket in sockets)
            {
                tasks.Add(socket.SendMessage(message, token));
            }
            return Task.WhenAll(tasks);
        }

        public static async Task<T> GetMessage<T>(this WebSocket socket)
        {
            var arraySegment = new ArraySegment<byte>(new byte[1024 * 4]);

            var recievedBytes = new List<byte>();
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(arraySegment, CancellationToken.None);
                if (result.MessageType != WebSocketMessageType.Text)
                    throw new InvalidOperationException("Not text message");
                recievedBytes.AddRange(arraySegment.Take(result.Count));
            }
            while (!result.EndOfMessage);

            var json = Encoding.ASCII.GetString(recievedBytes.ToArray());
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static Task SendMessage<T>(this WebSocket socket, T message, CancellationToken token)
        {
            var serialized = JsonConvert.SerializeObject(message);
            var bytes = Encoding.ASCII.GetBytes(serialized);
            var arraySegment = new ArraySegment<byte>(bytes);
            return socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, token);
        }
    }
}
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MultiplayerSudoku.Application;
using MultiplayerSudoku.Application.Contract;
using MultiplayerSudoku.Application.Contract.Messages;
using MultiplayerSudoku.Host.Exceptions;

namespace MultiplayerSudoku.Host.Middlewares
{
    public class SudokuSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IGameAgent _gameAgent;

        public SudokuSocketMiddleware(IGameAgent gameAgent, RequestDelegate next)
        {
            _gameAgent = gameAgent ?? throw new ArgumentNullException(nameof(gameAgent));
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path != "/sudoku/ws" ||
                !context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }

            var peerId = Guid.NewGuid();
            var socket = await context.WebSockets.AcceptWebSocketAsync();
            while (socket.CloseStatus == null)
            {
                var msg = await Listen(peerId, socket);
                _gameAgent.Post(msg);
            }
        }

        private static async Task<GameMessage> Listen(Guid peerId, WebSocket socket)
        {
            try
            {
                var message = await socket.GetMessage<Message>();
                switch (message)
                {
                    case Message.Initialize cnct:
                        return new GameMessage.NewPeer(
                            peerId,
                            cnct.Username,
                            new UserNotifyer(socket)); //TODO: factory/func

                    case Message.Update upd:
                        return new GameMessage.Update(
                            peerId,
                            upd.Row,
                            upd.Column,
                            upd.Value);

                    default: throw new InvalidOperationException();
                }
            }
            catch (WebSocketClosedException)
            {
                return new GameMessage.LostConnection(peerId);
            }
        }
    }
}
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MultiplayerSudoku.Application.Contract;
using MultiplayerSudoku.Application.Contract.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MultiplayerSudoku.Application
{
    public class UserNotifyer : IUserNotifyer
    {
        private WebSocket _socket;
        private ActionBlock<UserMessage> _agent;

        public UserNotifyer(WebSocket socket)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _agent = new ActionBlock<UserMessage>(ProcessMessage);
        }

        public void Post(UserMessage message)
        {
            _agent.Post(message);
        }

        public void Complete()
        {
            _agent.Complete();
            _agent.Completion
                .ContinueWith(_ => _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None));
        }

        private Task ProcessMessage(UserMessage message)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var serialized = JsonConvert.SerializeObject(message, settings);
            var bytes = Encoding.ASCII.GetBytes(serialized);
            var arraySegment = new ArraySegment<byte>(bytes);
            return _socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}

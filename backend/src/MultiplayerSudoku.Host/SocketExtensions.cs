using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MultiplayerSudoku.Host.Exceptions;
using Newtonsoft.Json;

namespace MultiplayerSudoku.Host
{
    public static class SocketExtensions
    {
        public static async Task<T> GetMessage<T>(this WebSocket socket)
        {
            var arraySegment = new ArraySegment<byte>(new byte[1024 * 4]);

            var recievedBytes = new List<byte>();
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(arraySegment, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                    throw new WebSocketClosedException();
                if (result.MessageType == WebSocketMessageType.Binary)
                    throw new InvalidOperationException("Not text message");
                recievedBytes.AddRange(arraySegment.Take(result.Count));
            }
            while (!result.EndOfMessage);

            var json = Encoding.ASCII.GetString(recievedBytes.ToArray());
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
using System;
using JsonSubTypes;
using Newtonsoft.Json;

namespace MultiplayerSudoku.Host
{
    [JsonConverter(typeof(JsonSubtypes), "type")]
    [JsonSubtypes.KnownSubType(typeof(Message.Initialize), "connect")]
    [JsonSubtypes.KnownSubType(typeof(Message.Update), "update")]
    public abstract class Message
    {
        public class Initialize : Message
        {
            public Initialize(string username)
            {
                Username = username ?? throw new ArgumentNullException(nameof(username));
            }

            public string Username { get; }
        }

        public class Update : Message
        {
            public Update(int row, int column, int value)
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

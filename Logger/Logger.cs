using Discord;
using static System.Guid;

namespace PrototonBot.Log
{
    public abstract class Logger : ILogger
    {
        public string _guid;
        public Logger()
        {
            _guid = NewGuid().ToString()[^4..];
        }

        public abstract Task Log(LogMessage message);
    }
}
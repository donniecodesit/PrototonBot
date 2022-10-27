using Discord;

namespace PrototonBot.Log
{
    public class ConsoleLogger : Logger
    {
        // Override Log method from ILogger, passing message to LogToConsoleAsync()
        public override async Task Log(LogMessage message)
        {
            // Using Task.Run() in case there are any long-running actions, to prevent blocking the Gateway.
            await Task.Run(() => LogToConsoleAsync(this, message));
        }

        private Task LogToConsoleAsync<T>(T logger, LogMessage message) where T : ILogger
        {
            Console.WriteLine($"guid:{_guid} : " + message);
            return Task.CompletedTask;
        }
    }
}

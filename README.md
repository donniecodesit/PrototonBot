PrototonBot
===
This is the successor to ProtoBot Classic, a Discord Bot written by [Donovan Laws](https://github.com/donovanlaws).
The previous bot was written in JavaScript, and was rewritten and improved later in 2020 in the C# language.

### Setup
In order to set this bot up, check the requirements below and ensure they are met:
- First, clone the latest version of this repository.
- Compile, by running `dotnet build` inside the root directory of the repository. Output will appear somewhere inside `bin` (probably `bin/Debug/net5.0`).
- Next, copy the `Storage` directory, alongside the compiled bot itself, to wherever you want to run it.
- Update `Storage/config.toml` with any API keys and IDs you need.
- Finally, start MongoDB and run the bot with `./PrototonBot.exe` (or just `./PrototonBot` on *nix systems).


### Requirements
- .NET is required for this bot to compile. Version 5.0x is used in production: [Download Here](https://dotnet.microsoft.com/download/dotnet/5.0)
- MongoDB is required for this bot to operate. Version 4.4x is used in production: [Download Here](https://www.mongodb.com/try/download/community)
- The MonogoDB server run locally (localhost) alongside the bot on the same server.
- This bot is able to be run 24/7 with proper setup using PM2 (or other services) on a server.

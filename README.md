PrototonBot
===

This is the successor to ShepherdBot Classic, a Discord Bot written by [Donovan Laws](https://github.com/donovanlaws).
The previous bot was written in JavaScript, and was rewritten and improved later in 2020 in the C# language.
It is now being completely revised and renewed at the beginning of 2022 using everything I've learned about programming from my engineering course,
and general knowledge and habits for formatting and commenting well inside of my code. :)


Before setting this bot up, please check the requirements below and ensure they're met, then follow setup.

### Requirements
- .NET is required for this bot to compile. Version 5.0.x is used in production: [Download Here](https://dotnet.microsoft.com/download/dotnet/5.0)
- MongoDB is required for this bot to operate. Version 4.4.x is used in production: [Download Here](https://www.mongodb.com/try/download/community)
- The URL for the MongoDB server by default is running locally alongside the bot on the same server. Change this as needed.
- Not a requirement, but best practice to learn how to run the bot 24/7 using services like PM2 on a server.

### Setup
- First, clone or fork the latest version of this repository to your local machine in a new repo.
- Compile, by running `dotnet build` inside the root directory of the repository. Output should appear in `bin/Debug/net5.0`.
- Next, you have two choices:
-  A. Copy the `Storage` directory into the `net5.0` folder.
-  B. Copy the `Storage` directory and the contents of the `net5.0` to a new folder somewhere.]]
- >I prefer option A, because every time I rebuild the project, it does not modify `net5.0/Storage`, so I can always run the project from there.
- Update `(net5.0 or your folder)`/`Storage/config.toml` with the API keys and IDs required inside the file. DiscordBotList is optional.
- Finally, start your MongoDB server and run the bot by either opening `PrototonBot.exe` inside your build folder, 
- >or running with `./PrototonBot.exe` (or just `./PrototonBot` on *nix systems).

### License
This project is licensed under the Creative Commons BY-NC-SA 4.0 License.
You can read about the license [here](https://creativecommons.org/licenses/by-nc-sa/4.0), or the full license text in [license.txt](https://github.com/donovanlaws/PrototonBot/blob/main/license.txt).
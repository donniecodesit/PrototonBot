<div align="center">
   <img alt="PrototonBot" src="https://i.imgur.com/2aCduA6.png"/>
   <img alt="Discord" src="https://img.shields.io/discord/919135423090024499?color=%238D59FF&label=Discord%20Support%20Server">
</div>

# What is Prototon Bot?

*Click [here](https://discord.com/oauth2/authorize?client_id=919110017142095914&permissions=8&scope=bot) to invite PrototonBot to your server. :)*

PrototonBot is the successor to ShepherdBot Classic, a Discord Bot written by [Donovan Laws](https://github.com/donovanlaws).
The previous bot had been written in JavaScript and was later rewritten and improved in 2020 in C# with Discord.Net.
The old bot has been since discontinued and is now being completely revised and renewed as of 2022, using everything I've learned about programming so far.
I've also been taking an Engineering course at Thinkful and have learned a lot about JavaScript and Front-End and Back-End development, but that's not quite related to this bot.

# How to build Prototon Bot

## Notice
Please be careful when building this project and follow the instructions accordingly as they are listed. Every build here is considered "functional and tested".
It may be best to only download the source code or compiled bot from the *Releases* tab, as main branch still undergoes changes and updates.
**I nor any co-developers will assist or support developing, building, or hosting your modified bot.**
The time, people, and resources to do this are unavailable. Follow the instructions below!

### Requirements
Before building this bot and setting it up, you're going to need these requirements.
- .NET is required for this bot to compile. Version 5.0.x is used in production: [Download Here](https://dotnet.microsoft.com/download/dotnet/5.0)
- MongoDB is required for this bot to operate. Version 4.4.x is used in production: [Download Here](https://www.mongodb.com/try/download/community)
- Otherwise, making sure to properly follow setup. To test the Mongo databast locally, you can run it on `localhost`.

### Modifying The Bot
If you're familiar with the C# language, take some time to read through the layout of the project to understand how the bot works.
In it's current state, it's very easy to customize and add/remove to this bot.
If you add or remove commands, be sure to update that on the `help` command.
If you add or remove user info or inventory keys, or server keys, be sure to update the objects in `MongoUtil/`

### Setup
1. Fork or clone the latest version of this respository, or use the source code from the *releases* tab.
2. Compile the bot inside the root directory using `dotnet build`. The output will then appear in `bin/Debug/net5.0`.
3. Next, copy the `Storage` directory into the `net5.0` folder, the bot requires that folder.
4. Update `net5.0/Storage/config.toml` with the API keys and IDs required inside the file.
   - Required: `DiscordToken`, `UserID`, `MongoURL`, `DeveloperIDs`, `CacheDir`.
   - Optional: `EnableBotList`, `BotListToken`. (Only relevant if you're wanting to list your bot on Discord Bot List's website.)
5. Start your MongoDB server and make sure to add these collections: `servers.info`, `users.info`, `users.inventory`.
6. Start the bot by opening `PrototonBot.exe`, or the command `./PrototonBot`.

# Contributors and Resources

## Contributors
Over the course of the bot's lifetime, three users have helped with this bot.
For now, each of them will remain anonymous unless they request to be added to this README.
- Thank you to the one who has helped consistently with the bot's programming and server setup. 💜
- Thank you to the one who helped implement a few commands and also gave some general tips and advice. 💙
- Thank you to the one who has been the morale support and curious one with the bot. 🧡

If you want to contribute or help, please feel free to join our [TO_BE_ADDED_SERVER](https://discord.com) and ask for help/feedback in the support section.
Or you can provide ideas, thoughts, feedback, or contributions to the features section.

## Resources and Tools used:
- [Discord.Net](https://github.com/discord-net/Discord.Net)
- [DiscordBotList's API](https://top.gg/)
- [MongoDB Driver](https://github.com/mongodb/mongo-csharp-driver)
- [Nett](https://github.com/paiden/Nett)
- [Magick.NET-Q8-AnyCPU](https://github.com/dlemstra/Magick.NET)

# Legal Stuffs..

## Using PrototonBot
Please provide credit in your project wherever it is due. It's important to credit the original creators of projects, spend some time reading the license.
I do not mind users using this bot, please use and follow the same license agreement, and do not just copy/modify the project without crediting me. ^^

## License
This project is licensed under the Creative Commons BY-NC-SA 4.0 License.
You can read about the license [here](https://creativecommons.org/licenses/by-nc-sa/4.0), or the full license text in [license.txt](https://github.com/donovanlaws/PrototonBot/blob/main/license.txt).

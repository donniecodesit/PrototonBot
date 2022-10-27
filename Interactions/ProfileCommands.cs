using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using PrototonBot.MongoUtility;
using ImageMagick;
using MongoDB.Driver;
using System.Net;

namespace PrototonBot.Interactions
{
    public class ProfileCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("simpleprofile", "[profile] Display your profile in an embed")]
        public async Task SimpleProfile([Summary(description: "A tagged user (@)")] SocketUser user)
        {
            if (user.IsBot)
            {
                await RespondAsync("Sorry, but bots do not have profiles. :robot:");
                return;
            }

            UserObject mongoUsr = MongoHandler.GetUser(user.Id.ToString()).Result;
            InventoryObject mongoInv = MongoHandler.GetInventory(user.Id.ToString()).Result;
            if (mongoUsr == null || mongoInv == null)
            {
                var newUser = Context.Client.GetUserAsync(user.Id).Result;
                await MongoHandler.CreateNewUser(newUser);
                mongoUsr = MongoHandler.GetUser(user.Id.ToString()).Result;
                mongoInv = MongoHandler.GetInventory(user.Id.ToString()).Result;
            }

            var _embed = new EmbedBuilder();
            if (Utilities.IsUserDeveloper(user.Id.ToString()))
            {
                _embed.WithTitle($":star2: {user.Username}'s Wallet :star2:");
                _embed.WithColor(0xB285FE);
            }
            else
            {
                _embed.WithTitle($"{user.Username}'s Wallet");
                _embed.WithColor(0xB2A2F1);
            }

            _embed.AddField("Account Holder", mongoUsr.Name, true);
            _embed.AddField("Account Balance", mongoUsr.Money, true);
            _embed.AddField("Pats Received", mongoUsr.PatsReceived, true);
            _embed.AddField("Level", mongoUsr.Level, true);
            _embed.AddField("Total Purchases", mongoUsr.Purchases, true);
            _embed.WithFooter("Use profile for much more detailed information.");
            await RespondAsync("", embed: _embed.Build());
        }

        [SlashCommand("embedprofile", "[profile] Display your profile in a detailed embed")]
        public async Task EmbedProfile([Summary(description: "A tagged user (@)")] SocketUser user)
        {
            if (user.IsBot)
            {
                await RespondAsync("Sorry, but bots do not have profiles. :robot:");
                return;
            }

            UserObject mongoUsr = MongoHandler.GetUser(user.Id.ToString()).Result;
            InventoryObject mongoInv = MongoHandler.GetInventory(user.Id.ToString()).Result;
            if (mongoUsr == null || mongoInv == null)
            {
                var newUser = Context.Client.GetUserAsync(user.Id).Result;
                await MongoHandler.CreateNewUser(newUser);
                mongoUsr = MongoHandler.GetUser(user.Id.ToString()).Result;
                mongoInv = MongoHandler.GetInventory(user.Id.ToString()).Result;
            }

            double multiplier = 1;
            if (mongoUsr.Boosted || mongoUsr.Mutuals) multiplier = 1.05;
            if (mongoUsr.Boosted && mongoUsr.Mutuals) multiplier = 1.1;

            var _embed = new EmbedBuilder();
            if (Utilities.IsUserDeveloper(user.Id.ToString()))
            {
                _embed.WithTitle($":star2: {user.Username}'s Profile :star2:");
                _embed.WithColor(0xB285FE);
                _embed.WithDescription($"This user is a developer for PrototonBot!\n{mongoUsr.Description}");
            }
            else
            {
                _embed.WithTitle($"{user.Username}'s Profile");
                _embed.WithColor(0xB2A2F1);
                _embed.WithDescription($"{mongoUsr.Description}");
            }

            _embed.WithThumbnailUrl(user.GetAvatarUrl());
            _embed.WithFooter("Boosted & Mutual Partners both provide a 5% boost to Chat EXP and Protobucks.");
            _embed.AddField(":moneybag: Protobucks", mongoUsr.Money, true);
            _embed.AddField(":game_die: Level & EXP", $"**{mongoUsr.Level}** ({mongoUsr.EXP})", true);
            _embed.AddField(":speak_no_evil: Pats Received", mongoUsr.PatsReceived, true);
            _embed.AddField(":money_with_wings: Items Bought", mongoUsr.Purchases, true);

            if (mongoUsr.Partner != "None")
            {
                var partner = MongoHandler.GetUser(mongoUsr.Partner).Result;
                _embed.AddField(":knife: Crime Partner", partner.Name, true);
            }
            else
            {
                _embed.AddField(":knife: Crime Partner", "None", true);
            }

            _embed.AddField(":busts_in_silhouette: Mutual Partners", mongoUsr.Mutuals, true);
            _embed.AddField("Coins", $"Daily Coins: **{mongoInv.DailyCoins}**/{mongoInv.DailyCoinsTotal}\nPat Coins: **{mongoInv.PatCoins}**/{mongoInv.PatCoinsTotal}\nGamble Coins: **{mongoInv.GambleCoins}**/{mongoInv.GambleCoinsTotal}", true);
            _embed.AddField(":money_mouth: Gambles", $"Times: {mongoUsr.Gambles}\nWins: {mongoUsr.GamblesWon} *({mongoUsr.GamblesNetGain})*\nLosses: {mongoUsr.GamblesLost} *({mongoUsr.GamblesNetLoss})*", true);
            _embed.AddField(":arrow_double_up: Stats", $"Luck: {mongoUsr.Luck}\nDaily Bonus: {mongoUsr.DailyBonus}", true);
            _embed.AddField("Transfers", $"Received: {mongoUsr.TransferIn}\nSent: {mongoUsr.TransferOut}", true);
            _embed.AddField(":chart_with_upwards_trend: PrototonBot Boosted", $"{mongoUsr.Boosted} ({multiplier}x)", true);

            await RespondAsync("", embed: _embed.Build());
        }

        [SlashCommand("setdescription", "[profile] Set your profile description. Max chars 240.")]
        public async Task SetDescription([Summary(description: "Your new description. Type 'clear' to remove.")] String message)
        {
            UserObject mongoUsr = MongoHandler.GetUser(Context.User.Id.ToString()).Result;
            if (message.ToLower() == "clear")
            {
                await MongoHandler.UpdateUser(mongoUsr.Id, "Description", "This user has not set a description.");
                await RespondAsync("Your description has been cleared!");
                return;
            }
            else
            {
                if (message.Count() > 240)
                {
                    await RespondAsync($"Please keep your description within 240 characters.\nYour character count was: {message.Count()}/240");
                }
                else
                {
                    await MongoHandler.UpdateUser(mongoUsr.Id, "Description", message);
                    await RespondAsync($"Alright, done! Your description has been updated!\n> {message}");
                }
                return;
            }
        }

        [SlashCommand("inventory", "[profile] Display your inventory in a detailed embed")]
        public async Task EmbedInventory([Summary(description: "A tagged user (@)")] SocketUser user)
        {
            if (user.IsBot)
            {
                await RespondAsync("Sorry, but bots do not have inventories. :robot:");
                return;
            }

            InventoryObject mongoInv = MongoHandler.GetInventory(user.Id.ToString()).Result;
            if (mongoInv == null)
            {
                var newUser = Context.Client.GetUserAsync(user.Id).Result;
                await MongoHandler.CreateNewUser(newUser);
                mongoInv = MongoHandler.GetInventory(user.Id.ToString()).Result;
            }

            var _embed = new EmbedBuilder();
            if (Utilities.IsUserDeveloper(user.Id.ToString()))
            {
                _embed.WithTitle($":star2: {user.Username}'s Inventory :star2:");
                _embed.WithColor(0xB285FE);
            }
            else
            {
                _embed.WithTitle($"{user.Username}'s Inventory");
                _embed.WithColor(0xB2A2F1);
            }
            _embed.WithThumbnailUrl(Context.Client.GetUserAsync(Convert.ToUInt64(user.Id.ToString())).Result.GetAvatarUrl());
            _embed.WithFooter("Numbers in parenthesis are how many uses that item has left.");
            _embed.AddField(":pick:", $"{mongoInv.Picks} ({mongoInv.PickUses})", true);
            _embed.AddField(":gem:", $"{mongoInv.Diamonds}", true);
            _embed.AddField(":bricks:", $"{mongoInv.Bricks}", true);
            _embed.AddField(":wrench:", $"{mongoInv.Wrenches} ({mongoInv.WrenchUses})", true);
            _embed.AddField(":nut_and_bolt:", $"{mongoInv.Bolts}", true);
            _embed.AddField(":gear:", $"{mongoInv.Gears}", true);
            _embed.AddField(":axe:", $"{mongoInv.Axes} ({mongoInv.AxeUses})", true);
            _embed.AddField(":evergreen_tree:", $"{mongoInv.Logs}", true);
            _embed.AddField(":fallen_leaf:", $"{mongoInv.Leaves}", true);
            _embed.AddField(":paperclip:", $"{mongoInv.Paperclips}", true);
            _embed.AddField(":bulb:", $"{mongoInv.Bulbs}", true);
            _embed.AddField(":cd:", $"{mongoInv.CDs}", true);
            await RespondAsync("", embed: _embed.Build());
        }

        [SlashCommand("daily", "[profile] Claim your daily rewards, try and maintain a streak!")]
        public async Task DailyRedemption()
        {
            UserObject mongoUsr = MongoHandler.GetUser(Context.Interaction.User.Id.ToString()).Result;
            InventoryObject mongoInv = MongoHandler.GetInventory(Context.Interaction.User.Id.ToString()).Result;
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var dailySuccess = new EmbedBuilder();
            var userXPMultiplier = (1 * (mongoUsr.Boosted ? 1.05 : 1) * (mongoUsr.Mutuals ? 1.05 : 1)); //1, 1.05, or 1.1025

            // Within last 24 hours: >(currentTime - 86400)
            // Not within last 24 hours: <(currentTime - 86400)

            // If command excuted within 24 hours since last daily, reject.
            if (mongoUsr.LastDaily > (currentTime - 86400))
            {
                var spanOfTime = TimeSpan.FromSeconds(((mongoUsr.LastDaily + 86400) - currentTime));
                string str = spanOfTime.Hours + " hour(s), " + spanOfTime.Minutes + " minute(s), and " + spanOfTime.Seconds + " second(s)!";

                var dailyRejected = new EmbedBuilder();
                dailyRejected.WithColor(0xB2A2F1);
                dailyRejected.AddField("Daily Already Claimed!", $"<@{mongoUsr.Id}>, You've already claimed your daily today!\nGet your next one in {str}");
                dailyRejected.WithThumbnailUrl(Context.User.GetAvatarUrl());

                await RespondAsync("", embed: dailyRejected.Build());
                return;
            }

            // If command executed outside of 24 hours, and within 48 hours, streak+
            else if (mongoUsr.LastDaily > (currentTime - 172800))
            {
                if (((mongoUsr.DailyStreak + 1) > 1) && ((mongoUsr.DailyStreak + 1) % 7) == 0)
                {
                    await MongoHandler.UpdateUser(mongoUsr.Id, "Money", (mongoUsr.Money + 2000 + mongoUsr.DailyBonus));
                    await MongoHandler.UpdateUser(mongoUsr.Id, "EXP", (Math.Floor(mongoUsr.EXP + (300 * userXPMultiplier))));
                    dailySuccess.AddField($"Daily Claimed: {mongoUsr.Name}!", $"You received **{500 + mongoUsr.DailyBonus}** Protobucks, and **1500** more for completing a weekly streak!\nYou now have **{mongoUsr.Money + 2000 + mongoUsr.DailyBonus}** Protobucks, and received 1 Daily Coin.\nStreak: {1 + mongoUsr.DailyStreak} *(Bonus every 7 days!)*");
                }
                else
                {
                    await MongoHandler.UpdateUser(mongoUsr.Id, "Money", (mongoUsr.Money + 500 + mongoUsr.DailyBonus));
                    await MongoHandler.UpdateUser(mongoUsr.Id, "EXP", (Math.Floor(mongoUsr.EXP + (150 * userXPMultiplier))));
                    dailySuccess.AddField($"Daily Claimed: {mongoUsr.Name}!", $"You received **{500 + mongoUsr.DailyBonus}** Protobucks!\nYou now have **{mongoUsr.Money + 500 + mongoUsr.DailyBonus}** Protobucks, and received 1 Daily Coin.\nStreak: {1 + mongoUsr.DailyStreak} *(Bonus every 7 days!)*");
                }
                await MongoHandler.UpdateUser(mongoUsr.Id, "DailyStreak", (mongoUsr.DailyStreak + 1));
            }

            // Command was executed outside of 48 hours, streak-
            else
            {
                await MongoHandler.UpdateUser(mongoUsr.Id, "Money", (mongoUsr.Money + 500 + mongoUsr.DailyBonus));
                await MongoHandler.UpdateUser(mongoUsr.Id, "EXP", (Math.Floor(mongoUsr.EXP + (75 * userXPMultiplier))));
                await MongoHandler.UpdateUser(mongoUsr.Id, "DailyStreak", 1);
                dailySuccess.AddField($"Daily Claimed: {mongoUsr.Name}!", $"You received **{500 + mongoUsr.DailyBonus}** Protobucks{(mongoUsr.DailyStreak == 0 ? "!" : $", but lost your daily streak of {mongoUsr.DailyStreak}!")}\nYou now have **{mongoUsr.Money + 500 + mongoUsr.DailyBonus}** Protobucks, and received 1 Daily Coin.\nStreak: 1 *(Bonus every 7 days!)*");
            }

            await MongoHandler.UpdateUser(mongoUsr.Id, "LastDaily", currentTime);
            await MongoHandler.UpdateInventory(mongoUsr.Id, "DailyCoins", (mongoInv.DailyCoins + 1));
            await MongoHandler.UpdateInventory(mongoUsr.Id, "DailyCoinsTotal", (mongoInv.DailyCoinsTotal + 1));
            dailySuccess.WithColor(0xB2A2F1);
            dailySuccess.WithThumbnailUrl(Context.User.GetAvatarUrl());

            await RespondAsync("", embed: dailySuccess.Build());
        }

        [SlashCommand("pat", "[profile] Give someone a pat, get a coin for it!")]
        public async Task PatRedemption([Summary(description: "A tagged user (@)")] SocketUser user)
        {
            if (user.IsBot)
            {
                await RespondAsync("Sorry, but bots cannot receive pats. :robot:");
                return;
            }

            if (user == Context.User)
            {
                await RespondAsync($"It would be a bit unfair to give yourself a pat, right? You have arms, just pat yourself on the back! :purple_heart:");
                return;
            }

            var authorUsr = MongoHandler.GetUser(Context.User.Id.ToString()).Result;
            var authorInv = MongoHandler.GetInventory(Context.User.Id.ToString()).Result;
            var taggedUsr = MongoHandler.GetUser(user.Id.ToString()).Result;

            if (taggedUsr == null)
            {
                var newUser = Context.Client.GetUserAsync(user.Id).Result;
                await MongoHandler.CreateNewUser(newUser);
                taggedUsr = MongoHandler.GetUser(user.Id.ToString()).Result;
            }

            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (authorUsr.LastPat > (currentTime - 86400))
            {
                long secsRemaining = ((authorUsr.LastPat + 86400) - currentTime);
                var spanOfTime = TimeSpan.FromSeconds(secsRemaining);
                string str = spanOfTime.Hours + " hours, " + spanOfTime.Minutes + " minutes, and " + spanOfTime.Seconds + " seconds!";

                var dailyPatRejected = new EmbedBuilder();
                dailyPatRejected.WithColor(0xB2A2F1);
                dailyPatRejected.AddField("Pat Already Claimed!", $"You've already given a pat today!\nYou can give another one in {str}");
                await RespondAsync("", embed: dailyPatRejected.Build());
                return;
            }

            //Add 1PC, 1PCT, and lastdaily to author
            await MongoHandler.UpdateUser(authorUsr.Id, "LastPat", currentTime);
            await MongoHandler.UpdateInventory(authorUsr.Id, "PatCoins", (authorInv.PatCoins + 1));
            await MongoHandler.UpdateInventory(authorUsr.Id, "PatCoinsTotal", (authorInv.PatCoinsTotal + 1));
            //Add 1 pat received to tagged user
            await MongoHandler.UpdateUser(taggedUsr.Id, "PatsReceived", (taggedUsr.PatsReceived + 1));

            var PatSuccess = new EmbedBuilder();
            PatSuccess.WithColor(0xB2A2F1);
            PatSuccess.AddField("Pat Success!", $"<@{taggedUsr.Id}> has received a pat! <3\n{authorUsr.Name} also received 1 Pat Coin!");
            await RespondAsync("", embed: PatSuccess.Build());
        }

        [SlashCommand("transfer", "[profile] Transfer your Protobucks to another user.")]
        public async Task TransferProtobucks([Summary(description: "A tagged user (@)")] SocketUser user, [Summary(description: "The amount to send")] int moneyToSend)
        {
            if (user.IsBot)
            {
                await RespondAsync("Sorry, but you cannot send Protobucks to bots. :robot:");
                return;
            }

            if (user == Context.User)
            {
                await RespondAsync($"You can't send Protobucks to yourself, silly!");
                return;
            }

            var authorUsr = MongoHandler.GetUser(Context.User.Id.ToString()).Result;
            var taggedUsr = MongoHandler.GetUser(user.Id.ToString()).Result;

            if (taggedUsr == null)
            {
                var newUser = Context.Client.GetUserAsync(user.Id).Result;
                await MongoHandler.CreateNewUser(newUser);
                taggedUsr = MongoHandler.GetUser(user.Id.ToString()).Result;
            }

            long moneySending = 0;
            try
            {
                moneySending = Convert.ToInt64(moneyToSend);
                if (moneySending <= 0)
                {
                    await RespondAsync("Negative? What? What are you trying to do here?");
                    return;
                }
                if (moneySending > authorUsr.Money)
                {
                    await RespondAsync("Baka! You don't even have that many Protobucks to send!");
                    return;
                }
            }
            catch (FormatException)
            {
                await RespondAsync("For reasons I believe are obvious, that many Protobucks cannot be sent.");
            }

            //author loses money and gains transfer out
            await MongoHandler.UpdateUser(authorUsr.Id, "Money", (authorUsr.Money - moneySending));
            await MongoHandler.UpdateUser(authorUsr.Id, "TransferOut", (authorUsr.TransferOut + moneySending));
            //recipient receives the money and gains transfer in
            await MongoHandler.UpdateUser(taggedUsr.Id, "Money", (taggedUsr.Money + moneySending));
            await MongoHandler.UpdateUser(taggedUsr.Id, "TransferIn", (taggedUsr.TransferIn + moneySending));
            await RespondAsync($">>> __**Success!** You've sent {moneySending} Protobucks to <@{taggedUsr.Id}>.__\n{Context.User.Username}: *{authorUsr.Money}* **🡮** *{authorUsr.Money - moneySending}*\n{taggedUsr.Name}: *{taggedUsr.Money}* **🡭** *{taggedUsr.Money + moneySending}*");
            return;
        }

        [SlashCommand("leaderboard", "[profile] View either the richest users or top chatters.")]
        public async Task Leaderboard([Summary(description: "The type: money or levels")][Choice("money", "money")][Choice("levels", "levels")] String message)
        {
            if (message != "money" && message != "levels")
            {
                await RespondAsync("Please specify either ``money`` or ``levels`` to view the leaderboard.");
                return;
            }

            var index = 0;
            string userList = "";
            var totalUsers = MongoHandler.GetTotalUserCount().Result.Count();
            var _embed = new EmbedBuilder();
            _embed.WithColor(0xB2A2F1);

            if (message == "money")
            {
                _embed.WithTitle("PrototonBot Protobucks Leaderboard");
                foreach (var user in MongoHandler.GetRichestUsers().Result)
                {
                    index++;
                    userList += $"\n{index} - **{user.Name}** with **{user.Money}** Protobucks.";
                }
                _embed.AddField("Richest Users", userList, true);
                _embed.AddField("Total Users", totalUsers, true);
            }

            else if (message == "levels")
            {
                _embed.WithTitle("PrototonBot Level Leaderboard");
                foreach (var user in MongoHandler.GetExperiencedUsers().Result)
                {
                    index++;
                    userList += $"\n{index} - **{user.Name}** who is level **{user.Level}**.";
                }
                _embed.AddField("Richest Users", userList, true);
                _embed.AddField("Total Users", totalUsers, true);
            }

            await RespondAsync("", embed: _embed.Build());
        }

        [SlashCommand("partnerset", "[profile] Set your partner in crime on your profile!")]
        public async Task SetPartner([Summary(description: "A tagged user (@)")] SocketUser user)
        {
            if (user.IsBot)
            {
                await RespondAsync("Sorry, but bots cannot be your partner. :robot:");
                return;
            }

            if (user.Id == Context.User.Id)
            {
                await RespondAsync("You cannot be your own partner. :detective:");
                return;
            }

            UserObject author = MongoHandler.GetUser(Context.User.Id.ToString()).Result;
            UserObject partner = MongoHandler.GetUser(user.Id.ToString()).Result;
            UserObject currentPartner;

            if (partner == null)
            {
                var newUser = Context.Client.GetUserAsync(user.Id).Result;
                await MongoHandler.CreateNewUser(newUser);
                partner = MongoHandler.GetUser(user.Id.ToString()).Result;
            }

            if (author.Partner == partner.Id)
            {
                await RespondAsync($"Hey hey! {partner.Name} is already your partner!");
                return;
            }

            if (author.Partner == "None")
            {
                await MongoHandler.UpdateUser(author.Id, "Partner", user.Id);
                if (partner.Partner == Context.User.Id.ToString())
                {
                    await MongoHandler.UpdateUser(author.Id, "Mutuals", true);
                    await MongoHandler.UpdateUser(partner.Id, "Mutuals", true);
                    await RespondAsync($"Awe, nice! You and <@{partner.Id}> are now mutual partners!");
                    return;
                }
                else
                {
                    await RespondAsync($"Awe, nice! <@{partner.Id}> is now your partner!");
                    return;
                }
            }

            else if (author.Partner != "None")
            {
                currentPartner = MongoHandler.GetUser(author.Partner).Result;
                await MongoHandler.UpdateUser(author.Id, "Partner", user.Id);


                if (partner.Partner == Context.User.Id.ToString())
                {
                    await MongoHandler.UpdateUser(author.Id, "Mutuals", true);
                    await MongoHandler.UpdateUser(partner.Id, "Mutuals", true);
                    await RespondAsync($"Awe, nice! You and <@{partner.Id}> are now mutual partners!");
                    return;
                }

                if (currentPartner.Partner == Context.User.Id.ToString())
                {
                    await MongoHandler.UpdateUser(currentPartner.Id, "Partner", "None");
                    await MongoHandler.UpdateUser(currentPartner.Id, "Mutuals", false);
                    await MongoHandler.UpdateUser(author.Id, "Mutuals", false);
                    if (partner.Partner == author.Id)
                    {
                        await RespondAsync($"Awe, you and {currentPartner.Name} are no longer partners, but <@{partner.Id}> is now your mutual partner!");
                        await MongoHandler.UpdateUser(author.Id, "Mutuals", true);
                        await MongoHandler.UpdateUser(partner.Id, "Mutuals", true);
                    }
                    else
                    {
                        await RespondAsync($"Awe, you and {currentPartner.Name} are no longer partners, but <@{partner.Id}> is now your partner!");
                    }

                    try
                    {
                        await Context.Client.GetUserAsync(Convert.ToUInt64(currentPartner.Id)).Result.SendMessageAsync($"PrototonBot reporting in!\nI wish to let you know that {author.Name} has removed you as a partner.\nThis has removed them as your partner and changed your mutual partner status.");
                    }
                    catch
                    {
                        Console.WriteLine("Unable to message a user to let them know they were unpartnered.");
                    }
                    return;
                }

                if (partner.Partner != Context.User.Id.ToString() && currentPartner.Partner != Context.User.Id.ToString())
                {
                    await RespondAsync($"Sad to see em go... {currentPartner.Name} is no longer your partner, but <@{partner.Id}> is now your partner!");
                    return;
                }
            }
        }

        [SlashCommand("partnerremove", "[profile] Removes your current partner.")]
        public async Task RemovePartner()
        {
            UserObject author = MongoHandler.GetUser(Context.User.Id.ToString()).Result;
            if (author.Partner == "None")
            {
                await RespondAsync("You don't have a partner to remove!");
                return;
            }
            UserObject partner = MongoHandler.GetUser(author.Partner).Result;
            await MongoHandler.UpdateUser(author.Id, "Partner", "None");
            await MongoHandler.UpdateUser(author.Id, "Mutuals", false);
            if (partner.Partner == author.Id)
            {
                await MongoHandler.UpdateUser(partner.Id, "Partner", "None");
                await MongoHandler.UpdateUser(partner.Id, "Mutuals", false);
                await RespondAsync($"Awe, you and {partner.Name} are no longer partners.");

                try
                {
                    await Context.Client.GetUserAsync(Convert.ToUInt64(partner.Id)).Result.SendMessageAsync($"PrototonBot reporting in!\nI wish to let you know that {author.Name} has removed you as a partner.\nThis has removed them as your partner and changed your mutual partner status.");
                }
                catch
                {
                    Console.WriteLine("Unable to message a user to let them know they were unpartnered.");
                }
            }
            else
            {
                await RespondAsync($"Sad to see em go... {partner.Name} is no longer your partner.");
            }
            return;
        }

        [SlashCommand("settheme", "[profile] Set your current profile theme to one you own.")]
        public async Task SetTheme([Summary(description: "The name of a theme you own, or 'default' to reset it.")][Choice("red", "red")][Choice("yellow", "yellow")][Choice("green", "green")][Choice("blue", "blue")][Choice("purple", "purple")][Choice("pink", "pink")][Choice("black", "black")][Choice("pumpkin", "pumpkin")][Choice("default", "default")] String theme)
        {
            UserObject mongoUsr = MongoHandler.GetUser(Context.User.Id.ToString()).Result;
            InventoryObject mongoInv = MongoHandler.GetInventory(Context.User.Id.ToString()).Result;

            if (!mongoInv.OwnedThemes.Contains(theme.ToLower()) && theme.ToLower() != "default")
            {
                var _embed = new EmbedBuilder();
                _embed.WithTitle($"Which theme?");
                _embed.WithColor(0xB2A2F1);
                _embed.AddField("You Own", $"{String.Join(", ", mongoInv.OwnedThemes)}, and default.", true);
                _embed.WithFooter("Missing themes? Buy them with /buy profiletheme!");
                await RespondAsync("", embed: _embed.Build());
                return;
            }

            switch (theme.ToLower())
            {
                case "red":
                    {
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "PickedTheme", "red");
                        break;
                    }
                case "yellow":
                    {
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "PickedTheme", "yellow");
                        break;
                    }
                case "green":
                    {
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "PickedTheme", "green");
                        break;
                    }
                case "blue":
                    {
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "PickedTheme", "blue");
                        break;
                    }
                case "purple":
                    {
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "PickedTheme", "purple");
                        break;
                    }
                case "pink":
                    {
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "PickedTheme", "pink");
                        break;
                    }
                case "black":
                    {
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "PickedTheme", "black");
                        break;
                    }
                case "pumpkin":
                    {
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "PickedTheme", "pumpkin");
                        break;
                    }
                case "default":
                    {
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "PickedTheme", "default");
                        break;
                    }
                default:
                    {
                        return;
                    }
            }

            await RespondAsync($"Sounds good, your profile theme is now set to {theme.ToLower()}!");
        }

        [SlashCommand("equipbadge", "[profile] Equip a badge to a profile slot.")]
        public async Task EquipBadge(
            [Summary(description: "The name of a badge you own, or 'remove'.")]
            [Choice("lgbtflag", "lgbtflag")]
            [Choice("aceflag", "aceflag")]
            [Choice("biflag", "biflag")]
            [Choice("demiflag", "demiflag")]
            [Choice("gayflag", "gayflag")]
            [Choice("lesbianflag", "lesbianflag")]
            [Choice("panflag", "panflag")]
            [Choice("abroflag", "abroflag")]
            [Choice("heteroflag", "heteroflag")]
            [Choice("remove", "remove")] String badge,
            [Summary(description: "The slot to change. (1-9)")]
            [Choice("1", 1)]
            [Choice("2", 2)][Choice("3", 3)]
            [Choice("4", 4)]
            [Choice("5", 5)]
            [Choice("6", 6)]
            [Choice("7", 7)]
            [Choice("8", 8)]
            [Choice("9", 9)] int slot)
        {
            UserObject mongoUsr = MongoHandler.GetUser(Context.User.Id.ToString()).Result;
            InventoryObject mongoInv = MongoHandler.GetInventory(Context.User.Id.ToString()).Result;
            bool isOverwriting = false;
            string overwrittenBadge = "";

            if (!mongoInv.OwnedBadges.Contains(badge.ToLower()) && badge.ToLower() != "remove")
            {
                var _embed = new EmbedBuilder();
                _embed.WithTitle($"Which badge?");
                _embed.WithColor(0xB2A2F1);
                _embed.AddField("You Own", $"{String.Join(", ", mongoInv.OwnedBadges)}.", true);
                _embed.WithFooter("Missing badges? Buy them with /buy badge!");
                await RespondAsync("", embed: _embed.Build());
                return;
            }

            if (slot < 1 || slot > 9)
            {
                await RespondAsync("The assigned slot must be a value between 1 and 9.");
                return;
            }

            if (badge.ToLower() == "remove")
            {
                var updatedSlots = mongoUsr.BadgeSlots;
                updatedSlots[slot - 1] = "none";
                await MongoHandler.UpdateUser(mongoUsr.Id, "BadgeSlots", updatedSlots);
                await RespondAsync($"Successful! The {badge.ToLower()} was removed from slot {slot}!");
                return;
            }

            if (mongoUsr.BadgeSlots.Contains(badge.ToLower()))
            {
                // If the user already had this badge equipped, it should be removed from where it was.
                var equippedIndex = mongoUsr.BadgeSlots.IndexOf(badge.ToLower());
                var updatedSlots = mongoUsr.BadgeSlots;
                updatedSlots[equippedIndex] = "none";
                await MongoHandler.UpdateUser(mongoUsr.Id, "BadgeSlots", updatedSlots);
                mongoUsr = MongoHandler.GetUser(Context.User.Id.ToString()).Result;
            }

            var updatedBadges = mongoUsr.BadgeSlots;
            if (updatedBadges[slot - 1] != "none")
            {
                isOverwriting = true;
                overwrittenBadge = updatedBadges[slot - 1];
            }

            updatedBadges[slot - 1] = badge.ToLower();
            await MongoHandler.UpdateUser(mongoUsr.Id, "BadgeSlots", updatedBadges);
            await RespondAsync($"Successful! The {badge.ToLower()} has been equipped to slot {slot}{(isOverwriting ? $", overwriting the {overwrittenBadge} badge" : "")}!");

        }

        [SlashCommand("profile", "[profile] View a user's full profile")]
        public async Task ProfileCommand([Summary(description: "A tagged user (@)")] SocketUser user = null)
        {
            if (user == null) user = Context.User;
            Directory.CreateDirectory(Program.CacheDir);
            var webClient = new WebClient();

            if (user.IsBot)
            {
                await RespondAsync("Sorry, but bots do not have profiles. :robot:");
                return;
            }

            UserObject mongoUsr = MongoHandler.GetUser(user.Id.ToString()).Result;
            InventoryObject mongoInv = MongoHandler.GetInventory(user.Id.ToString()).Result;
            if (mongoUsr == null || mongoInv == null)
            {
                var newUser = Context.Client.GetUserAsync(user.Id).Result;
                await MongoHandler.CreateNewUser(newUser);
                mongoUsr = MongoHandler.GetUser(user.Id.ToString()).Result;
                mongoInv = MongoHandler.GetInventory(user.Id.ToString()).Result;
            }

            // Text read settings are modified, so they must be re-initialized every time.
            var textSettings = new MagickReadSettings
            {
                BackgroundColor = MagickColors.Transparent,
                FillColor = MagickColors.White,
                Font = Path.Combine("Storage", "BarlowCondensed-Regular.ttf"),
                FontStyle = FontStyleType.Normal,
                TextGravity = Gravity.Center,
                Width = 1000,
                Height = 600,
                TextInterwordSpacing = 8D
            };

            // Fetch the user's profile photo.
            var discordUser = Context.Client.GetUserAsync(Convert.ToUInt64(user.Id)).Result;
            webClient.DownloadFile($"https://cdn.discordapp.com/avatars/{discordUser.Id}/{discordUser.AvatarId}.png?size=512", Path.Combine(Program.CacheDir, $"{user.Id}.png"));
            var userPhoto = new MagickImage(Path.Combine(Program.CacheDir, $"{user.Id}.png"), ImageReadSettings);
            userPhoto.Resize(212, 212);

            // LAYER 1: Background
            var canvas = mongoInv.PickedTheme switch
            {
                "red" => RedProfileBackground.Clone(),
                "yellow" => YellowProfileBackground.Clone(),
                "green" => GreenProfileBackground.Clone(),
                "blue" => BlueProfileBackground.Clone(),
                "purple" => PurpleProfileBackground.Clone(),
                "pink" => PinkProfileBackground.Clone(),
                "black" => BlackProfileBackground.Clone(),
                "pumpkin" => PumpkinProfileBackground.Clone(),
                _ => DefaultProfileBackground.Clone()
            };

            // Calculate User Experience for the bar
            var expForCurrent = (mongoUsr.Level == 0) ? 0 : ((20d * mongoUsr.Level) * ((31d * mongoUsr.Level) - 17d)) / 3d;
            var expForNext = ((20d * (mongoUsr.Level + 1d)) * ((31d * (mongoUsr.Level + 1d)) - 17d)) / 3d;
            var expToNext = expForNext - mongoUsr.EXP;
            var expOffset = mongoUsr.EXP - expForCurrent;
            var expPercent = expOffset / (expForNext - expForCurrent);
            if (expPercent < 0.005) expPercent = 0.005;

            // LAYER 2: Experience Bar
            var experienceBar = mongoInv.PickedTheme switch
            {
                "red" => RedExperienceBar.Clone(),
                "yellow" => YellowExperienceBar.Clone(),
                "green" => GreenExperienceBar.Clone(),
                "blue" => BlueExperienceBar.Clone(),
                "purple" => PurpleExperienceBar.Clone(),
                "pink" => PinkExperienceBar.Clone(),
                "black" => BlackExperienceBar.Clone(),
                "pumpkin" => PumpkinExperienceBar.Clone(),
                _ => DefaultExperienceBar.Clone()
            };

            experienceBar.Crop((int)(experienceBar.Width * expPercent), experienceBar.Height);
            canvas.Composite(experienceBar, 143, 244, CompositeOperator.Over);

            // LAYER 3: User Icon
            userPhoto.Composite(ProfileMask, CompositeOperator.CopyAlpha);
            if (Program.DeveloperIDs.Contains(user.Id.ToString())) userPhoto.Composite(DeveloperOverlay, CompositeOperator.Over);
            canvas.Composite(userPhoto, 20, 20, CompositeOperator.Over);

            // LAYER 4: Developer Tip & Badges
            // See above ToDo

            // LAYER 5: Username and Description
            textSettings.Width = 750;
            textSettings.Height = 110;
            textSettings.TextGravity = Gravity.Center;
            textSettings.FontPointsize = (mongoUsr.Description.Length < 121) ? 36 : 20;
            canvas.Composite(new MagickImage($"caption:{mongoUsr.Description}", textSettings), 230, 70, CompositeOperator.Over);

            textSettings.Height = 50;
            textSettings.FontPointsize = 40;
            canvas.Composite(new MagickImage($"caption:{discordUser.Username}#{discordUser.Discriminator}", textSettings), 230, 20, CompositeOperator.Over);

            // LAYER 6: Level Area
            textSettings.Width = 123;
            textSettings.Height = 47;
            textSettings.FontPointsize = 32;
            textSettings.TextGravity = Gravity.South;
            canvas.Composite(new MagickImage($"caption:Level {mongoUsr.Level}", textSettings), 10, 244, CompositeOperator.Over);

            // LAYER 7: Experience Area
            textSettings.Width = 240;
            textSettings.Height = 26;
            textSettings.FontPointsize = 26;
            textSettings.TextGravity = Gravity.East;
            canvas.Composite(new MagickImage($"caption:{(long)expToNext} XP to {mongoUsr.Level + 1}", textSettings), 741, 265, CompositeOperator.Over);

            // LAYER 8: Value Entries
            // Adjust text for left side entries.
            textSettings.TextGravity = Gravity.West;
            textSettings.Width = 450;
            textSettings.Height = 275;
            textSettings.FontPointsize = 26;
            textSettings.TextInterlineSpacing = 8;

            var leftEntries = $"caption:Protobucks: {mongoUsr.Money}\nPats Received: {mongoUsr.PatsReceived}\nDaily Streak of {mongoUsr.DailyStreak}\nDaily Bonus of +{mongoUsr.DailyBonus}\nPurchases Made: {mongoUsr.Purchases}\nPartnered with {(mongoUsr.Partner != "None" ? MongoHandler.GetUser(mongoUsr.Partner).Result.Name : "Nobody")}\n{(mongoUsr.Mutuals ? "Mutual Partner!" : "")}";
            canvas.Composite(new MagickImage(leftEntries, textSettings), 20, 305, CompositeOperator.Over);

            // Adjust text for right side entries.
            textSettings.TextGravity = Gravity.East;

            var rightEntries = $"caption:Luck Level: {mongoUsr.Luck}\nGambled {mongoUsr.Gambles} times\n{mongoUsr.GamblesWon} Wins (Total Won: {mongoUsr.GamblesNetGain})\n{mongoUsr.GamblesLost} Losses (Total Lost: {mongoUsr.GamblesNetLoss})\nReceived {mongoUsr.TransferIn} Protobucks\nGiven/Sent {mongoUsr.TransferOut} Protobucks\nCurrent XP + Money Multiplier: {Math.Round((1 * (mongoUsr.Mutuals ? 1.05 : 1) * (mongoUsr.Boosted ? 1.05 : 1)) * 100)}%";
            canvas.Composite(new MagickImage(rightEntries, textSettings), 530, 305, CompositeOperator.Over);

            // Adjust text for middle entries.
            textSettings.TextGravity = Gravity.Center;
            textSettings.Width = 240;
            var toolsOwned = mongoInv.Axes + mongoInv.Picks + mongoInv.Wrenches;
            var materialsOwned = mongoInv.Paperclips + mongoInv.Diamonds + mongoInv.Bolts + mongoInv.Logs + mongoInv.Bulbs + mongoInv.Bricks + mongoInv.Gears + mongoInv.Leaves + mongoInv.CDs;
            var middleEntries = $"caption:Daily Coins: {mongoInv.DailyCoins} / {mongoInv.DailyCoinsTotal}\nPat Coins: {mongoInv.PatCoins} / {mongoInv.PatCoinsTotal}\nGamble Coins: {mongoInv.GambleCoins} / {mongoInv.GambleCoinsTotal}\n{mongoInv.OwnedThemes.Count() + 1} Themes Owned\n{mongoInv.OwnedBadges.Count()} Badges Owned\n{toolsOwned} Tools Owned\n{materialsOwned} Materials Owned";
            canvas.Composite(new MagickImage(middleEntries, textSettings), 380, 305, CompositeOperator.Over);

            // LAYER 9: Badges
            for (var index = 0; index < 9; index++)
            {
                if (mongoUsr.BadgeSlots[index] != "none")
                {
                    var badgeToDisplay = mongoUsr.BadgeSlots[index] switch
                    {
                        "lgbtflag" => LgbtFlag.Clone(),
                        "aceflag" => AceFlag.Clone(),
                        "biflag" => BiFlag.Clone(),
                        "demiflag" => DemiFlag.Clone(),
                        "gayflag" => GayFlag.Clone(),
                        "lesbianflag" => LesbianFlag.Clone(),
                        "panflag" => PanFlag.Clone(),
                        "abroflag" => AbroFlag.Clone(),
                        "heteroflag" => HeteroFlag.Clone()
                    };
                    var badgeXPos = 332 + index * 60;
                    canvas.Composite(badgeToDisplay, badgeXPos, 180, CompositeOperator.Over);
                }
            }


            canvas.Write(Path.Combine(Program.CacheDir, $"{user.Id}_profile.png"));
            await RespondWithFileAsync(Path.Combine(Program.CacheDir, $"{user.Id}_profile.png"));
        }

        // Image read settings (and images) are never modified, unlike text settings.
        private static readonly MagickReadSettings ImageReadSettings = new MagickReadSettings
        {
            BackgroundColor = MagickColors.Transparent
        };

        // Profile Picture Mask
        private static MagickImage ProfileMask = new MagickImage(Path.Combine("Storage", "ProfileAssets", "AvatarMask.png"), ImageReadSettings);

        // Developer Overlay Image
        private static MagickImage DeveloperOverlay = new MagickImage(Path.Combine("Storage", "ProfileAssets", "DeveloperOverlay.png"), ImageReadSettings);

        // Default Theme
        private static MagickImage DefaultProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileAssets", "DefaultTheme", "BACKGROUND.png"), ImageReadSettings);
        private static MagickImage DefaultExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileAssets", "DefaultTheme", "EXPBAR.png"), ImageReadSettings);

        // Red Theme
        private static MagickImage RedProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileAssets", "RedTheme", "BACKGROUND.png"), ImageReadSettings);
        private static MagickImage RedExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileAssets", "RedTheme", "EXPBAR.png"), ImageReadSettings);

        // Yellow Theme
        private static MagickImage YellowProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileAssets", "YellowTheme", "BACKGROUND.png"), ImageReadSettings);
        private static MagickImage YellowExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileAssets", "YellowTheme", "EXPBAR.png"), ImageReadSettings);

        // Green Theme
        private static MagickImage GreenProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileAssets", "GreenTheme", "BACKGROUND.png"), ImageReadSettings);
        private static MagickImage GreenExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileAssets", "GreenTheme", "EXPBAR.png"), ImageReadSettings);

        // Blue Theme
        private static MagickImage BlueProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileAssets", "BlueTheme", "BACKGROUND.png"), ImageReadSettings);
        private static MagickImage BlueExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileAssets", "BlueTheme", "EXPBAR.png"), ImageReadSettings);

        // Purple Theme
        private static MagickImage PurpleProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileAssets", "PurpleTheme", "BACKGROUND.png"), ImageReadSettings);
        private static MagickImage PurpleExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileAssets", "PurpleTheme", "EXPBAR.png"), ImageReadSettings);

        // Pink Theme
        private static MagickImage PinkProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileAssets", "PinkTheme", "BACKGROUND.png"), ImageReadSettings);
        private static MagickImage PinkExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileAssets", "PinkTheme", "EXPBAR.png"), ImageReadSettings);

        // Black Theme
        private static MagickImage BlackProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileAssets", "BlackTheme", "BACKGROUND.png"), ImageReadSettings);
        private static MagickImage BlackExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileAssets", "BlackTheme", "EXPBAR.png"), ImageReadSettings);

        // Pumpkin Theme
        private static MagickImage PumpkinProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileAssets", "PumpkinTheme", "BACKGROUND.png"), ImageReadSettings);
        private static MagickImage PumpkinExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileAssets", "PumpkinTheme", "EXPBAR.png"), ImageReadSettings);

        // Badges!
        private static MagickImage AbroFlag = new MagickImage(Path.Combine("Storage", "ProfileAssets", "Badges", "abroflag.png"), ImageReadSettings);
        private static MagickImage AceFlag = new MagickImage(Path.Combine("Storage", "ProfileAssets", "Badges", "aceflag.png"), ImageReadSettings);
        private static MagickImage BiFlag = new MagickImage(Path.Combine("Storage", "ProfileAssets", "Badges", "biflag.png"), ImageReadSettings);
        private static MagickImage DemiFlag = new MagickImage(Path.Combine("Storage", "ProfileAssets", "Badges", "demiflag.png"), ImageReadSettings);
        private static MagickImage GayFlag = new MagickImage(Path.Combine("Storage", "ProfileAssets", "Badges", "gayflag.png"), ImageReadSettings);
        private static MagickImage HeteroFlag = new MagickImage(Path.Combine("Storage", "ProfileAssets", "Badges", "heteroflag.png"), ImageReadSettings);
        private static MagickImage LesbianFlag = new MagickImage(Path.Combine("Storage", "ProfileAssets", "Badges", "lesbianflag.png"), ImageReadSettings);
        private static MagickImage LgbtFlag = new MagickImage(Path.Combine("Storage", "ProfileAssets", "Badges", "lgbtflag.png"), ImageReadSettings);
        private static MagickImage PanFlag = new MagickImage(Path.Combine("Storage", "ProfileAssets", "Badges", "panflag.png"), ImageReadSettings);
    }
}
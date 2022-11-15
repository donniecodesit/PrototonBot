using Discord;
using Discord.Interactions;
using PrototonBot.MongoUtility;

namespace PrototonBot.Interactions
{
    public class StoreCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("store", "[store] View the PrototonBot store")]
        public async Task ItemStore(
            [Summary(description: "The page of the store that you would like to see.")]
            [Choice("0: Main Page", 0)]
            [Choice("1: User Items", 1)]
            [Choice("2: Inventory Items", 2)]
            [Choice("3: Profile Badges", 3)] int page)
        {
            if (page > 3 || page < 0)
            {
                await RespondAsync("That page number is invalid.");
                return;
            }
            var mongoSvr = MongoHandler.GetServer(Context.Guild.Id.ToString()).Result;
            var mongoUsr = MongoHandler.GetUser(Context.User.Id.ToString()).Result;
            if (page == 0)
            {
                var _embed = new EmbedBuilder();
                _embed.WithColor(0xFFAB59);
                _embed.WithTitle("PrototonBot Store Directory");
                _embed.WithThumbnailUrl(Context.Client.GetUser(Context.Client.GetApplicationInfoAsync().Result.Id).GetAvatarUrl());
                _embed.WithDescription("PrototonBot has different store pages you can view! Which one do you want?");
                _embed.AddField($"/store 1", $"View personal profile effects you can purchase.");
                _embed.AddField($"/store 2", $"View inventory items you can purchase.");
                _embed.AddField($"/store 3", $"View badges you can equip on your profile.");
                await RespondAsync("", embed: _embed.Build());
                return;
            }

            if (page == 1)
            {
                var _embed = new EmbedBuilder();
                _embed.WithColor(0xFFAB59);
                _embed.WithTitle("PrototonBot Store - Page 1");
                _embed.WithThumbnailUrl(Context.Client.GetUser(Context.Client.GetApplicationInfoAsync().Result.Id).GetAvatarUrl());
                _embed.AddField($"``/buy profiletheme THEMENAME`` - 10,000 Protobucks", $"Purchase a profile theme that you can keep and equip.");
                _embed.AddField($"``/buy item boosted`` - 30,000 Protobucks", "Get a 5% multiplier on Chat EXP and Protobucks!");
                _embed.AddField($"``/buy item dailypat`` - 3,000 Protobucks", "Restore your daily pat so that you can give another one!");
                _embed.AddField("Your Protobucks", mongoUsr.Money);
                await RespondAsync("", embed: _embed.Build());
                return;
            }
            if (page == 2)
            {
                var _embed = new EmbedBuilder();
                _embed.WithColor(0xFFAB59);
                _embed.WithTitle("PrototonBot Store - Page 2");
                _embed.WithThumbnailUrl(Context.Client.GetUser(Context.Client.GetApplicationInfoAsync().Result.Id).GetAvatarUrl());
                _embed.AddField($":axe: ``/buy item axe`` - 1000 Protobucks", $"Allows you to use ``/chopdown``!");
                _embed.AddField($":pick: ``/buy item pick`` - 1250 Protobucks", $"Allows you to use ``/mine``!");
                _embed.AddField($":wrench: ``/buy item wrench`` - 1500 Protobucks", $"Allows you to use ``/salvage``!");
                _embed.AddField("Your Protobucks", mongoUsr.Money);
                await RespondAsync("", embed: _embed.Build());
                return;
            }
            if (page == 3)
            {
                var _embed = new EmbedBuilder();
                _embed.WithColor(0xFFAB59);
                _embed.WithTitle("PrototonBot Store - Page 3");
                _embed.WithThumbnailUrl(Context.Client.GetUser(Context.Client.GetApplicationInfoAsync().Result.Id).GetAvatarUrl());
                _embed.AddField($"``/buy badge lgbtflag`` - 1000 Protobucks", $"LGBT Pride Flag");
                _embed.AddField($"``/buy badge aceflag`` - 1000 Protobucks", $"Asexual Pride Flag");
                _embed.AddField($"``/buy badge biflag`` - 1000 Protobucks", $"Bisexual Pride Flag");
                _embed.AddField($"``/buy badge demiflag`` - 1000 Protobucks", $"Demisexual Pride Flag");
                _embed.AddField($"``/buy badge gayflag`` - 1000 Protobucks", $"Homosexual Pride Flag");
                _embed.AddField($"``/buy badge lesbianflag`` - 1000 Protobucks", $"Lesbian Pride Flag");
                _embed.AddField($"``/buy badge panflag`` - 1000 Protobucks", $"Pansexual Pride Flag");
                _embed.AddField($"``/buy badge abroflag`` - 1000 Protobucks", $"Abrosexual Pride Flag");
                _embed.AddField($"``/buy badge heteroflag`` - 1000 Protobucks", $"Heterosexual Pride Flag");
                _embed.AddField("Your Protobucks", mongoUsr.Money);
                await RespondAsync("", embed: _embed.Build());
                return;
            }
        }

        [SlashCommand("upgrade", "[store] Upgrade one of your state")]
        public async Task UpgradeStore(
            [Summary(description: "Available: luck, dailybonus")]
            [Choice("luck", "luck")]
            [Choice("dailybonus", "dailybonus")] String item)
        {
            var mongoUsr = MongoHandler.GetUser(Context.User.Id.ToString()).Result;
            var mongoInv = MongoHandler.GetInventory(Context.User.Id.ToString()).Result;

            if (item.ToLower() == "luck")
            {
                if (mongoInv.DailyCoins >= 14 && mongoInv.PatCoins >= 14 && mongoInv.GambleCoins >= 50)
                {
                    if (mongoUsr.Luck >= 0 && mongoUsr.Luck <= 23)
                    {
                        await RespondAsync($"Wowwee! Look at all these coins, oh my gosh! That's amazing! Thank you for this massive heap! You must have worked really hard for these, so I'll fuse them into your soul and give you an increase of 1 on your Luck, but keep in mind it maxes out at 25.");
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "DailyCoins", (mongoInv.DailyCoins - 14));
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "PatCoins", (mongoInv.PatCoins - 14));
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "GambleCoins", (mongoInv.GambleCoins - 50));
                        await MongoHandler.UpdateUser(mongoUsr.Id, "Luck", (mongoUsr.Luck + 1));
                    }

                    else if (mongoUsr.Luck == 24)
                    {
                        await RespondAsync($"Wowwee! Look at all these coins, oh my gosh! That's amazing! Thank you for this massive heap! You must have worked really hard for these, so I'll fuse them into your soul and give you an increase of 1 on your Luck-\n***OH MY GOSH WOW YOU REACHED THE MAXIMUM LUCK STAT OF 25! CONGRATULATIONS!***");
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "DailyCoins", (mongoInv.DailyCoins - 14));
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "PatCoins", (mongoInv.PatCoins - 14));
                        await MongoHandler.UpdateInventory(mongoUsr.Id, "GambleCoins", (mongoInv.GambleCoins - 50));
                        await MongoHandler.UpdateUser(mongoUsr.Id, "Luck", (mongoUsr.Luck + 1));
                    }

                    else if (mongoUsr.Luck == 25) await Context.Channel.SendMessageAsync($"I'm super sorry, but the Luck stat caps out at 25, and.. you're already there. Try upgrading other stats now!");
                }
                else await RespondAsync($"Seems like you don't have enough coins. Here's what you have compared to what you need!\nDC: ``{mongoInv.DailyCoins}/14``, PC: ``{mongoInv.PatCoins}/14``, ``{mongoInv.GambleCoins}/50``.");
                return;
            }
            else if (item.ToLower() == "dailybonus")
            {
                if (mongoUsr.DailyBonus >= 3000)
                {
                    await RespondAsync($"My apologies, but the limit to Daily Bonus caps at 3000 Protobucks! :broken_heart:");
                    return;
                }

                if (mongoInv.DailyCoins >= 14)
                {
                    await RespondAsync($"Wowza, you really are devoted! I'll take these 14 Daily Coins from you and fuse them into your soul...\nNow you earn 100 more Protobucks on your dailies!");
                    await MongoHandler.UpdateInventory(mongoUsr.Id, "DailyCoins", (mongoInv.DailyCoins - 14));
                    await MongoHandler.UpdateUser(mongoUsr.Id, "DailyBonus", (mongoUsr.DailyBonus + 100));
                }
                else
                {
                    await RespondAsync($"Seems like you don't have enough coins. Here's what you have compared to what you need!\nDC: ``{mongoInv.DailyCoins}/14``.");
                }
                return;
            }
            else
            {
                var _embed = new EmbedBuilder();
                _embed.WithColor(0xFFAB59);
                _embed.WithTitle("PrototonBot Upgrade Shop");
                _embed.WithThumbnailUrl(Context.Client.GetUser(Context.Client.GetApplicationInfoAsync().Result.Id).GetAvatarUrl());
                _embed.WithDescription($"Hey! Welcome to the super magic Upgrade Shop! We take those amazing coins you've earned from using our services and infuse them with your profile to give you better statistics all around! Currently, we provide 2 stats. <@{mongoUsr.Id}>");
                _embed.AddField($"``/upgrade luck`` (Your stat: {mongoUsr.Luck})", $"Costs: 14 Daily Coins, 14 Pat Coins, 50 Gamble Coins.\nRaises your Luck by 1, increasing your probability at gambling. Maximum of 25.");
                _embed.AddField($"``/upgrade dailybonus`` (Your stat: {mongoUsr.DailyBonus})", $"Costs: 14 Daily Coins.\nRaises your Daily earnings by 100.");
                _embed.AddField("Your Coins", $"DC: {mongoInv.DailyCoins} | PC: {mongoInv.PatCoins} | GC: {mongoInv.GambleCoins}");
                await RespondAsync("", embed: _embed.Build());
                return;
            }
        }

        [Group("buy", "[store] Purchase an item from the PrototonBot store")]
        public class BuyCommands : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("item", "[store] Purchase a specific item")]
            public async Task BuyItem(
                [Summary(description: "Name of the item")]
                [Choice("boosted", "boosted")]
                [Choice("dailypat", "dailypat")]
                [Choice("axe", "axe")]
                [Choice("pick", "pick")]
                [Choice("wrench", "wrench")] String item)
            {
                var mongoUsr = MongoHandler.GetUser(Context.User.Id.ToString()).Result;
                var mongoInv = MongoHandler.GetInventory(Context.User.Id.ToString()).Result;

                switch (item.ToLower())
                {
                    case "boosted":
                        {
                            if (mongoUsr.Boosted) await RespondAsync("You've already purchased the Boosted ability.");
                            else if (mongoUsr.Money >= 30000)
                            {
                                await RespondAsync($"Congratulations! You're now boosted and had 30000 Protobucks taken from your bank. Enjoy!");
                                await MongoHandler.UpdateUser(mongoUsr.Id, "Money", (mongoUsr.Money - 30000));
                                await MongoHandler.UpdateUser(mongoUsr.Id, "Purchases", (mongoUsr.Purchases + 1));
                                await MongoHandler.UpdateUser(mongoUsr.Id, "Boosted", true);
                            }
                            else await RespondAsync($"Sorry, but you don't have enough Protobucks to afford this!\nYou have {mongoUsr.Money}/30000 Protobucks.");
                            break;
                        }
                    case "dailypat":
                        {
                            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            if (!(mongoUsr.LastPat > (currentTime - 86400))) await RespondAsync($"You haven't even given away your daily pat today, wouldn't that be a waste of Protobucks?");
                            else if (mongoUsr.Money >= 3000)
                            {
                                await MongoHandler.UpdateUser(mongoUsr.Id, "LastPat", 0);
                                await MongoHandler.UpdateUser(mongoUsr.Id, "Money", (mongoUsr.Money - 3000));
                                await MongoHandler.UpdateUser(mongoUsr.Id, "Purchases", (mongoUsr.Purchases + 1));
                                await RespondAsync($"Awesome! Your daily pat was reset and 3000 Protobucks taken from your bank. Enjoy!");
                            }
                            else await RespondAsync($"Sorry, but you don't have enough Protobucks to afford this!\nYou have {mongoUsr.Money}/3000 Protobucks.");
                            break;
                        }
                    case "axe":
                        {
                            if (mongoUsr.Money >= 1000)
                            {
                                await MongoHandler.UpdateInventory(mongoUsr.Id, "Axes", (mongoInv.Axes + 1));
                                if (mongoInv.AxeUses == 0) await MongoHandler.UpdateInventory(mongoUsr.Id, "AxeUses", 10);
                                await MongoHandler.UpdateUser(mongoUsr.Id, "Money", (mongoUsr.Money - 1000));
                                await MongoHandler.UpdateUser(mongoUsr.Id, "Purchases", (mongoUsr.Purchases + 1));
                                await RespondAsync($"Awesome, you've purchased an :axe: for 1000 Protobucks! You now have {mongoInv.Axes + 1} Axe{((mongoInv.Axes + 1 == 1) ? "" : "s")}!");
                            }
                            else await RespondAsync($"Sorry, but you don't have enough Protobucks to afford this!\nYou have {mongoUsr.Money}/1000 Protobucks.");
                            break;
                        }
                    case "pick":
                        {
                            if (mongoUsr.Money >= 1250)
                            {
                                await MongoHandler.UpdateInventory(mongoUsr.Id, "Picks", (mongoInv.Picks + 1));
                                if (mongoInv.PickUses == 0) await MongoHandler.UpdateInventory(mongoUsr.Id, "PickUses", 10);
                                await MongoHandler.UpdateUser(mongoUsr.Id, "Money", (mongoUsr.Money - 1250));
                                await MongoHandler.UpdateUser(mongoUsr.Id, "Purchases", (mongoUsr.Purchases + 1));
                                await RespondAsync($"Awesome, you've purchased a :pick: for 1250 Protobucks! You now have {mongoInv.Picks + 1} Pick{((mongoInv.Picks + 1 == 1) ? "" : "s")}!");
                            }
                            else await RespondAsync($"Sorry, but you don't have enough Protobucks to afford this!\nYou have {mongoUsr.Money}/1250 Protobucks.");
                            break;
                        }
                    case "wrench":
                        {
                            if (mongoUsr.Money >= 1500)
                            {
                                await MongoHandler.UpdateInventory(mongoUsr.Id, "Wrenches", (mongoInv.Wrenches + 1));
                                if (mongoInv.WrenchUses == 0) await MongoHandler.UpdateInventory(mongoUsr.Id, "WrenchUses", 10);
                                await MongoHandler.UpdateUser(mongoUsr.Id, "Money", (mongoUsr.Money - 1500));
                                await MongoHandler.UpdateUser(mongoUsr.Id, "Purchases", (mongoUsr.Purchases + 1));
                                await RespondAsync($"Awesome, you've purchased a :wrench: for 1500 Protobucks! You now have {mongoInv.Wrenches + 1} Wrench{((mongoInv.Wrenches + 1 == 1) ? "" : "es")}!");
                            }
                            else await RespondAsync($"Sorry, but you don't have enough Protobucks to afford this!\nYou have {mongoUsr.Money}/1500 Protobucks.");
                            break;
                        }
                    default:
                        {
                            await RespondAsync($"Sorry, but the item you typed doesn't exist in our store. Please check the spelling or store and try again :purple_heart:");
                            break;
                        }
                }
            }

            [SlashCommand("profiletheme", "[store] Purchase a specific profile theme")]
            public async Task BuyTheme(
                [Summary(description: "Name of the theme")]
                [Choice("red", "red")]
                [Choice("yellow", "yellow")]
                [Choice("green", "green")]
                [Choice("blue", "blue")]
                [Choice("purple", "purple")]
                [Choice("pink", "pink")]
                [Choice("black", "black")]
                [Choice("pumpkin", "pumpkin")] String theme)
            {
                var mongoUsr = MongoHandler.GetUser(Context.User.Id.ToString()).Result;
                var mongoInv = MongoHandler.GetInventory(Context.User.Id.ToString()).Result;

                if (mongoUsr.Money <= 10000) await RespondAsync($"Sorry, but you don't have enough Protobucks to afford this!\nYou have {mongoUsr.Money}/10000 Protobucks.");
                else
                {
                    var ownedThemes = mongoInv.OwnedThemes;
                    if (ownedThemes.Contains(theme.ToLower()))
                    {
                        await RespondAsync("You already own that theme!");
                        return;
                    }

                    switch (theme.ToLower())
                    {
                        case "red":
                            {
                                ownedThemes.Add("red");
                                break;
                            }
                        case "yellow":
                            {
                                ownedThemes.Add("yellow");
                                break;
                            }
                        case "green":
                            {
                                ownedThemes.Add("green");
                                break;
                            }
                        case "blue":
                            {
                                ownedThemes.Add("blue");
                                break;
                            }
                        case "purple":
                            {
                                ownedThemes.Add("purple");
                                break;
                            }
                        case "pink":
                            {
                                ownedThemes.Add("pink");
                                break;
                            }
                        case "black":
                            {
                                ownedThemes.Add("black");
                                break;
                            }
                        case "pumpkin":
                            {
                                ownedThemes.Add("pumpkin");
                                break;
                            }
                        default:
                            {
                                await RespondAsync("Which theme?\nAvailable: red, yellow, green, blue, purple, pink, black, pumpkin");
                                return;
                            }
                    }

                    await MongoHandler.UpdateInventory(mongoUsr.Id, "OwnedThemes", ownedThemes);
                    await MongoHandler.UpdateUser(mongoUsr.Id, "Money", (mongoUsr.Money - 10000));
                    await MongoHandler.UpdateUser(mongoUsr.Id, "Purchases", (mongoUsr.Purchases + 1));
                    await RespondAsync($"Awesome, you've purchased the {theme.ToLower()} theme for 10000 Protobucks! Go equip it using ``/settheme {theme.ToLower()}``");
                }
            }

            [SlashCommand("badge", "[store] Purchase a specific profile badge")]
            public async Task BuyBadge(
                [Summary(description: "Name of the badge")]
                [Choice("lgbtflag", "lgbtflag")]
                [Choice("aceflag", "aceflag")]
                [Choice("biflag", "biflag")]
                [Choice("demiflag", "demiflag")]
                [Choice("gayflag", "gayflag")]
                [Choice("lesbianflag", "lesbianflag")]
                [Choice("panflag", "panflag")]
                [Choice("abroflag", "abroflag")]
                [Choice("heteroflag", "heteroflag")] String badge)
            {
                var mongoUsr = MongoHandler.GetUser(Context.User.Id.ToString()).Result;
                var mongoInv = MongoHandler.GetInventory(Context.User.Id.ToString()).Result;

                if (mongoUsr.Money <= 1000) await RespondAsync($"Sorry, but you don't have enough Protobucks to afford this!\nYou have {mongoUsr.Money}/1000 Protobucks.");
                else
                {
                    var ownedBadges = mongoInv.OwnedBadges;
                    if (ownedBadges.Contains(badge.ToLower()))
                    {
                        await RespondAsync("You already own that badge!");
                        return;
                    }

                    switch (badge.ToLower())
                    {
                        case "lgbtflag":
                            {
                                ownedBadges.Add("lgbtflag");
                                break;
                            }
                        case "aceflag":
                            {
                                ownedBadges.Add("aceflag");
                                break;
                            }
                        case "biflag":
                            {
                                ownedBadges.Add("biflag");
                                break;
                            }
                        case "demiflag":
                            {
                                ownedBadges.Add("demiflag");
                                break;
                            }
                        case "gayflag":
                            {
                                ownedBadges.Add("gayflag");
                                break;
                            }
                        case "lesbianflag":
                            {
                                ownedBadges.Add("lesbianflag");
                                break;
                            }
                        case "panflag":
                            {
                                ownedBadges.Add("panflag");
                                break;
                            }
                        case "abroflag":
                            {
                                ownedBadges.Add("abroflag");
                                break;
                            }
                        case "heteroflag":
                            {
                                ownedBadges.Add("heteroflag");
                                break;
                            }
                        default:
                            {
                                await RespondAsync("Which badge?\nAvailable: lgbtflag, aceflag, biflag, demiflag, gayflag, lesbianflag, panflag, abroflag, heteroflag");
                                return;
                            }
                    }

                    await MongoHandler.UpdateInventory(mongoUsr.Id, "OwnedBadges", ownedBadges);
                    await MongoHandler.UpdateUser(mongoUsr.Id, "Money", (mongoUsr.Money - 1000));
                    await MongoHandler.UpdateUser(mongoUsr.Id, "Purchases", (mongoUsr.Purchases + 1));
                    await RespondAsync($"Awesome, you've purchased the {badge.ToLower()} badge for 1000 Protobucks! Go equip it using ``/equipbadge {badge.ToLower()}...``");
                }
            }
        }

        [SlashCommand("redeem", "[store] Redeem a code for a reward!")]
        public async Task RedeemCode([Summary(description: "The code you would like to redeem.")] String code)
        {
            var mongoUsr = MongoHandler.GetUser(Context.Interaction.User.Id.ToString()).Result;

            // If there are no active codes, stop. If the code was not an active one, stop. If the user has used it already, stop. Otherwise, continue.
            if (Program.ActiveRedeemCodes == null) await RespondAsync("There are currently no active codes.");
            else if (!Program.ActiveRedeemCodes.Contains(code)) await RespondAsync("That was not a valid active code.");
            else if (mongoUsr.RedeemedCodes.Contains(code)) await RespondAsync("You've already redeemed that code!");
            else
            {
                switch (code)
                {
                    case "PrototonBeta2022":
                        var usedCodes = mongoUsr.RedeemedCodes;
                        usedCodes.Add(code);
                        await MongoHandler.UpdateUser(mongoUsr.Id, "RedeemedCodes", usedCodes);
                        await MongoHandler.UpdateUser(mongoUsr.Id, "Money", mongoUsr.Money + 11000);
                        await RespondAsync("Code redeemed! You've been given: 11000 Protobucks!");
                        break;
                    default:
                        await RespondAsync("That was not a valid active code.");
                        break;
                }
            }
        }
    }
}

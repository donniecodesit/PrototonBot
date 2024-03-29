﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PrototonBot.MongoUtility;

namespace PrototonBot
{
    public class Utilities
    {
        private static Random random = new Random();

        // The user is a marked as a developer if any of these IDs match.
        public static bool IsUserDeveloper(string userId) => Program.DeveloperIDs.Contains(userId);

        // Handle giving the user money and xp for talking. Cooldown: 1 minute
        public static Task chatReward(IUser author)
        {
            var user = MongoHandler.GetUser(author.Id.ToString()).Result;
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // If the user received chat rewards within the last 60 seconds, cancel.
            if (user.LastMessage > (currentTime - 60))
                return Task.CompletedTask;

            // Update the user's Money. Multiply depending on stats.
            long moneyToSet = random.Next(1, 8);
            if (user.Mutuals && user.Boosted)
                moneyToSet = (long)Math.Round(moneyToSet * 1.1);
            else if (user.Mutuals || user.Boosted)
                moneyToSet = (long)Math.Round(moneyToSet * 1.05);
            moneyToSet += user.Money;

            // Update the user's EXP. Multiply depending on stats.
            double expToSet = random.Next(10, 21);
            if (user.Mutuals && user.Boosted)
                expToSet = Math.Round(expToSet * 1.1);
            else if (user.Mutuals || user.Boosted)
                expToSet = Math.Round(expToSet * 1.05);
            expToSet += user.EXP;

            // Update the user's data in the database.
            MongoHandler.UpdateUser(author.Id.ToString(), "Money", moneyToSet);
            MongoHandler.UpdateUser(author.Id.ToString(), "EXP", expToSet);
            MongoHandler.UpdateUser(author.Id.ToString(), "LastMessage", currentTime);
            return Task.CompletedTask;
        }

        // Handle checking if the user is ready to level up and inform them (depending on if it's enabled)
        public static async Task LevelUpdater(SocketUserMessage message)
        {
            var user = MongoHandler.GetUser(message.Author.Id.ToString()).Result;
            var server = MongoHandler
                .GetServer((message.Author as SocketGuildUser).Guild.Id.ToString())
                .Result;
            long currentLevel = (long)
                Math.Floor((170 + Math.Sqrt(28900 - (6 * 310 * -user.EXP))) / 620);

            if (currentLevel != user.Level)
            {
                // Reply with a level up message is the level has changed, and level up messages are enabled.
                if (
                    server.LevelUpMessages
                    && server.EnabledChannels.Contains(message.Channel.Id.ToString())
                )
                    await message.Channel.SendMessageAsync(
                        $"**Congratulations {message.Author.Username}, you've reached Level {currentLevel}!** :tada:"
                    );

                // Regardless of if a reply was sent, now update the user's level.
                await MongoHandler.UpdateUser(message.Author.Id.ToString(), "Level", currentLevel);
            }
            return;
        }

        // Handle checking if the user is ready to level up and inform them (depending on if it's enabled)
        public static async Task interactionLevelUpdater(SocketInteraction interaction)
        {
            var user = MongoHandler.GetUser(interaction.User.Id.ToString()).Result;
            var server = MongoHandler
                .GetServer((interaction.User as SocketGuildUser).Guild.Id.ToString())
                .Result;
            long currentLevel = (long)
                Math.Floor((170 + Math.Sqrt(28900 - (6 * 310 * -user.EXP))) / 620);

            if (currentLevel != user.Level)
            {
                // Reply with a level up message is the level has changed, and level up messages are enabled.
                if (
                    server.LevelUpMessages
                    && server.EnabledChannels.Contains(interaction.Channel.Id.ToString())
                )
                    await interaction.Channel.SendMessageAsync(
                        $"**Congratulations {interaction.User.Username}, you've reached Level {currentLevel}!** :tada:"
                    );

                // Regardless of if a reply was sent, now update the user's level.
                await MongoHandler.UpdateUser(
                    interaction.User.Id.ToString(),
                    "Level",
                    currentLevel
                );
            }
            return;
        }

        public static string profanityFilter(String input)
        {
            var inputLower = input.ToLower();
            try
            {
                return wordBlacklist.Where(s => inputLower.Contains(s)).First();
            }
            catch
            {
                return "";
            }
        }

        private static List<string> wordBlacklist = new List<string>()
        {
            "anal",
            "anus",
            "asshat",
            "asshole",
            "ballsack",
            "balls",
            "bastard",
            "bitch",
            "biatch",
            "blowjob",
            "blow job",
            "bollock",
            "bollok",
            "boner",
            "boob",
            "buttplug",
            "clitoris",
            "cock",
            "coon",
            "cunt",
            "damn",
            "dick",
            "dildo",
            "dyke",
            "fag",
            "faggot",
            "feck",
            "fellate",
            "fellatio",
            "felching",
            "fuck",
            "f u c k",
            "fudgepacker",
            "fudge packer",
            "flange",
            "Goddamn",
            "God damn",
            "jerk",
            "jizz",
            "knobend",
            "knob end",
            "labia",
            "muff",
            "nigger",
            "nigga",
            "penis",
            "piss",
            "poop",
            "prick",
            "pube",
            "pussy",
            "queer",
            "retard",
            "retarded",
            "rere",
            "scrotum",
            "sex",
            "shit",
            "s hit",
            "sh1t",
            "simp",
            "slut",
            "smegma",
            "spunk",
            "tit",
            "tosser",
            "turd",
            "twat",
            "vagina",
            "wank",
            "whore",
            "a55",
            "a55hole",
            "aeolus",
            "ahole",
            "analprobe",
            "anilingus",
            "areola",
            "areole",
            "arian",
            "aryan",
            "assbang",
            "assbanged",
            "assbangs",
            "asses",
            "assfuck",
            "assfucker",
            "assh0le",
            "assho1e",
            "ass hole",
            "assholes",
            "assmaster",
            "assmunch",
            "asswipe",
            "asswipes",
            "azazel",
            "b1tch",
            "babes",
            "bang",
            "banger",
            "barf",
            "bastards",
            "bawdy",
            "beaner",
            "beardedclam",
            "beastiality",
            "beatch",
            "beater",
            "beaver",
            "beeyotch",
            "beotch",
            "bigtits",
            "big tits",
            "bimbo",
            "bitched",
            "bitches",
            "bitchy",
            "blowjobs",
            "bollocks",
            "boners",
            "boobies",
            "boobs",
            "booby",
            "boozer",
            "boozy",
            "bosom",
            "bosomy",
            "bowel",
            "bowels",
            "bra",
            "brassiere",
            "breast",
            "breasts",
            "bukkake",
            "bullshit",
            "bull shit",
            "bullshits",
            "bullshitted",
            "bullturds",
            "bung",
            "busty",
            "butt fuck",
            "buttfuck",
            "buttfucker",
            "c.0.c.k",
            "c.o.c.k.",
            "c.u.n.t",
            "c0ck",
            "c-0-c-k",
            "cahone",
            "cameltoe",
            "carpetmuncher",
            "cawk",
            "cervix",
            "chinc",
            "chincs",
            "chink",
            "chode",
            "chodes",
            "cl1t",
            "climax",
            "clit",
            "clitorus",
            "clits",
            "clitty",
            "cocain",
            "cocaine",
            "c-o-c-k",
            "cockblock",
            "cockholster",
            "cockknocker",
            "cocks",
            "cocksmoker",
            "cocksucker",
            "cock sucker",
            "coital",
            "commie",
            "condom",
            "coons",
            "corksucker",
            "cracker",
            "crackwhore",
            "crappy",
            "cummin",
            "cumming",
            "cumshot",
            "cumshots",
            "cumslut",
            "cumstain",
            "cunilingus",
            "cunnilingus",
            "cunny",
            "c-u-n-t",
            "cuntface",
            "cunthunter",
            "cuntlick",
            "cuntlicker",
            "cunts",
            "d0ng",
            "d0uch3",
            "d0uche",
            "d1ck",
            "d1ld0",
            "d1ldo",
            "dago",
            "dagos",
            "dawgie-style",
            "dickbag",
            "dickdipper",
            "dickface",
            "dickflipper",
            "dickhead",
            "dickheads",
            "dickish",
            "dick-ish",
            "dickripper",
            "dicksipper",
            "dickweed",
            "dickwhipper",
            "dickzipper",
            "diddle",
            "dike",
            "dildos",
            "diligaf",
            "dillweed",
            "dimwit",
            "dingle",
            "dipship",
            "doggie-style",
            "doggy-style",
            "doofus",
            "doosh",
            "dopey",
            "douch3",
            "douche",
            "douchebag",
            "douchebags",
            "douchey",
            "dumass",
            "dumbass",
            "dumbasses",
            "dykes",
            "ejaculate",
            "enlargement",
            "erect",
            "erection",
            "erotic",
            "essohbee",
            "extacy",
            "extasy",
            "f.u.c.k",
            "fack",
            "fagg",
            "fagged",
            "faggit",
            "fagot",
            "fags",
            "faig",
            "faigt",
            "fannybandit",
            "fartknocker",
            "felch",
            "felcher",
            "feltch",
            "feltcher",
            "fisted",
            "fisting",
            "fisty",
            "floozy",
            "foad",
            "fondle",
            "foobar",
            "foreskin",
            "freex",
            "frigg",
            "frigga",
            "f-u-c-k",
            "fuckass",
            "fucked",
            "fucker",
            "fuckface",
            "fuckin",
            "fucking",
            "fucknugget",
            "fucknut",
            "fuckoff",
            "fucks",
            "fucktard",
            "fuck-tard",
            "fuckup",
            "fuckwad",
            "fuckwit",
            "fvck",
            "fxck",
            "gae",
            "gai",
            "ganja",
            "gey",
            "gfy",
            "ghay",
            "ghey",
            "gigolo",
            "glans",
            "goatse",
            "godamn",
            "godamnit",
            "goddam",
            "goddammit",
            "goddamn",
            "goldenshower",
            "gonad",
            "gonads",
            "gook",
            "gooks",
            "gringo",
            "gspot",
            "g-spot",
            "gtfo",
            "guido",
            "h0m0",
            "h0mo",
            "handjob",
            "hard on",
            "he11",
            "hebe",
            "heeb",
            "hemp",
            "heroin",
            "herp",
            "herpes",
            "herpy",
            "hitler",
            "hiv",
            "hobag",
            "hom0",
            "homey",
            "homoey",
            "honky",
            "hooch",
            "hookah",
            "hooker",
            "hoor",
            "hootch",
            "hooter",
            "hooters",
            "horny",
            "hump",
            "humped",
            "humping",
            "hussy",
            "hymen",
            "inbred",
            "incest",
            "injun",
            "j3rk0ff",
            "jackass",
            "jackhole",
            "jackoff",
            "jap",
            "japs",
            "jerk0ff",
            "jerked",
            "jerkoff",
            "jism",
            "jiz",
            "jizm",
            "jizzed",
            "junkie",
            "junky",
            "kike",
            "kikes",
            "kinky",
            "kkk",
            "klan",
            "kooch",
            "kooches",
            "kootch",
            "kraut",
            "kyke",
            "lech",
            "leper",
            "lesbo",
            "lesbos",
            "lezbo",
            "lezbos",
            "lezzie",
            "lezzies",
            "lezzy",
            "loin",
            "loins",
            "lube",
            "lusty",
            "massa",
            "masterbate",
            "masterbating",
            "masterbation",
            "masturbate",
            "masturbating",
            "masturbation",
            "menses",
            "menstruate",
            "menstruation",
            "meth",
            "m-fucking",
            "molest",
            "moolie",
            "moron",
            "motherfucka",
            "motherfucker",
            "motherfucking",
            "mtherfucker",
            "mthrfucker",
            "mthrfucking",
            "muffdiver",
            "murder",
            "muthafuckaz",
            "muthafucker",
            "mutherfucker",
            "mutherfucking",
            "muthrfucking",
            "naked",
            "napalm",
            "nappy",
            "nazi",
            "nazism",
            "negro",
            "niggah",
            "niggas",
            "niggaz",
            "niggers",
            "niggle",
            "niglet",
            "nimrod",
            "ninny",
            "nipple",
            "nooky",
            "nympho",
            "opiate",
            "opium",
            "oral",
            "orally",
            "organ",
            "orgasm",
            "orgasmic",
            "orgies",
            "orgy",
            "ovary",
            "ovum",
            "ovums",
            "p.u.s.s.y.",
            "paddy",
            "paki",
            "pantie",
            "panties",
            "panty",
            "pastie",
            "pasty",
            "pecker",
            "pedo",
            "pedophile",
            "pedophilia",
            "pedophiliac",
            "penetrate",
            "penetration",
            "penial",
            "penile",
            "perversion",
            "peyote",
            "phalli",
            "phallic",
            "phuck",
            "pillowbiter",
            "pimp",
            "pinko",
            "pissed",
            "pissoff",
            "piss-off",
            "polack",
            "pollock",
            "poon",
            "poontang",
            "porn",
            "porno",
            "pornography",
            "potty",
            "prig",
            "prostitute",
            "prude",
            "pubic",
            "pubis",
            "punkass",
            "punky",
            "puss",
            "pussies",
            "pussypounder",
            "puto",
            "queaf",
            "queef",
            "queero",
            "queers",
            "quicky",
            "quim",
            "racy",
            "rape",
            "raped",
            "raper",
            "rapist",
            "raunch",
            "rectal",
            "rectum",
            "rectus",
            "reefer",
            "reetard",
            "reich",
            "revue",
            "rimjob",
            "ritard",
            "rtard",
            "r-tard",
            "rump",
            "rumprammer",
            "ruski",
            "s.h.i.t.",
            "s.o.b.",
            "s0b",
            "sadism",
            "sadist",
            "scag",
            "scantily",
            "schizo",
            "schlong",
            "screw",
            "screwed",
            "scrog",
            "scrot",
            "scrote",
            "scrud",
            "scum",
            "seaman",
            "seamen",
            "seduce",
            "semen",
            "sexual",
            "s-h-1-t",
            "shamedame",
            "s-h-i-t",
            "shite",
            "shiteater",
            "shitface",
            "shithead",
            "shithole",
            "shithouse",
            "shits",
            "shitt",
            "shitted",
            "shitter",
            "shitty",
            "shiz",
            "sissy",
            "skag",
            "skank",
            "slave",
            "sleaze",
            "sleazy",
            "slutdumper",
            "slutkiss",
            "sluts",
            "smut",
            "smutty",
            "snatch",
            "sniper",
            "snuff",
            "s-o-b",
            "sodom",
            "souse",
            "soused",
            "sperm",
            "spic",
            "spick",
            "spik",
            "spiks",
            "spooge",
            "steamy",
            "stfu",
            "stiffy",
            "stoned",
            "strip",
            "stroke",
            "stupid",
            "suck",
            "sucked",
            "sucking",
            "sumofabiatch",
            "t1t",
            "tampon",
            "tard",
            "tawdry",
            "teabagging",
            "teat",
            "terd",
            "teste",
            "testee",
            "testes",
            "testicle",
            "testis",
            "thrust",
            "thug",
            "tinkle",
            "titfuck",
            "titi",
            "tits",
            "tittiefucker",
            "titties",
            "titty",
            "tittyfuck",
            "tittyfucker",
            "toke",
            "toots",
            "tramp",
            "transsexual",
            "trashy",
            "tubgirl",
            "tush",
            "twats",
            "ugly",
            "undies",
            "unwed",
            "urinal",
            "urine",
            "uterus",
            "uzi",
            "vag",
            "valium",
            "viagra",
            "virgin",
            "vixen",
            "vodka",
            "vomit",
            "voyeur",
            "vulgar",
            "vulva",
            "wad",
            "wang",
            "wanker",
            "wazoo",
            "wedgie",
            "weed",
            "weenie",
            "weewee",
            "weiner",
            "weirdo",
            "wench",
            "wetback",
            "wh0re",
            "wh0reface",
            "whitey",
            "whiz",
            "whoralicious",
            "whorealicious",
            "whored",
            "whoreface",
            "whorehopper",
            "whorehouse",
            "whores",
            "whoring",
            "wigger",
            "womb",
            "woody",
            "wop",
            "x-rated",
            "xxx",
            "yeasty",
            "yobbo",
            "zoophile"
        };
    }
}

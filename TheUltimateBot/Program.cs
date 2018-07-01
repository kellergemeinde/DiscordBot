using DSharpPlus;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using TheUltimateBot.Data;

namespace TheUltimateBot
{
    internal class Program
    {
        public static DiscordClient discord;
        public static CommandsNextExtension commands;
        private static object fileLock = new object();
        private static string logFile = "discordBot.log";

        private static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = "MzU3OTY3Njc2ODU4OTU3ODM2.DJxn1g.dDD9xW7JIQ0g_Sols0Klx6bZSOY",
                TokenType = TokenType.Bot
            });
            var commandConfig = new CommandsNextConfiguration();
            commandConfig.StringPrefixes = new List<string>() { "!!" };
            commands = discord.UseCommandsNext(commandConfig);

            discord.MessageCreated += (messageCreateEventArgs) =>
            {
                if (messageCreateEventArgs.Guild == null)
                {
                    lock (fileLock)
                    {
                        using (StreamWriter sw = new StreamWriter(logFile, true))
                        {
                            sw.WriteLine("[" + DateTime.UtcNow.ToShortTimeString() + "]" + "[MSG]" + messageCreateEventArgs.Message.Content);
                        }
                    }
                }
                return null;
            };

            discord.MessageCreated += (messageCreateEventArgs) =>
            {
                if (messageCreateEventArgs.Message.Content.ToLower().EndsWith("stop"))
                {
                    messageCreateEventArgs.Message.RespondAsync("**HAMMERTIME**");
                }
                return null;
            };

            discord.MessageCreated += async (messageCreateEventArgs) =>
            {
                Regex pr0grammLinkRegex = new Regex("pr0gramm\\.com\\/[newtopuser]+\\/[\\/a-zA-Z]*([0-9]+)");
                var msg = messageCreateEventArgs.Message.Content;
                var mention = messageCreateEventArgs.Message.Author.Mention;
                var apiUrl = @"http://pr0gramm.com/api/items/get?id=";
                var apiUrlComments = @"http://pr0gramm.com/api/items/info?itemId=";

                var matches = pr0grammLinkRegex.Matches(msg);
                if (matches.Count > 0 && !msg.Contains("\n") && !messageCreateEventArgs.Author.IsBot)
                {
                    var id = matches.FirstOrDefault().Groups[1].ToString();
                    string thumb = "";
                    string comment = "";
                    string link = "";

                    using (HttpClient client = new HttpClient())
                    using (HttpResponseMessage response = await client.GetAsync(apiUrl + id))
                    using (HttpContent content = response.Content)
                    {
                        string jsonString = await content.ReadAsStringAsync();
                        var json = JObject.Parse(jsonString);
                        JToken pr0Result;
                        json.TryGetValue("items", out pr0Result);

                        var item = pr0Result.FirstOrDefault();

                        var image = item["image"].ToString();
                        if (image.EndsWith("jpg") || image.EndsWith("png") || image.EndsWith("gif"))
                        {
                            thumb = @"http://img.pr0gramm.com/" + image;
                        }
                        else
                        {
                            thumb = @"http://thumb.pr0gramm.com/" + item["thumb"];
                        }
                        id = item["id"].ToString();
                        link = @"http://pr0gramm.com/new/" + id;
                    }

                    using (HttpClient client = new HttpClient())
                    using (HttpResponseMessage response = await client.GetAsync(apiUrlComments + id))
                    using (HttpContent content = response.Content)
                    {
                        string jsonString = await content.ReadAsStringAsync();
                        var json = JObject.Parse(jsonString);
                        JToken pr0Result;
                        json.TryGetValue("comments", out pr0Result);
                        int upvotes = 0;
                        foreach (var item in pr0Result)
                        {
                            if (int.Parse(item["up"].ToString()) > upvotes)
                            {
                                upvotes = int.Parse(item["up"].ToString());
                                comment = item["content"].ToString();
                            }
                        }
                    }

                    await messageCreateEventArgs.Message.RespondAsync(mention);
                    await messageCreateEventArgs.Message.RespondAsync(thumb);
                    await messageCreateEventArgs.Message.RespondAsync(link + "\nComment: *" + comment + "*");
                    await messageCreateEventArgs.Message.DeleteAsync();
                }
            };

            discord.MessageCreated += async (messageCreateEventArgs) =>
            {
                try
                {
                    var data = new SqliteDataConnector(messageCreateEventArgs.Guild);
                    if (data.IsInDatabase(messageCreateEventArgs.Author))
                    {
                        data.UpdateActivity(messageCreateEventArgs.Author);
                    }
                }
                catch (Exception)
                {

                }
            };

            commands.CommandExecuted += (commandExecutionEventArgs) =>
            {
                var ctx = commandExecutionEventArgs.Context;
                ctx.Message.DeleteAsync();
                return null;
            };

            commands.RegisterCommands<PublicCommands>();
            commands.RegisterCommands<Commands.Information>();
            commands.RegisterCommands<Commands.Administration>();
            commands.RegisterCommands<Commands.Pr0gramm>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }

    public class PublicCommands : BaseCommandModule
    {
        [Command("status")]
        public async Task UpdateStatus(CommandContext ctx, string status)
        {
            await Program.discord.UpdateStatusAsync(new DSharpPlus.Entities.DiscordActivity(status.Trim()));
        }
    }
}
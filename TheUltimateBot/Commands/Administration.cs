using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheUltimateBot.Data;
using TheUltimateBot.Imaging;

namespace TheUltimateBot.Commands
{
    public class Administration : BaseCommandModule
    {
        [Command("pingactive")]
        public async Task PingActive(CommandContext ctx, string message)
        {
            using (var data = new SqliteDataConnector(ctx.Guild))
            {
                foreach (var member in data.GetActive())
                {
                    await member.SendMessageAsync(message);
                }
            }
        }

        [Command("pinginactive")]
        public async Task PingInactive(CommandContext ctx, string message)
        {
            using (var data = new SqliteDataConnector(ctx.Guild))
            {
                foreach (var member in data.GetInactive())
                {
                    await member.SendMessageAsync(message);
                }
            }
        }

        [Command("pingall")]
        public async Task PingAll(CommandContext ctx, string message)
        {
            foreach (var member in ctx.Guild.Members)
            {
                await member.SendMessageAsync(message);
            }
        }

        [Command("pingall")]
        public async Task PingAll(CommandContext ctx, string message, string group)
        {
            foreach (var member in ctx.Guild.Members)
            {
                if (member.Roles.Select(x => x.Name.ToLower()).Contains(group.ToLower()))
                {
                    await member.SendMessageAsync(message);
                }
            }
        }

        [Command("reset")]
        public async Task Reset(CommandContext ctx)
        {
            using (var data = new SqliteDataConnector(ctx.Guild))
            {
                data.ResetDatabase();

                await ctx.Message.RespondAsync("Reset complete.");
            }
        }

        [Command("reset")]
        public async Task Reset(CommandContext ctx, string roleName)
        {
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == roleName.ToLower());
            using (var data = new SqliteDataConnector(ctx.Guild))
            {
                data.ResetDatabase();

                var result = data.AddRoleAsActive(ctx.Guild, role);

                await ctx.Channel.SendMessageAsync("Reset complete.");
                await ctx.Channel.SendMessageAsync("Following members set as active:");
                await ctx.Channel.SendMessageAsync(string.Join('\n', result.Select(x => x.DisplayName)));
            }
        }

        [Command("setactive")]
        public async Task SetActive(CommandContext ctx, string mention)
        {
            var user = ctx.Guild.Members.First(x => x.Mention.Equals(mention));
            using (var data = new SqliteDataConnector(ctx.Guild))
            {
                if (!data.IsInDatabase(user))
                {
                    data.SetActive(user);
                    await ctx.Message.RespondAsync(user.Mention + " added to active members");
                }
                else
                {
                    await ctx.Message.RespondAsync(user.DisplayName + " is already an active member");
                }
            }
        }

        [Command("setinactive")]
        public async Task SetInactive(CommandContext ctx, string mention)
        {
            var user = ctx.Guild.Members.First(x => x.Mention.Equals(mention));
            using (var data = new SqliteDataConnector(ctx.Guild))
            {
                data.SetInactive(user);
                await ctx.Message.RespondAsync(user.DisplayName + " is not an active member anymore");
            }
        }

        [Command("setalwaysactive")]
        public async Task SetAlwaysActive(CommandContext ctx, string mention)
        {
            var user = ctx.Guild.Members.First(x => mention.Contains(x.Id.ToString()));
            using (var data = new SqliteDataConnector(ctx.Guild))
            {
                if (!data.IsInDatabase(user))
                {
                    data.SetActive(user);
                    await ctx.Message.RespondAsync(user.Mention + " added to active members");
                }

                data.SetAlwaysActive(user, true);
            }
        }

        [Command("checkactive")]
        public async Task CheckActive(CommandContext ctx)
        {
            using (var data = new SqliteDataConnector(ctx.Guild))
            {
                var active = data.GetActive();

                await ctx.Message.RespondAsync(string.Join('\n', active.Select(x => x.Username + ", last activity " + ((data.GetLastActivity(x) > 0) ? FromUnixTimeStamp(data.GetLastActivity(x)).ToShortDateString() : " ALWAYSACTIVE"))));
            }
        }

        public DateTime FromUnixTimeStamp(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        [Command("text2img")]
        public async Task TextToImage(CommandContext ctx, string text)
        {
            FontFamily fontFamily = new FontFamily("Arial");
            Font font = new Font(
               fontFamily,
               8,
               FontStyle.Regular,
               GraphicsUnit.Point);

            var test = Drawing.DrawText(text, font, Color.Black, Color.White);
            using (var ms = new MemoryStream())
            {
                test.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                await ctx.Message.RespondWithFileAsync(ms, "TEST.png");
            }
        }

        [Command("ActiveImg")]
        public async Task ActiveMembersAsImage(CommandContext ctx)
        {
            var test = Drawing.DrawActiveMemberStructure(ctx.Guild);
            using (var ms = new MemoryStream())
            {
                test.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                await ctx.Message.RespondWithFileAsync(ms, "TEST.png");
            }
        }
    }
}
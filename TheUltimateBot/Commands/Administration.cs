using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Linq;
using System.Threading.Tasks;
using TheUltimateBot.Data;

namespace TheUltimateBot.Commands
{
    public class Administration : BaseCommandModule
    {
        [Command("pingactive")]
        public async Task PingActive(CommandContext ctx, string message)
        {
            var data = new SqliteDataConnector(ctx.Guild);
            foreach (var member in data.GetActive())
            {
                await member.SendMessageAsync(message);
            }
        }

        [Command("pinginactive")]
        public async Task PingInactive(CommandContext ctx, string message)
        {
            var data = new SqliteDataConnector(ctx.Guild);
            foreach (var member in data.GetInactive())
            {
                await member.SendMessageAsync(message);
            }
        }

        [Command("reset")]
        public async Task Reset(CommandContext ctx, string roleName)
        {
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            var data = new SqliteDataConnector(ctx.Guild);

            var result = data.AddRoleAsActive(ctx.Guild, role);

            await ctx.Channel.SendMessageAsync("Reset complete.");
            await ctx.Channel.SendMessageAsync("Following members set as active:");
            await ctx.Channel.SendMessageAsync(string.Join('\n', result.Select(x => x.DisplayName)));
        }
    }
}
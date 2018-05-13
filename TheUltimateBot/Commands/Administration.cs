using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TheUltimateBot.Commands
{
    public class Administration : BaseCommandModule
    {
        [Command("active")]
        public async Task UpdateStatus(CommandContext ctx, string status)
        {
            // TODO: add real functionality
            await Program.discord.UpdateStatusAsync(new DSharpPlus.Entities.DiscordActivity(status.Trim()));
        }
    }
}
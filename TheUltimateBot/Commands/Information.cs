using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace TheUltimateBot.Commands
{
    public class Information : BaseCommandModule
    {
        [Command("urban")]
        [Description("Posts the urbandictionary definition of a given phrase or word")]
        public async Task Urban(CommandContext ctx, string term)
        {
            var clientMention = ctx.Message.Author.Mention;
            var apiUrl = @"http://api.urbandictionary.com/v0/define?term=" + term.Trim();
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(apiUrl))
            using (HttpContent content = response.Content)
            {
                string jsonString = await content.ReadAsStringAsync();
                var json = JObject.Parse(jsonString);
                JToken urbanResult;
                json.TryGetValue("list", out urbanResult);
                if (urbanResult.HasValues)
                {
                    var defintion = urbanResult.First["definition"];
                    var example = urbanResult.First["example"];

                    await ctx.RespondAsync(clientMention + "\n**Urban**" + ctx.RawArgumentString + "\n\n**Definition:**\n" + defintion + "\n\n**Example:**\n" + example);
                }
                else
                {
                    await ctx.RespondAsync(clientMention + " Sorry no matchning entry for **" + ctx.RawArgumentString + "**");
                }
            }
        }
    }
}
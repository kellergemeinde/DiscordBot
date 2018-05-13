using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TheUltimateBot.Commands
{
    public class Pr0gramm : BaseCommandModule
    {
        [Command("pr0gramm")]
        [Description("Posts the recent **Top** post of a given category/tag")]
        public async Task GetPr0grammContent(CommandContext ctx, string tag)
        {
            var clientMention = ctx.Message.Author.Mention;
            var apiUrlTags = @"http://pr0gramm.com/api/items/get?tags=" + tag.Trim();
            var apiUrlComments = @"http://pr0gramm.com/api/items/info?itemId=";
            string thumb = string.Empty, link = string.Empty, comment = string.Empty, id = string.Empty;
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(apiUrlTags))
            using (HttpContent content = response.Content)
            {
                string jsonString = await content.ReadAsStringAsync();
                var json = JObject.Parse(jsonString);
                JToken pr0Result;
                json.TryGetValue("items", out pr0Result);
                foreach (var item in pr0Result)
                {
                    if (item["promoted"].ToString() != "0")
                    {
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
                        break;
                    }
                }
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

            await ctx.Message.RespondAsync(clientMention + "\n Result for: **" + ctx.RawArgumentString + "**");
            await ctx.Message.RespondAsync(thumb);
            await ctx.Message.RespondAsync(link + "\nComment: *" + comment + "*");
        }
    }
}
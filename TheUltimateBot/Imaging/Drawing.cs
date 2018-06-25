using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TheUltimateBot.Data;

namespace TheUltimateBot.Imaging
{
    public static class Drawing
    {
        public static Image DrawText(String text, Font font, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;
        }

        public static Image DrawActiveMemberStructure(DiscordGuild guild, string fontFamilyString = null, int offsetX = 0, int offsetY = 0)
        {
            // Organize the GuildStructure to get the Size right
            FontFamily fontFamily;
            if (fontFamilyString == null)
            {
                fontFamily = new FontFamily("Arial");
            }
            else
            {
                fontFamily = new FontFamily(fontFamilyString);
            }

            var smallFont = new Font(
                  fontFamily,
                  10,
                  FontStyle.Regular,
                  GraphicsUnit.Point);

            var bigFont = new Font(
                  fontFamily,
                  14,
                  FontStyle.Bold,
                  GraphicsUnit.Point);

            var data = new SqliteDataConnector(guild);
            var activeMembers = data.GetActive();
            var grouped = new Dictionary<DiscordRole, HashSet<DiscordMember>>();
            float neededWidth = 0;
            float neededHeight = 0;

            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            foreach (var member in activeMembers)
            {
                foreach (var role in member.Roles)
                {
                    if (!grouped.ContainsKey(role))
                    {
                        grouped.Add(role, new HashSet<DiscordMember>());
                    }
                    grouped[role].Add(member);
                }
            }

            var actualTexts = new Dictionary<DiscordRole, string>();

            foreach (var item in grouped)
            {
                actualTexts.Add(item.Key, string.Join("\n", item.Value.Select(x => x.DisplayName)));
                var roleMemberTextSize = drawing.MeasureString(actualTexts[item.Key], smallFont);
                var roleTextSize = drawing.MeasureString(item.Key.Name, bigFont);
                neededHeight += roleMemberTextSize.Height;
                neededWidth += (roleMemberTextSize.Width > roleTextSize.Width ? roleMemberTextSize.Width : roleTextSize.Width) + 5;
            }
            var roleNameTextHeight = drawing.MeasureString("X", bigFont).Height + 5; ;
            neededHeight += roleNameTextHeight;

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)neededWidth + 11, (int)neededHeight + 11);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(Color.DarkGray);

            //create a brush for the text
            Brush textBrush = new SolidBrush(Color.White);
            int currentPosition = 5;
            foreach (var item in actualTexts)
            {
                var color = item.Key.Color;
                drawing.DrawString(item.Key.Name, bigFont, new SolidBrush(Color.FromArgb(255, color.R, color.G, color.B)), currentPosition, 0);
                drawing.DrawString(item.Value, smallFont, textBrush, currentPosition, roleNameTextHeight);
                currentPosition += (int)drawing.MeasureString(actualTexts[item.Key], smallFont).Width + 5;
            }

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;
        }
    }
}
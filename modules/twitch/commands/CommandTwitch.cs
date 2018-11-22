using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Newtonsoft.Json.Linq;

namespace Agatha2
{
	internal class CommandTwitch : BotCommand
	{
        public CommandTwitch()
        {
            usage = "twitch";
            description = "Look up a Twitch streamer.";
            aliases = new List<string>(new string[] {"twitch"});
        }
        public override async Task ExecuteCommand(SocketMessage message)
        {
            ModuleTwitch twitch = (ModuleTwitch)parent;
            string[] message_contents = message.Content.Substring(1).Split(" ");
            string msg = "No user supplied for lookup.";
			if(message_contents.Length >= 2)
			{
                String streamer = message_contents[1];
                JToken jData = twitch.RetrieveUserIdFromUserName(streamer);
                if(jData != null && jData.HasValues)
                {
                    EmbedBuilder embedBuilder = new EmbedBuilder();
                    embedBuilder.ImageUrl = jData["profile_image_url"].ToString();
                    embedBuilder.Description = $"**[{jData["display_name"].ToString()}](http://twitch.tv/{streamer})**\n{jData["description"].ToString()}";
                    await message.Channel.SendMessageAsync($"{message.Author.Mention}", false, embedBuilder);
                }
                else
                {
                     await message.Channel.SendMessageAsync($"{message.Author.Mention}: No user found for '{streamer}'.");
                }
            }
            else
            {
                await twitch.PollStreamers(message);
                await message.Channel.SendMessageAsync($"{message.Author.Mention}: subscribed streamer polling complete.");
            }
        }
    }
}
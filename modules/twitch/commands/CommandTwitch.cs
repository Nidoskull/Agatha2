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
			aliases = new List<string>() {"twitch"};
		}
		public override async Task ExecuteCommand(SocketMessage message)
		{
			ModuleTwitch twitch = (ModuleTwitch)parent;
			string[] message_contents = message.Content.Substring(1).Split(" ");
			if(message_contents.Length >= 2)
			{
				String streamer = message_contents[1];
				JToken jData = twitch.RetrieveUserIdFromUserName(streamer);
				if(jData != null && jData.HasValues)
				{
					var request = (HttpWebRequest)WebRequest.Create($"https://api.twitch.tv/helix/streams?user_id={twitch.streamNametoID[streamer]}");
					request.Method = "Get";
					request.Timeout = 12000;
					request.ContentType = "application/vnd.twitchtv.v5+json";
					request.Headers.Add("Client-ID", Program.StreamAPIClientID);

					try
					{
						JToken jsonStream = null;
						using (var s = request.GetResponse().GetResponseStream())
						{
							using (var sr = new System.IO.StreamReader(s))
							{
								var jsonObject = JObject.Parse(sr.ReadToEnd());
								var tmp = jsonObject["data"];
								if(tmp.HasValues)
								{
									jsonStream = tmp[0];
								}
							}
						}
						await message.Channel.SendMessageAsync($"{message.Author.Mention}", false, twitch.MakeAuthorEmbed(jData, jsonStream));
					}
					catch(WebException e)
					{
						Console.WriteLine($"Stream error: {e}");
					}
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
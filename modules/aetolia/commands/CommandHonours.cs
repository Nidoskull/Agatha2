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
	internal class CommandHonours : BotCommand
	{
		internal CommandHonours()
		{
			usage = "honours <character>";
			description = "Shows information about an Aetolia character.";
			aliases = new List<string>() {"honours", "honors"};
		}
		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			ModuleAetolia aetolia = (ModuleAetolia)parent;
			string result = "There is no such person, I'm afraid.";
			string[] message_contents = message.Content.Substring(1).Split(" ");
			if(message_contents.Length < 2)
			{
				result = "Who do you wish to know about?";
			}
			else
			{
				HttpWebResponse aetInfo = aetolia.GetAPIResponse($"characters/{message_contents[1].ToLower()}");
				if(aetInfo != null)
				{
					var s = aetInfo.GetResponseStream();
					if(s != null)
					{
						StreamReader sr = new StreamReader(s);
						JObject ci = JObject.Parse(sr.ReadToEnd());
						result = "```\n-------------------------------------------------------------------------------";
						result = $"{result}\n{ci["fullname"]}";
						result = $"{result}\n-------------------------------------------------------------------------------";
						if(ci["class"].ToString().Equals("(none)"))
						{
							result = $"{result}\nThey are a level {ci["level"]} {ci["race"]} with no class.";
						}
						else
						{
							result = $"{result}\nThey are a level {ci["level"]} {ci["race"]} {ci["class"]}.";
						}
						if(ci["city"].ToString().Equals("(none)"))
						{
							result = $"{result}\nThey hold no citizenship.";
						}
						else
						{
							result = $"{result}\nThey are a citizen of {ci["city"]}.";
						}
						if(ci["guild"].ToString().Equals("(none)"))
						{
							result = $"{result}\nThey hold no guild membership.";
						}
						else
						{
							result = $"{result}\nThey are a member of the {ci["guild"]}.";
						}
						result = $"{result}\nThey are {ci["xp rank"].ToString().ToLower()} in experience, {ci["explore rank"].ToString().ToLower()} in exploration and {ci["combat rank"].ToString().ToLower()} in combat.";
						result = $"{result}\n-------------------------------------------------------------------------------```";
					}
				}
			}
			await message.Channel.SendMessageAsync($"{message.Author.Mention}: {result}");	
		}
	}
}
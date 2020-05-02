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

		internal string GetKeyValueSafe(string keyReply, string defaultReply, JObject ci, bool lowercase)
		{
			string reply = defaultReply;
			if(ci.ContainsKey(keyReply))
			{
				reply = ci[keyReply].ToString();
			}
			if(lowercase)
			{
				reply = reply.ToLower();
			}
			return reply;
		}
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
						Console.WriteLine($"honk2 {ci.ToString()}");
						result = "```\n---------------------------------------------------------------------";
						result = $"{result}\n{GetKeyValueSafe("fullname", "unknown", ci, false)}";
						result = $"{result}\n---------------------------------------------------------------------";
						if(GetKeyValueSafe("class", "(none)", ci, false).Equals("(none)"))
						{
							result = $"{result}\nThey are a level {GetKeyValueSafe("level", "0", ci, false)} {GetKeyValueSafe("race", "mysterious", ci, false)} with no class.";
						}
						else
						{
							result = $"{result}\nThey are a level {GetKeyValueSafe("level", "0", ci, false)} {GetKeyValueSafe("race", "mysterious", ci, false)} {GetKeyValueSafe("class", "adventurer", ci, false)}.";
						}
						if(GetKeyValueSafe("city", "(none)", ci, false).Equals("(none)"))
						{
							result = $"{result}\nThey hold no citizenship.";
						}
						else
						{
							result = $"{result}\nThey are a citizen of {GetKeyValueSafe("city", "(none)", ci, false)}.";
						}
						if(GetKeyValueSafe("guild", "(none)", ci, false).Equals("(none)"))
						{
							result = $"{result}\nThey hold no guild membership.";
						}
						else
						{
							result = $"{result}\nThey are a member of the {GetKeyValueSafe("guild", "(none)", ci, false).Equals("(none)")}.";
						}
						result = $"{result}\nThey are {GetKeyValueSafe("xp rank", "unranked", ci, true)} in experience, {GetKeyValueSafe("explore rank", "unranked", ci, true)} in exploration and {GetKeyValueSafe("combat rank", "unranked", ci, true)} in combat.";
						result = $"{result}\n---------------------------------------------------------------------```";
					}
				}
			}
			await Program.SendReply(message, result);
		}
	}
}
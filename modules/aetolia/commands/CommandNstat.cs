/*
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
	internal class CommandNstat : BotCommand
	{
		internal CommandNstat()
		{
			usage = "nstat";
			description = "Lists Aetolian news sections and post counts for use with the readnews command.";
			aliases = new List<string>() {"nstat"};
		}
		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			ModuleAetolia aetolia = (ModuleAetolia)parent;
			HttpWebResponse aetInfo = aetolia.GetAPIResponse("news");
			string result = "Unknown error.";
			if(aetInfo != null)
			{
				var s = aetInfo.GetResponseStream();
				if(s != null)
				{
					result = "```\n-- The Aetolian News --------------------------------------";
					StreamReader sr = new StreamReader(s);
					foreach(var x in JToken.Parse(sr.ReadToEnd()))
					{
						string padding = new string(' ', 49 - (x["name"].ToString().Length + x["total"].ToString().Length));
						result = $"{result}\n {x["name"]}:{padding}{x["total"]} posts.";
					}
					result = $"{result}\n-----------------------------------------------------------";
					result = $"{result}\n Read individual posts using {guild.commandPrefix}READNEWS [SECTION] [NUMBER].";
					result = $"{result}\n-----------------------------------------------------------\n```";
				}
			}

			await Program.SendReply(message, result);
		}
	}
}
*/
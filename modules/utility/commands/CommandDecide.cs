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
	internal class CommandDecide : BotCommand
	{
		private List<string> choiceSplitters = new List<string>() {"|", ","," or "};
		private List<string> endings = new List<string>() {".","!","?"};
		internal CommandDecide()
		{
			usage = "decide option 1|option 2|...|option n";
			description = "Picks between some options. Use | or , to delineate choices.";
			aliases = new List<string>() {"decide", "pick", "choose"};
		}
		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			string messageText = message.Content.Substring(message.Content.Split(" ")[0].Length);

			List<string> options = null;
			
			foreach(string sep in choiceSplitters)
			{
				options = messageText.Split(sep).OfType<string>().ToList();
				if(options.Count > 1)
				{
					break;
				}
			}

			if(options.Count <= 1)
			{
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: Give me some options to choose between.");
			}
			else
			{
				string choice = options[Program.rand.Next(options.Count)].Trim();
				choice = $"{choice.Substring(0,1).ToUpper()}{choice.Substring(1)}";
				if(!endings.Contains(choice.Substring(choice.Length)))
				{
					choice = $"{choice}.";
				}
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: {choice}");
			}
		}
	}
}
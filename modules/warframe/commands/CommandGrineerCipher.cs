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
using System.Globalization;

namespace Agatha2
{
	internal class CommandGrineerCipher : BotCommand
	{
		internal CommandGrineerCipher()
		{
			description = "Strength to the Grineer!";
			usage = "grineer [message to translate]";
			aliases = new List<string>() {"grineer"};

			// Create and store our cypher.
			Dictionary<string, List<string>> cypher = new Dictionary<string, List<string>>();
			cypher.Add("ra",  new List<string> {"ba"});
			cypher.Add("ke",  new List<string> {"be"});
			cypher.Add("bro", new List<string> {"bo"});
			cypher.Add("f",   new List<string> {"ck"});
			cypher.Add("agr", new List<string> {"ar"});
			cypher.Add("gr",  new List<string> {"cl", "wh"});
			cypher.Add("k",   new List<string> {"c"});
			cypher.Add("tr",  new List<string> {"st"});
			cypher.Add("hu",  new List<string> {"i"});
			cypher.Add("s",   new List<string> {"ll"});
			cypher.Add("kl",  new List<string> {"th"});
			cypher.Add("r",   new List<string> {"w", "d", "h"});
			Program.cyphers.Add("grineer", cypher);

		}

		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			await message.Channel.SendMessageAsync($"{message.Author.Mention}: {Program.ApplyCypher(message.Content.Substring(message.Content.Split(" ")[0].Length), "grineer")}");
		}
	}
}
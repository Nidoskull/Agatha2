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
			Program.cyphers.Add("grineer", new CypherGrineer());
		}

		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			await message.Channel.SendMessageAsync($"{message.Author.Mention}: {Program.ApplyCypher(message.Content.Substring(message.Content.Split(" ")[0].Length), "grineer")}");
		}
	}

	internal class CypherGrineer : BotCypher
	{
		public CypherGrineer()
		{
			substitution.Add("ra",  new List<string> {"ba"});
			substitution.Add("ke",  new List<string> {"be"});
			substitution.Add("bro", new List<string> {"bo"});
			substitution.Add("f",   new List<string> {"ck"});
			substitution.Add("agr", new List<string> {"ar"});
			substitution.Add("gr",  new List<string> {"cl", "wh"});
			substitution.Add("k",   new List<string> {"c"});
			substitution.Add("tr",  new List<string> {"st"});
			substitution.Add("hu",  new List<string> {"i"});
			substitution.Add("s",   new List<string> {"ll"});
			substitution.Add("kl",  new List<string> {"th"});
			substitution.Add("r",   new List<string> {"w", "d", "h"});
		}
		internal override string ApplyPreSubstitution(string incoming)
		{
			return incoming;
		}
		internal override string ApplyPostSubstitution(string incoming)
		{
			return incoming;
		}
	}
}
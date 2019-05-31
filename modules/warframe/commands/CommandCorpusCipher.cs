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
	internal class CommandCorpusCipher : BotCommand
	{

		internal CommandCorpusCipher()
		{
			description = "App kaip pke Yotkuy!";
			usage = "itkup [message to translate]";
			aliases = new List<string>() {"itkup", "corpus"};
			Program.cyphers.Add("corpus", new CypherCorpus());
		}

		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			await message.Channel.SendMessageAsync($"{message.Author.Mention}: {Program.ApplyCypher(message.Content.Substring(message.Content.Split(" ")[0].Length), "corpus")}");
		}
	}

	internal class CypherCorpus : BotCypher
	{
		public CypherCorpus()
		{
			substitution.Add("k", new List<string> {"p", "h", "x"});
			substitution.Add("p", new List<string> {"t", "d", "l"});
			substitution.Add("t", new List<string> {"b", "j", "n", "f", "r", "v"});
			substitution.Add("y", new List<string> {"c", "s"});
			substitution.Add("j", new List<string> {"g", "w"});
			substitution.Add("s", new List<string> {"m"});
			substitution.Add("r", new List<string> {"q"});
			substitution.Add("b", new List<string> {"z"});
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
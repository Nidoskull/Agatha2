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

			Dictionary<string, List<string>> cypher = new Dictionary<string, List<string>>();
			cypher.Add("k", new List<string> {"p", "h", "x"});
			cypher.Add("p", new List<string> {"t", "d", "l"});
			cypher.Add("t", new List<string> {"b", "j", "n", "f", "r", "v"});
			cypher.Add("y", new List<string> {"c", "s"});
			cypher.Add("j", new List<string> {"g", "w"});
			cypher.Add("s", new List<string> {"m"});
			cypher.Add("r", new List<string> {"q"});
			cypher.Add("b", new List<string> {"z"});
			Program.cyphers.Add("corpus", cypher);

		}

		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			await message.Channel.SendMessageAsync($"{message.Author.Mention}: {Program.ApplyCypher(message.Content.Substring(message.Content.Split(" ")[0].Length), "corpus")}");
		}
	}
}
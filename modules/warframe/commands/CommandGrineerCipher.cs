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
			Program.cyphers.Add("clem", new CypherClem());
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

	internal class CypherClem : BotCypher
	{
		internal override string ApplyPostSubstitution(string incoming)
		{
			List<string> clem = new List<string>();
			int clemCount = incoming.Length/5;
			
			bool placedFullStop = false;
			for(int i = 1;i<=clemCount;i++)
			{
				int clemChance = Program.rand.Next(100);
				string nextClem = null;
				if(clemChance <= 10)
				{
					nextClem = "clem-clem";
				}
				else if(clemChance <= 90)
				{
					nextClem = "clem";
				}
				else if(clemChance <= 98)
				{
					nextClem = "grakata";
				}
				else
				{
					nextClem = "TWO GRAKATA";
				}
				string endCharacter = null;
				int endCharChance = Program.rand.Next(100);
				if(endCharChance <= 10)
				{
					endCharacter = ".";
				}
				else if(endCharChance <= 15 || i == clemCount)
				{
					endCharacter = "!";
				}
				if(placedFullStop)
				{
					nextClem = $"{nextClem.Substring(0,1).ToUpper()}{nextClem.Substring(1)}";
					placedFullStop = false;
				}
				if(endCharacter != null)
				{
					nextClem = $"{nextClem}{endCharacter}";
					placedFullStop = true;
				}
				else if(endCharChance <= 30)
				{
					nextClem = $"{nextClem},";
				}
				clem.Add(nextClem);
			}
			string result = string.Join(" ", clem.ToArray());
			return $"{result.Substring(0,1).ToUpper()}{result.Substring(1)}";
		}
	}
}
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Agatha2
{
	internal class CommandDwink : BotCommand
	{
		internal CommandDwink()
		{
			usage = "dwink";
			description = "Whips up a wandom dwink fow youw enjoyment. :3c";
			aliases = new List<string>() {"dwink"};
			Program.cyphers.Add("owo", new CypherOwo());

		}
		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			List<string> drinkParts = new List<string>();
			ModuleBartender bartender = (ModuleBartender)parent;
			foreach(string drinkPart in bartender.validDrinkFields) 
			{
				drinkParts.Add(bartender.BartendingData[drinkPart][Program.rand.Next(bartender.BartendingData[drinkPart].Count)]);
			}
			string result = $"slings {drinkParts[0]}, containing {drinkParts[1]} {drinkParts[2]}, down the bar to {message.Author.Mention}.";
			await message.Channel.SendMessageAsync($"_{Program.ApplyCypher(result, "owo")}_");
		}
	}

	internal class CypherOwo : BotCypher
	{
		public CypherOwo()
		{
			substitution.Add("frick", new List<string> {"fuck"});
			substitution.Add("w", new List<string> {"l", "r", "qu"});
		}
		internal override string ApplyPreSubstitution(string incoming)
		{
			return incoming;
		}
		internal override string ApplyPostSubstitution(string incoming)
		{
			return $"{incoming} OwO";
		}
	}
}
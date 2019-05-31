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

			Dictionary<string, List<string>> cypher = new Dictionary<string, List<string>>();
			cypher.Add("w", new List<string> {"l", "r", "qu"});
			cypher.Add("frick", new List<string> {"fuck"});
			Program.cyphers.Add("owo", cypher);

		}
		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			List<string> drinkParts = new List<string>();
			ModuleBartender bartender = (ModuleBartender)parent;
			foreach(string drinkPart in bartender.validDrinkFields) 
			{
				drinkParts.Add(bartender.BartendingData[drinkPart][Program.rand.Next(bartender.BartendingData[drinkPart].Count)]);
			}
			string result = $"_slings {drinkParts[0]}, containing {drinkParts[1]} {drinkParts[2]}, down the bar to {message.Author.Mention}._";
			await message.Channel.SendMessageAsync($"{message.Author.Mention}: {Program.ApplyCypher(result, "owo")}");
		}
	}
}
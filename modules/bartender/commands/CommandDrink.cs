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
	internal class CommandDrink : BotCommand
	{
		internal CommandDrink()
		{
			usage = "drink | drink add [beverage|garnish|vessel] thing";
			description = "Whips up a random drink for your enjoyment.";
			aliases = new List<string>() {"drink"};
		}
		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			ModuleBartender bartender = (ModuleBartender)parent;
			string result = $"Usage: {usage}";
			string[] message_contents = message.Content.Substring(1).Split(" ");
			if(message_contents.Length == 1) 
			{
				List<string> drinkParts = new List<string>();
				foreach(string drinkPart in bartender.validDrinkFields) 
				{
					drinkParts.Add(bartender.BartendingData[drinkPart][Program.rand.Next(bartender.BartendingData[drinkPart].Count)]);
				}
				result = $"_slings {drinkParts[0]}, containing {drinkParts[1]} {drinkParts[2]}, down the bar to {message.Author.Mention}._";
			} 
			else if(message_contents.Length >= 4)
			{
				if(message_contents[1].ToLower() == "add")
				{
					string barKey = message_contents[2].ToLower();
					if(bartender.validDrinkFields.Contains(barKey))
					{
						string barText = message.Content.Substring(12 + barKey.Length);
						bartender.BartendingData[barKey].Add(barText);
						result = $"_will now stock {barText} as a {barKey}._";
						bartender.SaveBartendingData();
					}
				}
			}
			await message.Channel.SendMessageAsync(result);
		}
	}
}
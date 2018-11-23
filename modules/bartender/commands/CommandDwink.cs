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
		public CommandDwink()
		{
			usage = "dwink";
			description = "Whips up a wandom dwink fow youw enjoyment. :3c";
			aliases = new List<string>() {"dwink"};
		}
		public override async Task ExecuteCommand(SocketMessage message)
		{
			List<string> drinkParts = new List<string>();
			ModuleBartender bartender = (ModuleBartender)parent;
			foreach(string drinkPart in bartender.validDrinkFields) 
			{
				drinkParts.Add(bartender.BartendingData[drinkPart][Program.rand.Next(bartender.BartendingData[drinkPart].Count)]);
			}
			string result = $"_slings {drinkParts[0]}, containing {drinkParts[1]} {drinkParts[2]}, down the bar to {message.Author.Mention}._";
			string owotext = "";
			char lastChar = '\0';
			foreach(char c in result)
			{
				switch(c)
				{
					case 'l':
					case 'r':
						owotext += 'w';
						break;
					case 'L':
					case 'R':
						owotext += 'W';
						break;
					case 'u':
						if(lastChar == 'Q' || lastChar == 'q')
						{
							owotext += 'w';
						}
						else
						{
							owotext += c;								
						}
						break;
					case 'U':
						if(lastChar == 'Q' || lastChar == 'q')
						{
							owotext += 'W';
						}
						else
						{
							owotext += c;
						}
						break;
					default:
						owotext += c;
						break;
				}
				lastChar = c;
			}
			result = $"{owotext} :3c";			
			await message.Channel.SendMessageAsync(result);
		}
	}
}
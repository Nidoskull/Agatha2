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
	internal class CommandReplyrate : BotCommand
	{
		internal CommandReplyrate()
		{
			usage = "replyrate <#>";
			description = "Sets the percentage replyrate of the Markov module.";
			aliases = new List<string>() {"replyrate"};
		}
		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			if(Program.IsAuthorized(message.Author, guild.guildId))
			{
				ModuleMarkov markov = (ModuleMarkov)parent;
				try
				{
					int newReplyRate = Convert.ToInt32(message.Content.Substring(11));
					if(newReplyRate >= 0 && newReplyRate <= 100)
					{
						markov.markovChance = newReplyRate;
						await message.Channel.SendMessageAsync($"{message.Author.Mention}: Reply rate is now {markov.markovChance}.");
					}
					else
					{
						await message.Channel.SendMessageAsync($"{message.Author.Mention}: Enter a value between 0 and 100, insect.");					
					}
				}
				catch
				{
					await message.Channel.SendMessageAsync($"{message.Author.Mention}: Enter a value between 0 and 100, insect.");					
				}
			} 
			else
			{
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: You are not authorized, insect.");
			}
		}
	}
}
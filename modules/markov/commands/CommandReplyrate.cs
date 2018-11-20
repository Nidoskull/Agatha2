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
        public CommandReplyrate()
        {
            usage = "replyrate <#>";
            description = "Sets the percentage replyrate of the Markov module.";
            aliases = new List<string>(new string[] {"replyrate"});
        }
        public override async Task ExecuteCommand(SocketMessage message)
        {
            if(Program.IsAuthorized(message.Author))
			{
				try
				{
					int newReplyRate = Convert.ToInt32(message.Content.Substring(11));
					if(newReplyRate >= 0 && newReplyRate <= 100)
					{
						Program.MarkovChance = newReplyRate;
						await message.Channel.SendMessageAsync($"{message.Author.Mention}: Reply rate is now {Program.MarkovChance}.");
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
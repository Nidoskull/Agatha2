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
	internal class CommandSandwich : BotCommand
	{
        public CommandSandwich()
        {
            usage = "sandwich | sandwich add [bread|plate|filling|garnish] thing";
            description = "Whips up a random sandwich for your enjoyment.";
            aliases = new List<string>(new string[] {"sandwich"});
        }

        public override async Task ExecuteCommand(SocketMessage message)
		{
            ModuleBartender bartender = (ModuleBartender)parent;
			string result = $"Usage: {usage}.";
			string[] message_contents = message.Content.Substring(1).Split(" ");
			if(message_contents.Length == 1) 
			{
				result = $"_slaps {bartender.SandwichData["bread"][Program.rand.Next(bartender.SandwichData["bread"].Count)]} down on {bartender.SandwichData["plate"][Program.rand.Next(bartender.SandwichData["plate"].Count)]}, then adds {bartender.SandwichData["filling"][Program.rand.Next(bartender.SandwichData["filling"].Count)]}, {bartender.SandwichData["filling"][Program.rand.Next(bartender.SandwichData["filling"].Count)]}, and {bartender.SandwichData["filling"][Program.rand.Next(bartender.SandwichData["filling"].Count)]} before finishing with {bartender.SandwichData["garnish"][Program.rand.Next(bartender.SandwichData["garnish"].Count)]} and slinging it down the bar to {message.Author.Mention}._";
			}
			else if(message_contents.Length >= 4)
			{
				if(message_contents[1].ToLower() == "add")
				{
					string barKey = message_contents[2].ToLower();
					if(bartender.validSandwichFields.Contains(barKey))
					{
						string barText = message.Content.Substring(15 + barKey.Length);
						bartender.SandwichData[barKey].Add(barText);
						result = $"_will now stock {barText} as a {barKey}._";
						bartender.SaveSandwichData();
					}
				}
			}
			await message.Channel.SendMessageAsync(result);
		}
    }
}
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
using System.Text.RegularExpressions;

namespace Agatha2
{
	internal class CommandRoll : BotCommand
	{
		internal CommandRoll()
		{
			usage = "roll [1-100]d[1-100]<+/-[modifier]>";
			description = "Rolls dice in a 'standard' schema (d6, d20, etc).";
			aliases = new List<string>() {"roll", "dice", "d"};
		}
		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			EmbedBuilder embedBuilder = new EmbedBuilder();
			bool foundDice = false;
			foreach(Match m in Regex.Matches(message.Content.Substring(6), "(\\d*)(#*)d(\\d+)([+-]\\d+)*"))
			{
				DicePool dice = new DicePool(m);
				embedBuilder.AddField(dice.Label, dice.SummarizeStandardRoll());
				foundDice = true;
			}
			if(!foundDice)
			{
				await Program.SendReply(message, $"Dice syntax is `{guild.commandPrefix}roll [1-100]d[1-100]<+/-[modifier]>` separated by spaces or commas. Separate dice count from number of sides with `#` for individual rolls.");
			}
			else
			{
				await Program.SendReply(message, embedBuilder);
			}
		}
	}
}
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

namespace Agatha2
{
	internal class CommandAlerts : BotCommand
	{
		public CommandAlerts()
		{
			usage = "alerts";
			description = "Get a list of current alerts from Warframe.";
			aliases = new List<string>() {"alerts"};
		}

		public override async Task ExecuteCommand(SocketMessage message)
		{
			ModuleWarframe wf = (ModuleWarframe)parent;
			if(wf.alerts.Count <= 0)
			{
			await message.Channel.SendMessageAsync($"{message.Author.Mention}: There are no mission alerts available currently, Tenno.");
			}
			else
			{
				EmbedBuilder embedBuilder = new EmbedBuilder();
				foreach(KeyValuePair<string, Dictionary<string, string>> alertInfo in wf.alerts)
				{
					if(alertInfo.Value["Expires"] != "unknown")
					{
						embedBuilder.AddField($"{alertInfo.Value["Header"]} - {alertInfo.Value["Mission Type"]} ({alertInfo.Value["Faction"]})", $"{alertInfo.Value["Level"]}. Expires in {alertInfo.Value["Expires"]}.\nRewards:{alertInfo.Value["Rewards"]}");
					}
				}
				await message.Channel.SendMessageAsync($"{message.Author.Mention}:", false, embedBuilder);
			}
		}
	}
}
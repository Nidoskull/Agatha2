using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agatha2
{
	internal class CommandGuild : BotCommand
	{
		internal CommandGuild()
		{
			usage = "guild";
			description = "Configures guild settings.";
			aliases = new List<string>() {"guild", "g"};
		}
		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			await message.Channel.SendMessageAsync("Sorry, not implemented.");
			// Add guild config
			// Add guild role registration
		}
	}
}
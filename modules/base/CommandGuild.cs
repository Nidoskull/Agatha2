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
			aliases = new List<string>() {"guild", "g", "guildconfig", "gconf"};
		}
		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{

			if(Program.IsAuthorized(message.Author, guild.guildId))
			{
				string returnMsg = "";
				string[] message_contents = message.Content.Substring(1).Split(" ");
				if(message_contents.Length <= 1)
				{
					await Program.SendReply(message, guild.GetConfigSettings());
				}
				else
				{
					returnMsg = guild.SetConfig(message_contents);
				}
				if(returnMsg != "" && returnMsg != null)
				{
					await Program.SendReply(message, returnMsg);
				}
			}
			else
			{
				await Program.SendReply(message, "You are not authorized to modify guild configuration, insect.");
				return;
			}
			return;
		}
	}
}
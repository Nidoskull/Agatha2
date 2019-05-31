using System;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agatha2
{
	internal class CommandPoke : BotCommand
	{
		internal CommandPoke()
		{
			usage = "poke";
			description = "Forces a module to call its periodic event, if any.";
			aliases = new List<string>() {"poke"};
		}
		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{

			EmbedBuilder embedBuilder = new EmbedBuilder();
			if(!Program.IsAuthorized(message.Author, guild.GuildId))
			{
				embedBuilder.Title = "Unauthorized.";
				embedBuilder.Description = "You are not authorized to use this command.";
			}
			else
			{
				SocketGuildChannel guildChannel = message.Channel as SocketGuildChannel;
				embedBuilder.Title = "Unknown module.";
				embedBuilder.Description = "Module not found.";
				string[] message_contents = message.Content.Substring(1).Split(" ");

				if(message_contents.Length >= 2)
				{
					string checkModuleName = message_contents[1].ToLower();
					foreach(BotModule module in Program.modules)
					{
						if(module.moduleName.ToLower().Equals(checkModuleName))
						{
							List<string> result = new List<string>();
							embedBuilder.Title = module.moduleName;
							if(module.hasPeriodicEventInSeconds > 0)
							{
								if(guild.enabledModules.Contains(module.moduleName))
								{
									embedBuilder.Description = $"Poking {module.moduleName} (enabled for this guild)";
								}
								else
								{
									embedBuilder.Description = $"Poking {module.moduleName} (disabled for this guild)";
								}
								module.DoPeriodicEvent();
							}
							else
							{
								embedBuilder.Description = $"Module {module.moduleName} has no periodic event.";
							}
							break;
						}
					}
				}
			}
			await Program.SendReply(message, embedBuilder);
		}
	}
}
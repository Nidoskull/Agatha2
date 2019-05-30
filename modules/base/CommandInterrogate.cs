using System;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agatha2
{
	internal class CommandInterrogate : BotCommand
	{
		internal CommandInterrogate()
		{
			usage = "interrogate";
			description = "Interrogates a module for operating information.";
			aliases = new List<string>() {"interrogate", "int"};
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
							if(guild.enabledModules.Contains(module.moduleName))
							{
								result.Add($"{module.moduleName} (enabled for this guild)");
							}
							else
							{
								embedBuilder.Title = $"{module.moduleName} (disabled for this guild)";

							}
							embedBuilder.Description = $"{string.Join("\n", result.ToArray())}\n{module.Interrogate()}";
							break;
						}
					}
				}
			}
			await message.Channel.SendMessageAsync($"{message.Author.Mention}:", false, embedBuilder.Build());
		}
	}
}
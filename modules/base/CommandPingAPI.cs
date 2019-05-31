using System;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agatha2
{
	internal class CommandPingAPI : BotCommand
	{
		internal CommandPingAPI()
		{
			usage = "pingapi";
			description = "Checks if a module with an API callout is functioning.";
			aliases = new List<string>() {"pingapi"};
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
								result.Add($"{module.moduleName} (disabled for this guild)");

							}
							if(message_contents.Length >= 3)
							{
								result.Add(module.PingAPI(message_contents[2]));
							}
							else
							{
								result.Add(module.PingAPI());
							}
							if(result.Count > 0)
							{
								string resultStr = string.Join("\n", result.ToArray());
								if(resultStr.Length > 2048)
								{
									resultStr = $"{resultStr.Substring(0,2000)}\n... [truncated]```";
								}
								embedBuilder.Description = $"{resultStr}";
							}
							else
							{
								embedBuilder.Description = $"No ping result, somehow.";
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
using System;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agatha2
{
	internal class CommandModules : BotCommand
	{
		internal CommandModules()
		{
			usage = "modules";
			description = "Lists all registered modules.";
			aliases = new List<string>() {"modules", "module"};
		}
		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			SocketGuildChannel guildChannel = message.Channel as SocketGuildChannel;
			EmbedBuilder embedBuilder = new EmbedBuilder();
			string result = $"{Program.modules.Count} modules loaded.";
			string[] message_contents = message.Content.Substring(1).Split(" ");
			if(message_contents.Length < 2)
			{
				string moduleList = "";
				foreach(BotModule module in Program.modules)
				{
					string tmpModName = module.moduleName.ToString();
					if(guild.enabledModules.Contains(tmpModName))
					{
						moduleList = $"{moduleList}{tmpModName}\n";
					}
					else
					{
						moduleList = $"{moduleList}~~{tmpModName}~~ (disabled for this guild)\n";
					}
				}
				embedBuilder.AddField("Modules", moduleList);
			}
			else 
			{
				string checkModuleName = message_contents[1].ToLower();
				BotModule foundModule = null;
				foreach(BotModule module in Program.modules)
				{
					if(module.moduleName.ToLower().Equals(checkModuleName))
					{
						foundModule = module;
						break;
					}
				}

				if(foundModule == null)
				{
					embedBuilder.Title = "Unknown module.";
					embedBuilder.Description = "Module not found.";
				} 
				else if(message_contents.Length >= 3 && (message_contents[2].ToLower().Equals("enable") || message_contents[2].ToLower().Equals("disable")))
				{
					embedBuilder.Title = foundModule.moduleName;					
					if(!Program.IsAuthorized(message.Author, guildChannel.Guild.Id))
					{
						embedBuilder.Description = "This command can only be used by a bot admin, sorry.";
					}
					else
					{
						bool enableModule = message_contents[2].ToLower().Equals("enable");
						if(enableModule)
						{
							if(Program.EnableModuleForGuild(foundModule, guildChannel.Guild.Id))
							{
								embedBuilder.Description = $"Enabled module {foundModule.moduleName} for this guild.";
							}
							else
							{
								embedBuilder.Description = $"Module {foundModule.moduleName} is already enabled for this guild.";
							}
						}
						else
						{
							if(Program.DisableModuleForGuild(foundModule, guildChannel.Guild.Id))
							{
								embedBuilder.Description = $"Disabled module {foundModule.moduleName} for this guild.";
							}
							else
							{
								embedBuilder.Description = $"Module {foundModule.moduleName} is already disabled for this guild.";
							}
						}
					}
				}
				else 
				{
					if(guild.enabledModules.Contains(foundModule.moduleName))
					{
						embedBuilder.Title = $"{foundModule.moduleName} (enabled for this guild)";
					}
					else
					{
						embedBuilder.Title = $"{foundModule.moduleName} (disabled for this guild)";

					}
					embedBuilder.Description = foundModule.description;
					string cmds = "";
					foreach(BotCommand command in Program.commands)
					{
						if(command.parent == foundModule)
						{
							cmds = $"{cmds}{command.aliases[0].ToString()}\n";
						}
					}
					if(!cmds.Equals(""))
					{
						embedBuilder.AddField("Commands", cmds);
					}
				}
			}
			await message.Channel.SendMessageAsync($"{message.Author.Mention}:", false, embedBuilder);
		}
	}
}
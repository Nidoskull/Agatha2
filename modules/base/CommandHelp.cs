using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agatha2
{
	internal class CommandHelp : BotCommand
	{
		public CommandHelp()
		{
			usage = "help";
			description = "Lists all registered commands along with usage.";
			aliases = new List<string>() {"help", "commands"};
		}
		public override async Task ExecuteCommand(SocketMessage message)
		{
			EmbedBuilder embedBuilder = new EmbedBuilder();
			string[] message_contents = message.Content.Substring(1).Split(" ");
			string result = "";
			if(message_contents.Length < 2)
			{
				embedBuilder.Title = $"{Program.commands.Count} commands registered.";
				foreach(BotCommand command in Program.commands)
				{
					string cmdName = command.aliases[0].ToString();
					result = $"{result}\n{cmdName} [{string.Join(", ", command.aliases.ToArray())}]";
				}
				result = $"{result}\n\n**Use {Program.CommandPrefix}help [command] for more information on usage.**";
			}
			else 
			{
				string checkCommandName = message_contents[1].ToLower();
				bool foundCmd = false;
				foreach(BotCommand command in Program.commands)
				{
					if(command.aliases.Contains(checkCommandName))
					{   
						foundCmd = true;
						string cmdName = command.aliases[0].ToString();
						embedBuilder.Title = $"Helpfile for `{cmdName}` ({command.parent.moduleName} module).";
						embedBuilder.AddField("Aliases", $"`{string.Join("`, `", command.aliases.ToArray())}`");
						embedBuilder.AddField("Usage", $"`{Program.CommandPrefix}{command.usage}`");
						embedBuilder.AddField("Description", command.description.ToString());
						break;
					}
				}
				if(!foundCmd)
				{
					embedBuilder.Title = $"Helpfile not found.";
					result = "Help for that command was not found.";
				}
			}

			embedBuilder.Description = result;
			await message.Channel.SendMessageAsync($"{message.Author.Mention}:", false, embedBuilder);
		}
	}
}
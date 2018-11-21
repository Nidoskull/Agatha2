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
            aliases = new List<string>(new string[] {"help", "commands"});
        }
        public override async Task ExecuteCommand(SocketMessage message)
        {

			string[] message_contents = message.Content.Substring(1).Split(" ");
            string result = $"{Program.commands.Count} commands registered:";
			if(message_contents.Length < 2)
            {
                foreach(BotCommand command in Program.commands)
                {
                    string cmdName = command.aliases[0].ToString();
                    result = $"{result}\n {cmdName} {new String(' ', 12 - cmdName.Length)} [{string.Join(", ", command.aliases.ToArray())}]";
                }
                result = $"```{result}\nUse {Program.CommandPrefix}help [command] for more information on usage.```";
            }
            else 
            {
                result = "Help for that command was not found.";
                string checkCommandName = message_contents[1].ToLower();
                foreach(BotCommand command in Program.commands)
                {
                    if(command.aliases.Contains(checkCommandName))
                    {   
                        string cmdName = command.aliases[0].ToString();
                        result = $"```\n{cmdName} {new String(' ', 13 - cmdName.Length)} [{string.Join(", ", command.aliases.ToArray())}] ({Program.CommandPrefix}{command.usage.ToString()}):\n{command.description.ToString()}\n```";
                        break;
                    }
                }
            }
            await message.Channel.SendMessageAsync($"{message.Author.Mention}: {result}");
        }
    }
}
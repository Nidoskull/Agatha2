using Discord.WebSocket;
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
            string result = $"{Program.commands.Count} commands registered:";
            foreach(BotCommand command in Program.commands)
            {
                result = $"{result}\n\n   {command.aliases[0].ToString()} [{string.Join(", ", command.aliases.ToArray())}] ({Program.CommandPrefix}{command.usage.ToString()}):\n      {command.description.ToString()}";
            }
            await message.Channel.SendMessageAsync($"{message.Author.Mention}: ```{result}```");
        }
    }
}
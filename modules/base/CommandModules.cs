using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agatha2
{
	internal class CommandModules : BotCommand
	{
        public CommandModules()
        {
            usage = "modules";
            description = "Lists all registered modules.";
            aliases = new List<string>(new string[] {"modules"});
        }
        public override async Task ExecuteCommand(SocketMessage message)
        {
            string result = $"{Program.modules.Count} modules loaded:";
            foreach(BotModule module in Program.modules)
            {
                result = $"{result}\n\n   {module.moduleName.ToString()}:\n      {module.description.ToString()}";
            }
            await message.Channel.SendMessageAsync($"{message.Author.Mention}: ```{result}```");
        }
    }
}
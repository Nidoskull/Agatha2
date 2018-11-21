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
            aliases = new List<string>(new string[] {"modules", "module"});
        }
        public override async Task ExecuteCommand(SocketMessage message)
        {
            string result = $"{Program.modules.Count} modules loaded:";
    		string[] message_contents = message.Content.Substring(1).Split(" ");
			if(message_contents.Length < 2)
            {
                foreach(BotModule module in Program.modules)
                {
                    string tmpModName = module.moduleName.ToString();
                    result = $"{result}\n {tmpModName}";
                }
                result = $"```{result}\nUse {Program.CommandPrefix}module [module] for more information.```";
            }
            else 
            {
                result = "Module not found.";
                string checkModuleName = message_contents[1].ToLower();
                foreach(BotModule module in Program.modules)
                {
                    if(module.moduleName.ToLower().Equals(checkModuleName))
                    {   
                        result = $"--- {module.moduleName} ---\n{module.description}\n\nCommands:";
                        foreach(BotCommand command in Program.commands)
                        {
                            if(command.parent == module)
                            {
                                result = $"{result}\n {command.aliases[0].ToString()}";
                            }
                        }
                        result = $"```{result}```";
                        break;
                    }
                }
            }
            await message.Channel.SendMessageAsync($"{message.Author.Mention}: {result}");
        }
    }
}
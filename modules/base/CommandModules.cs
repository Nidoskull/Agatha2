using Discord;
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
            EmbedBuilder embedBuilder = new EmbedBuilder();
            string result = $"{Program.modules.Count} modules loaded.";
    		string[] message_contents = message.Content.Substring(1).Split(" ");
			if(message_contents.Length < 2)
            {
                string moduleList = "";
                foreach(BotModule module in Program.modules)
                {
                    string tmpModName = module.moduleName.ToString();
                    moduleList = $"{moduleList}{tmpModName}\n";
                }
                embedBuilder.AddField("Modules", moduleList);
            }
            else 
            {
                result = "Module not found.";
                string checkModuleName = message_contents[1].ToLower();
                foreach(BotModule module in Program.modules)
                {
                    if(module.moduleName.ToLower().Equals(checkModuleName))
                    {   
                        embedBuilder.Title = module.moduleName;
                        result = module.description;
                        string cmds = "";
                        foreach(BotCommand command in Program.commands)
                        {
                            if(command.parent == module)
                            {
                                cmds = $"{cmds}{command.aliases[0].ToString()}\n";
                            }
                        }
                        if(!cmds.Equals(""))
                        {
                            embedBuilder.AddField("Commands", cmds);
                        }
                        break;
                    }
                }
            }
            embedBuilder.Description = result;
            await message.Channel.SendMessageAsync($"{message.Author.Mention}:", false, embedBuilder);
        }
    }
}
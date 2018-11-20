using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agatha2
{
	internal class CommandShutup : BotCommand
	{
        public CommandShutup()
        {
            usage = "shutup";
            description = "Disallows the bot from responding to chat.";
            aliases = new List<string>(new string[] {"shutup"});
        }
        public override async Task ExecuteCommand(SocketMessage message)
        {
			if(Program.Awake)
			{
				Program.Awake = false;
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: I will be silent for now.");
			} 
			else
			{
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: I am already being silent.");
			}
        }
    }
}
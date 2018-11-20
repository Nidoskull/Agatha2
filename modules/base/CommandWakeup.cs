using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agatha2
{
	internal class CommandWakeup : BotCommand
	{
        public CommandWakeup()
        {
            usage = "wakeup";
            description = "Allows the bot to respond to chat.";
            aliases = new List<string>(new string[] {"wakeup"});
        }
        public override async Task ExecuteCommand(SocketMessage message)
        {
			if(Program.Awake)
			{
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: I am already awake.");
			} 
			else
			{
				Program.Awake = true;
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: Good morning, insect.");
			}
        }
    }
}
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Agatha2
{
	internal class CommandIcebreaker : BotCommand
	{
        private List<string> comments;
		internal CommandIcebreaker()
		{
			usage = "icebreaker";
			description = "Picks a random icebreaker topic.";
			aliases = new List<string>() {"icebreaker"};
            comments = new List<string>(File.ReadAllLines(@"modules/utility/data/icebreakers.txt"));

		}
		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			await Program.SendReply(message, comments[Program.rand.Next(comments.Count)]);
		}
	}
}
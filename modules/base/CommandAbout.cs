using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Agatha2
{
	internal class CommandAbout : BotCommand
	{
        public CommandAbout()
        {
            usage = "about";
            description = "Shows some bot information.";
            aliases = new List<string>(new string[] {"about"});
        }
        public override async Task ExecuteCommand(SocketMessage message)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.Title = $"I am a Discord bot written in C# by {Program.SourceAuthor}.";
            embedBuilder.AddField("Version", Program.SourceVersion);
            embedBuilder.AddField("Source", $"[GitHub repository]({Program.SourceLocation}).");
			embedBuilder.Description = $"Use {Program.CommandPrefix}help to view usage information.";
            await message.Channel.SendMessageAsync($"{message.Author.Mention}:", false, embedBuilder);
        }
    }
}
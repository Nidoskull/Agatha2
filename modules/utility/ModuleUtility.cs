using Discord;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using System.Reactive.Linq;

namespace Agatha2
{
	internal class ModuleUtility : BotModule
	{
		internal ModuleUtility()
		{
			moduleName = "Utility";
			description = "Miscellaneous utility commands.";
		}
		internal override bool Register(List<BotCommand> commands)
		{
			commands.Add(new CommandDecide());
			return true;
		}
		internal override void ListenTo(SocketMessage message, GuildConfig guild)
		{
			if(message.Content.Length < 200 && message.Content.Substring(message.Content.Length) == "?")
			{
				int firstOr = message.Content.FirstIndexOf(" or ");
				int lastOr = message.Content.LastIndexOf(" or ");
				if(firstOr > 0 && firstOr == lastOr)
				{
					message.AddReactionAsync(new Emoji("←"));
					message.AddReactionAsync(new Emoji("→"));
				}
			}
		}
	}
}
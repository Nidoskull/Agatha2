using Discord;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using System.Reactive.Linq;
using System.Diagnostics;

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
			if(message.Content.Length < guild.adHocPollCharacterLimit && 
			 message.Content.ToLower().IndexOf("vote") >= 0 && 
			 message.Content.Substring(message.Content.Length-1) == "?")
			{
				int firstOr = message.Content.IndexOf(" or ");
				int lastOr = message.Content.LastIndexOf(" or ");
				if(firstOr > 0 && firstOr == lastOr)
				{
					SocketUserMessage msg = (SocketUserMessage)message;
					msg.AddReactionAsync(new Emoji("⬅"));
					msg.AddReactionAsync(new Emoji("➡"));
				}
			}
		}
	}
}
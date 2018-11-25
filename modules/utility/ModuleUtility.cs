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
		public ModuleUtility()
		{
			moduleName = "Utility";
			description = "Miscellaneous utility commands.";
		}
		public override bool Register(List<BotCommand> commands)
		{
			commands.Add(new CommandDecide());
			return true;
		}
	}
}
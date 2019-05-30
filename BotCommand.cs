using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Nett;
using System.Linq;
using Discord.Rest;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;

namespace Agatha2
{
	internal abstract class BotCommand
	{
		internal List<string> aliases;
		internal string usage;
		internal string description;
		internal BotModule parent;

		internal void Register(BotModule _parent)
		{
			parent = _parent;
		}
		internal abstract Task ExecuteCommand(SocketMessage message, GuildConfig guild);
	}
}
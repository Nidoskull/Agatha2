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
	internal abstract class BotModule
	{
		internal string moduleName;
		internal string description;
		internal int hasPeriodicEventInSeconds = -1;
		internal abstract bool Register(List<BotCommand> commands);
		internal virtual void ListenTo(SocketMessage message, GuildConfig guild) {}
		internal virtual void StartModule() {}
		internal virtual void ReactionAdded(SocketGuild guild, SocketReaction reaction) {}
		internal virtual void ReactionRemoved(SocketGuild guild, SocketReaction reaction) {}
		internal virtual void LoadConfig() {}
		internal virtual void DoPeriodicEvent() {}

		internal virtual string PingAPI()
		{
			return "Module has no API callout.";
		}

		internal virtual string PingAPI(string token)
		{
			return PingAPI();
		}

		internal virtual string Interrogate()
		{
			return "Module has no interrogation feedback.";
		}

	}
}
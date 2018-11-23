using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Newtonsoft.Json.Linq;

namespace Agatha2
{
	internal class CommandSS13status : BotCommand
	{
		public CommandSS13status()
		{
			usage = "ss13status <url | server alias>";
			description = "Shows information about an SS13 server using a compatible API.";
			aliases = new List<string>() {"ss13status","ss13"};
		}
		public override async Task ExecuteCommand(SocketMessage message)
		{
		}
	}
}
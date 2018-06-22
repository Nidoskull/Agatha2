using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.IO;

namespace Agatha2
{
	public class Program
	{
		private static DiscordSocketClient _client;
		private static BotConfig _config;

		public static DiscordSocketClient Client {get => _client; set => _client = value; }
		public static BotConfig Config {get => _config; set => _config = value; }

		public static void Main(string[] args)
		{
 			new Program().MainAsync().GetAwaiter().GetResult();
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		public async Task MainAsync()
		{

			Config = new BotConfig();
			Client = new DiscordSocketClient();

			Client.Log += Log;
			Client.MessageReceived += MessageReceived;

			await Client.LoginAsync(TokenType.Bot, Config.Token);
			await Client.StartAsync();
			await Task.Delay(-1);
			// force the Twitch poller to init
			await BotTwitchPoller.PollStreamers(null);
		}

		private async Task MessageReceived(SocketMessage message)
		{

			if(!message.Author.IsBot)
			{
				if(message.Content.Length > 1 && message.Content.StartsWith(Config.CommandPrefix))
				{
					await BotUtilities.ResolveCommand(message);
				}
				else 
				{
					BotMarkov.ReceiveInput(message);
				}
			}
		}
	}
}
using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Agatha2
{
	public class Program
	{
		public static Random rnjesus = new Random();
		private string _token = "SORRYNOTOKEN";
		public string Token { get => _token; set => _token = value; }
		public static DiscordSocketClient client;

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
			client = new DiscordSocketClient();

			client.Log += Log;
			client.MessageReceived += MessageReceived;

			await client.LoginAsync(TokenType.Bot, Token);
			await client.StartAsync();
			await Task.Delay(-1);
		}

		private async Task MessageReceived(SocketMessage message)
		{

			if(!message.Author.IsBot)
			{
				char[] message_chars = message.Content.ToCharArray();
				if(message_chars.Length > 1 && message_chars[0] == BotConfig.commandPrefix)
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
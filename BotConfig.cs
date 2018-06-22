using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ProtoBuf;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using Nett;

namespace Agatha2
{
	public class BotConfig 
	{
		private bool _isAwake;
		private string _commandPrefix;
		private int _markovChance;
		private string _token;
		private ulong _streamChannelId;
		private string _streamAPIClientID;

		public string Token { get => _token; set => _token = value; }
		public bool Awake { get => _isAwake; set => _isAwake = value; }
		public string CommandPrefix { get => _commandPrefix; set => _commandPrefix = value; }
		public string StreamAPIClientID { get => _streamAPIClientID; set => _streamAPIClientID = value; }
		public int MarkovChance { get => _markovChance; set => _markovChance = value; }
		public ulong StreamChannelID { get => _streamChannelId; set => _streamChannelId = value; }

		public BotConfig()
		{
			TomlTable configTable = Toml.ReadFile("data/config.tml");
			Token =             configTable.Get<string>("Token");
			Awake =             configTable.Get<bool>("Awake");
			CommandPrefix =     configTable.Get<string>("CommandPrefix");
			MarkovChance =      configTable.Get<int>("MarkovChance");
			StreamAPIClientID = configTable.Get<string>("StreamAPIClientID");

			string streamID =   configTable.Get<string>("StreamChannelID");
			StreamChannelID =   Convert.ToUInt64(streamID);
		}

		internal async Task WakeUp(SocketMessage message)
		{
			if(Awake)
			{
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: I am already awake.");
			} 
			else
			{
				Awake = true;
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: Good morning, insect.");
			}
		}
		internal async Task ShutUp(SocketMessage message)
		{
			if(Awake)
			{
				Awake = false;
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: I will be silent for now.");
			} 
			else
			{
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: I am already being silent.");
			}
		}
		internal async Task Save(SocketMessage message)
		{
			if(IsAuthorized(message.Author))
			{
				SaveBartendingData();
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: Saving bartending dictionary.");
				SaveConfigData();
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: Saving config.");
			}
			else
			{
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: You are not authorized, insect.");
			}
		}

		private void SaveConfigData() 
		{
			Toml.WriteFile(this, "data/config.tml");
		}
		public async Task SetReplyRate(SocketMessage message)
		{
			if(IsAuthorized(message.Author))
			{
				try
				{
					int newReplyRate = Convert.ToInt32(message.Content.Substring(11));
					if(newReplyRate >= 0 && newReplyRate <= 100)
					{
						MarkovChance = newReplyRate;
						await message.Channel.SendMessageAsync($"{message.Author.Mention}: Reply rate is now {MarkovChance}.");
					}
					else
					{
						await message.Channel.SendMessageAsync($"{message.Author.Mention}: Enter a value between 0 and 100, insect.");					
					}
				}
				catch
				{
					await message.Channel.SendMessageAsync($"{message.Author.Mention}: Enter a value between 0 and 100, insect.");					
				}
			} 
			else
			{
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: You are not authorized, insect.");
			}
		}

		public void SaveBartendingData()
		{
			if(BotUtilities.HasDictionaryChanged)
			{
				BotUtilities.HasDictionaryChanged = false;
				Console.WriteLine("Serializing bartending dictionary.");
				using (var file = File.Create("data/bartending.bin")) {
					Serializer.Serialize(file, BotUtilities.BartendingData);
				}
			}
		}

		private bool IsAuthorized(SocketUser user)
		{
			return true;
		}

	}
}
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ProtoBuf;
using System.IO;
using System.Reactive.Linq;
using System.Threading;

namespace Agatha2
{
	internal static class BotUtilities 
	{

		private static Dictionary<string, List<string>> bartending;
		private static bool hasDictionaryChanged = false;
		private static List<string> validDrinkFields = new List<string>(new string[] {"vessel","beverage","garnish"});

		static BotUtilities() 
		{
			Console.WriteLine("Deserializing bartending dictionary.");
			try
			{
				using (var file = File.OpenRead("data/bartending.bin")) {
					bartending = Serializer.Deserialize<Dictionary<string, List<string>>>(file);
				}
			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine(ex);
				bartending = new Dictionary<string, List<string>>();
				bartending.Add("vessel", new List<string>(new string[] {"a generic cup"}));
				bartending.Add("garnish", new List<string>(new string[] {"a generic garnish"}));
				bartending.Add("beverage", new List<string>(new string[] {"a generic liquid"}));
			}

			IObservable<long> periodicSaveTimer = Observable.Interval(TimeSpan.FromMinutes(10));
			CancellationTokenSource source = new CancellationTokenSource();
			Action action = (() => 
			{
				TrySaveDictionary();
			}
			);
			periodicSaveTimer.Subscribe(x => { Task task = new Task(action); task.Start();}, source.Token);
		}

		public static async Task ResolveCommand(SocketMessage message)
		{
			string[] message_contents = message.Content.Substring(1).Split(" ");
			switch(message_contents[0])
			{
				case "drink":
					await BotUtilities.CommandDrink(message);
					break;
				case "save":
					await BotUtilities.CommandSave(message);
					break;
				case "wakeup":
					await BotUtilities.CommandWakeUp(message);
					break;
				case "shutup":
					await BotUtilities.CommandShutUp(message);
					break;
				default:
					await message.Channel.SendMessageAsync("Unknown command, insect.");
					break;
			}
		}

		private static void TrySaveDictionary()
		{
			if(hasDictionaryChanged)
			{
				hasDictionaryChanged = false;
				Console.WriteLine("Serializing bartending dictionary.");
				using (var file = File.Create("data/bartending.bin")) {
					Serializer.Serialize(file, bartending);
				}
			}
		}

		private static bool IsAuthorized(SocketUser user)
		{
			return true;
		}

		internal static async Task CommandWakeUp(SocketMessage message)
		{
			if(BotConfig.isAwake)
			{
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: I am already awake.");
			} 
			else
			{
				BotConfig.isAwake = true;
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: Good morning, insect.");
			}
		}
		internal static async Task CommandShutUp(SocketMessage message)
		{
			if(BotConfig.isAwake)
			{
				BotConfig.isAwake = false;
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: I will be silent for now.");
			} 
			else
			{
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: I am already being silent.");
			}
		}
		internal static async Task CommandSave(SocketMessage message)
		{
			if(IsAuthorized(message.Author))
			{
				TrySaveDictionary();
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: Saving bartending dictionary.");
			}
			else
			{
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: You are not authorized, insect.");
			}
		}

		internal static async Task CommandDrink(SocketMessage message)
		{
			string result = "Usage: .drink add [beverage|garnish|vessel] thing.";
			string[] message_contents = message.Content.Substring(1).Split(" ");
			if(message_contents.Length == 1) 
			{
				List<string> drinkParts = new List<string>();
				foreach(string drinkPart in validDrinkFields) 
				{
					drinkParts.Add(bartending[drinkPart][Program.rnjesus.Next(bartending[drinkPart].Count)]);
				}
				result = $"_slings {drinkParts[0]}, containing {drinkParts[1]} {drinkParts[2]}, down the bar to {message.Author.Mention}._";
			} 
			else if(message_contents.Length >= 4)
			{
				if(message_contents[1].ToLower() == "add")
				{
					string barKey = message_contents[2].ToLower();
					if(validDrinkFields.Contains(barKey))
					{
						string barText = message.Content.Substring(12 + barKey.Length);
						bartending[barKey].Add(barText);
						result = $"_will now stock {barText} as a {barKey}._";
						hasDictionaryChanged = true;
					}
				}
			}
			await message.Channel.SendMessageAsync(result);
		}
	}
}
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

		private static Dictionary<string, List<string>> _bartending;
		private static bool _hasDictionaryChanged = false;
		private static List<string> validDrinkFields = new List<string>(new string[] {"vessel","beverage","garnish"});

		public static Random rnjesus = new Random();
		public static Dictionary<string, List<string>> BartendingData { get => _bartending; set => _bartending = value; }
		public static bool HasDictionaryChanged { get => _hasDictionaryChanged; set => _hasDictionaryChanged = value; }

		static BotUtilities() 
		{
			Console.WriteLine("Deserializing bartending dictionary.");
			try
			{
				using (var file = File.OpenRead("data/bartending.bin")) {
					BartendingData = Serializer.Deserialize<Dictionary<string, List<string>>>(file);
				}
			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine(ex);
				BartendingData = new Dictionary<string, List<string>>();
				BartendingData.Add("vessel", new List<string>(new string[] {"a generic cup"}));
				BartendingData.Add("garnish", new List<string>(new string[] {"a generic garnish"}));
				BartendingData.Add("beverage", new List<string>(new string[] {"a generic liquid"}));
			}

			IObservable<long> periodicSaveTimer = Observable.Interval(TimeSpan.FromMinutes(10));
			CancellationTokenSource source = new CancellationTokenSource();
			Action action = (() => 
			{
				Program.Config.SaveBartendingData();
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
					await Program.Config.Save(message);
					break;
				case "wakeup":
					await Program.Config.WakeUp(message);
					break;
				case "shutup":
					await Program.Config.ShutUp(message);
					break;
				case "replyrate":
					await Program.Config.SetReplyRate(message);
					break;
				default:
					await message.Channel.SendMessageAsync("Unknown command, insect.");
					break;
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
					drinkParts.Add(BartendingData[drinkPart][rnjesus.Next(BartendingData[drinkPart].Count)]);
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
						BartendingData[barKey].Add(barText);
						result = $"_will now stock {barText} as a {barKey}._";
						HasDictionaryChanged = true;
					}
				}
			}
			await message.Channel.SendMessageAsync(result);
		}
	}
}
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
	internal static class BotMarkov
	{
		private static bool hasDictionaryChanged = false;
		private static Dictionary<string, List<string>> markovDict;
		static BotMarkov()
		{
			Console.WriteLine("Deserializing Markov dictionary.");
			try
			{
				using (var file = File.OpenRead("data/markov.bin")) {
					markovDict = Serializer.Deserialize<Dictionary<string, List<string>>>(file);
				}
			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine(ex);
				markovDict = new Dictionary<string, List<string>>();
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

		private static void TrySaveDictionary()
		{
			if(hasDictionaryChanged)
			{
				hasDictionaryChanged = false;
				Console.WriteLine("Serializing Markov dictionary.");
				using (var file = File.Create("data/markov.bin")) {
					Serializer.Serialize(file, markovDict);
				}
			}
		}
		public static string GetMarkovChain(string initialToken)
		{
			string result = "";

			if(markovDict.ContainsKey(initialToken))
			{
				int max_tokens = 30;
				string token = initialToken;
				result = $"{token}";
				while(max_tokens > 0 && markovDict.ContainsKey(token) && markovDict[token].Count > 0)
				{
					token = markovDict[token][BotUtilities.rnjesus.Next(markovDict[token].Count)];
					result += $" {token}";
					max_tokens--;
				}
			}
			result = result[0].ToString().ToUpper() + result.Substring(1);
			string finalChar = result.Substring(result.Length-1, 1);
			if(finalChar != "." && finalChar != "!" && finalChar != "?")
			{
				result += ".";
			}
			return result;
		}
		public static void ReceiveInput(SocketMessage message)
		{
			string lastString = null;
			foreach(string token in message.Content.Split(" "))
			{
				if(!markovDict.ContainsKey(token))
				{
					markovDict.Add(token, new List<string>());
					hasDictionaryChanged = true;
				}
				if(lastString != null)
				{
					if(!markovDict[lastString].Contains(token))
					{
						markovDict[lastString].Add(token);
						hasDictionaryChanged = true;
					}
				}
				lastString = token;
			}
			if(Program.Config.Awake && (message.Content.Contains("Agatha") || message.Content.Contains("agatha") || BotUtilities.rnjesus.Next(100) <= Program.Config.MarkovChance))
			{
				string[] tokens = message.Content.Split(" ");
				string markovText = GetMarkovChain(tokens[BotUtilities.rnjesus.Next(tokens.Length)]);
				if(markovText != null && markovText != "") 
				{
					message.Channel.SendMessageAsync(markovText);
				}
			}
		}
	}
}
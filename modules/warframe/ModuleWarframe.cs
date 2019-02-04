using Discord;
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
using System.Threading;

namespace Agatha2
{
	internal class ModuleWarframe : BotModule
	{
		private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		internal Dictionary<string, Dictionary<string, string>> alerts = new Dictionary<string, Dictionary<string, string>>();
		internal Dictionary<string, string> nodes = new Dictionary<string, string>();
		private static List<string> hekPostStrings;
		private static List<string> ordisPostStrings;
		private static List<string> vorPostStrings;
		private Dictionary<string, string> tokensToStrings = new Dictionary<string, string>();

		public ModuleWarframe()
		{
			moduleName = "Warframe";
			description = "A pointless module for interjecting Warframe quotes into innocent conversations.";

			tokensToStrings.Add("mt_excavate",       "Excavation");
			tokensToStrings.Add("mt_sabotage",       "Sabotage");
			tokensToStrings.Add("mt_capture",        "Capture");
			tokensToStrings.Add("mt_survival",       "Survival");
			tokensToStrings.Add("mt_extermination",  "Exterminate");
			tokensToStrings.Add("mt_mobile_defense", "Mobile Defense");
			tokensToStrings.Add("mt_territory",      "Interception");
			tokensToStrings.Add("mt_intel",          "Spy");
			tokensToStrings.Add("fc_grineer",        "Grineer");
			tokensToStrings.Add("fc_infestation",    "Infestation");
			tokensToStrings.Add("fc_corpus",         "Corpus");
			tokensToStrings.Add("fc_orokin",         "Orokin");
			tokensToStrings.Add("/lotus/types/items/miscitems/voidteardrop",                             "Void Traces");
			tokensToStrings.Add("/lotus/types/items/miscitems/eventium",                                 "Synthula");
 			tokensToStrings.Add("/lotus/storeitems/upgrades/mods/fusionbundles/alertfusionbundlesmall",  "50 x Endo");
 			tokensToStrings.Add("/lotus/storeitems/upgrades/mods/fusionbundles/alertfusionbundlemedium", "100 x Endo");
			tokensToStrings.Add("/lotus/storeitems/types/recipes/weapons/plasmaswordblueprint",          "Plasma Sword Blueprint");
			tokensToStrings.Add("/lotus/storeitems/upgrades/mods/shotgun/dualstat/acceleratedblastmod",  "Accelerated Blast");

			JObject planets = JObject.Parse(File.ReadAllText("data/warframe_nodes.json"));
			Dictionary<int, string> planetIds = new Dictionary<int, string>();
			foreach(JToken planet in planets["planets"])
			{
				string planet_name = planet["name"].ToString().Substring(0, 1).ToUpper() + planet["name"].ToString().Substring(1).ToLower();
				planetIds.Add((int)planet["planet_id"], planet_name);
			}
			foreach(JToken node in planets["nodes"])
			{
				string node_name = node["name"].ToString().Substring(0, 1).ToUpper() + node["name"].ToString().Substring(1).ToLower();
				nodes.Add(node["node_id"].ToString(), $"{node_name} ({planetIds[(int)node["planet_id"]]})");
			}
		}

		private static DateTime FromUnixTime(long unixTime)
		{
			return epoch.AddMilliseconds(unixTime);
		}

		private string ConvertToken(string token)
		{
			token = token.ToLower();
			if(tokensToStrings.ContainsKey(token))
			{
				return tokensToStrings[token];
			}
			return token;
		}
		public override void StartModule()
		{
			Console.WriteLine("Starting Warframe world state polling.");
			CheckAlerts();
			IObservable<long> pollTimer = Observable.Interval(TimeSpan.FromMinutes(1));
			CancellationTokenSource source = new CancellationTokenSource();
			Action action = (() => 
			{
				CheckAlerts();
			}
			);
			pollTimer.Subscribe(x => { Task task = new Task(action); task.Start();}, source.Token);
		}

		public void CheckAlerts()
		{

			IMessageChannel channel = Program.Client.GetChannel(Program.WarframeFeedChannel) as IMessageChannel;
			if(channel == null)
			{
				return;
			}

			var request = (HttpWebRequest)WebRequest.Create("http://content.warframe.com/dynamic/worldState.php");
			request.Method = "Get";

			try
			{
				using (var s = request.GetResponse().GetResponseStream())
				{
					using (var sr = new System.IO.StreamReader(s))
					{
						Dictionary<string, Dictionary<string, string>> updatedAlerts = new Dictionary<string, Dictionary<string, string>>();
						List<string> newAlerts = new List<string>();
						var jsonObject = JObject.Parse(sr.ReadToEnd());
						foreach(var alert in jsonObject["Alerts"])
						{

							string alertId = alert["_id"]["$oid"].ToString();
							Dictionary<string, string> alertInfoToSave = new Dictionary<string, string>();
							if(!alerts.ContainsKey(alertId))
							{
								newAlerts.Add(alertId);
							}

							var alertData = alert["MissionInfo"];
							string expiryString = "unknown";

							try
							{
								DateTime convertedTime = FromUnixTime(Convert.ToInt64(alert["Expiry"]["$date"]["$numberLong"].ToString()));
								TimeSpan diff = convertedTime - DateTime.UtcNow;

								string hourString = null;
								if(diff.Hours == 1)
								{
									hourString = "1 hour";
								}
								else if(diff.Hours > 1)
								{
									hourString = $"{diff.Hours} hours";
								}

								string minuteString = null;
								if(diff.Minutes == 1)
								{
									minuteString = "1 minute";
								}
								else if(diff.Minutes > 1)
								{
									minuteString = $"{diff.Minutes} minutes";
								}

								string secondString = null;
								if(diff.Seconds == 1)
								{
									secondString = "1 second";
								}
								else if(diff.Seconds > 1)
								{
									secondString = $"{diff.Seconds} seconds";
								}

								if(hourString != null && minuteString != null && secondString != null)
								{
									expiryString = $"{hourString}, {minuteString} and {secondString}";
								}
								else if(hourString != null && minuteString != null)
								{
									expiryString = $"{hourString} and {minuteString}";
								}
								else if(minuteString != null && secondString != null)
								{
									expiryString = $"{minuteString} and {secondString}";
								}
								else if(hourString != null && secondString != null)
								{
									expiryString = $"{hourString} and {secondString}";
								}
								else if(hourString != null)
								{
									expiryString = hourString;
								}
								else if(minuteString != null)
								{
									expiryString = minuteString;
								}
								else if(secondString != null)
								{
									expiryString = secondString;
								}

							}
							catch(Exception e)
							{
								Console.WriteLine($"Couldn't convert time ({e.Message}).");
							} 

							string rewardString = "";
							if(alertData["missionReward"] != null)
							{
								if(alertData["missionReward"]["items"] != null)
								{
									foreach(var item in alertData["missionReward"]["items"])
									{
										rewardString += $"\n- {ConvertToken(item.ToString())}";
									}
								}
								if(alertData["missionReward"]["countedItems"] != null)
								{
									foreach(var item in alertData["missionReward"]["countedItems"])
									{
										rewardString += $"\n- {item["ItemCount"].ToString()} x {ConvertToken(item["ItemType"].ToString())}";
									}
								}
								if(alertData["missionReward"]["credits"] != null)
								{
									rewardString += $"\n- {alertData["missionReward"]["credits"].ToString()} credits";
								}
							}

							string alertHeader = alertData["location"].ToString();
							if(nodes.ContainsKey(alertHeader))
							{
								alertHeader = nodes[alertHeader];
							}

							alertInfoToSave.Add("Header",      alertHeader);
							alertInfoToSave.Add("Level",       $"Level {alertData["minEnemyLevel"].ToString()} - {alertData["maxEnemyLevel"].ToString()}");
							alertInfoToSave.Add("Mission Type", $"{ConvertToken(alertData["missionType"].ToString())}");
							alertInfoToSave.Add("Faction",     ConvertToken(alertData["faction"].ToString()));
							alertInfoToSave.Add("Rewards",     rewardString);
							alertInfoToSave.Add("Expires",     expiryString);
							updatedAlerts.Add(alertId, alertInfoToSave);
						}
						alerts = updatedAlerts;
						EmbedBuilder embedBuilder = new EmbedBuilder();
						if(newAlerts.Count > 0)
						{
							foreach(string alert in newAlerts)
							{
								if(!alerts.ContainsKey(alert))
								{
									continue;
								}
								Dictionary<string, string> alertInfo = alerts[alert];
								if(alertInfo["Expires"] != "unknown")
								{
									embedBuilder.AddField($"{alertInfo["Header"]} - {alertInfo["Mission Type"]} ({alertInfo["Faction"]})", $"{alertInfo["Level"]}. Expires in {alertInfo["Expires"]}.\nRewards:{alertInfo["Rewards"]}");
								}
							}
							channel.SendMessageAsync($"There are new mission alerts available, Tenno.", false, embedBuilder);
						}
					}
				}
			}
			catch(Exception e)
			{
				Console.WriteLine($"Exception in Warframe worldstate lookup: {e.ToString()}.");
			}
		}

		public override bool Register(List<BotCommand> commands)
		{
			vorPostStrings =   new List<string>(File.ReadAllLines("data/vor_strings.txt"));
			hekPostStrings =   new List<string>(File.ReadAllLines("data/hek_strings.txt"));
			ordisPostStrings = new List<string>(File.ReadAllLines("data/ordis_strings.txt"));
			commands.Add(new CommandAlerts());
			return true;
		}
		public override void ListenTo(SocketMessage message)
		{
			string searchSpace =  message.Content.ToLower();
			if(searchSpace.Contains("hek"))
			{
				Task.Run( () => message.Channel.SendMessageAsync(hekPostStrings[Program.rand.Next(hekPostStrings.Count)]));
			}
			else if(searchSpace.Contains("ordis"))
			{
				Task.Run( () => message.Channel.SendMessageAsync(ordisPostStrings[Program.rand.Next(ordisPostStrings.Count)]));
			}
			else if(searchSpace.Contains("look at them"))
			{
				Task.Run( () => message.Channel.SendMessageAsync(vorPostStrings[Program.rand.Next(vorPostStrings.Count)]));
			}
		}
	}
}
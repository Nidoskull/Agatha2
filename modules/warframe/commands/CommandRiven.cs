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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agatha2
{
	internal class CommandRiven : BotCommand
	{
		private DateTime lastRivenUpdate;
		private JArray rivenDataCache;
		internal CommandRiven()
		{
			usage = "riven";
			description = "Check the current pricing for a particular riven.";
			aliases = new List<string>() {"riven", "rivens"};
		}
		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			EmbedBuilder embedBuilder = new EmbedBuilder();
			try 
			{
				if(rivenDataCache == null || lastRivenUpdate.Date.CompareTo(DateTime.UtcNow.Date) != 0)
				{
					lastRivenUpdate = DateTime.UtcNow;
					WebClient client = new WebClient();
					rivenDataCache = JArray.Parse((string)client.DownloadString("http://n9e5v4d8.ssl.hwcdn.net/repos/weeklyRivensPC.json"));
					Program.WriteToLog($"Riven data refreshed.");
				}

				string fullMsg = message.Content.ToString().ToLower();
				int searchPoint = fullMsg.IndexOf(' ');
				string rivenName;
				if(searchPoint >= 0)
				{
					rivenName = fullMsg.Substring(searchPoint).Trim();
					if(rivenName != null && rivenName != "")
					{
						List<string> matches = new List<string>();
						foreach(JToken riven in rivenDataCache)
						{

							/*
								{
									"itemType" : "Melee Riven Mod",
									"compatibility" : null,
									"rerolled" : false,
									"avg" : 36.11,
									"stddev" : 68.23,
									"min" : 2,
									"max" : 4260,
									"pop" : 97,
									"median" : 35
								},
							*/

							string checkingName = riven["compatibility"]?.ToString();
							string cleanCheckingName = checkingName?.ToLower();
							if(cleanCheckingName != null && cleanCheckingName.IndexOf(rivenName) >= 0 && !matches.Contains(checkingName))
							{
								matches.Add(checkingName);
							}
						}

						if(matches.Count > 1)
						{
							embedBuilder.AddField("Multiple matches", $"There were several possible Rivens for your search term. Did you mean: {string.Join(", ", matches.ToArray())}?");
						}
						else if(matches.Count == 1)
						{

							string rivenTitle = matches[0];
							int entries = 0;
							int hig = 0;
							int low = 0;
							int sld = 0;
							double avg = 0;

							foreach(JToken riven in rivenDataCache)
							{
								if(rivenTitle.CompareTo(riven["compatibility"]?.ToString()) != 0)
								{
									continue;
								}
								entries++;
								int rivMin = (int)riven["min"];
								if(low == 0 || rivMin <= low)
								{
									low = rivMin;
								}
								int rivMax = (int)riven["max"];
								if(rivMax >= hig)
								{
									hig = rivMax;
								}
								sld += (int)riven["pop"];
								avg += (double)riven["median"];
							}
							avg /= entries;
							embedBuilder.Title = rivenTitle;
							embedBuilder.AddField("Highest price", $"{hig}");
							embedBuilder.AddField("Lowest price",  $"{low}");
							embedBuilder.AddField("Sold today",    $"{sld}");
						}
						else
						{
							embedBuilder.AddField("No results", "There were no Rivens matching your string sold today. Maybe try riven.market?");
						}
					}
					else
					{
						embedBuilder.AddField("No search term", "Please specify a name or partial name of the Riven you wish to search for.");
					}
				}
				else
				{
					embedBuilder.AddField("No search term", "Please specify a name or partial name of the Riven you wish to search for.");
				}
			}
			catch(Exception e)
			{
				Program.WriteToLog($"Exception when showing riven prices: {e.ToString()}.");
				embedBuilder.AddField("Oh no", "Something broke, sorry. Try again later.");
			}
			await Program.SendReply(message, embedBuilder);
		}
	}
}
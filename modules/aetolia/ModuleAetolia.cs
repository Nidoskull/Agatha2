using Discord;
using Discord.WebSocket;
using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Reactive.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Agatha2
{
	internal class FishingHole
	{
		internal string holeName;
		internal string holeType;
		internal string holeId;
		internal List<string> vNums;
		internal List<string> containsFish;
		internal FishingHole(string _name, string _type, List<string> _vnum, List<string> _fish)
		{
				holeName = _name;
				holeType = _type;
				vNums = _vnum;
				containsFish = _fish;
		}
	}

	internal class ModuleAetolia : BotModule
	{
		internal ModuleAetolia()
		{
			moduleName = "Aetolia";
			description = "A character lookup and news-reading module for the IRE MUD Aetolia: The Midnight Age.";
			hasPeriodicEventInSeconds = 60;
		}

		//internal List<string> seenEvents = new List<string>();
		private string fishDbPath = @"modules/aetolia/data/fishdb.json";
		internal List<FishingHole> fishingHoles;
		//internal Dictionary<ulong, ulong> aetoliaChannelIds = new Dictionary<ulong, ulong>();

		internal override void StartModule()
		{
			if(!File.Exists(fishDbPath))
			{
				Program.WriteToLog($"No fish database found.");
			}
			else
			{
				List<string> uniqueFish = new List<string>();
				JArray fishDb = JArray.Parse(File.ReadAllText(fishDbPath));
				foreach(JToken fish in fishDb)
				{
					FishingHole fishHole = new FishingHole(
						fish["name"].ToString(), 
						fish["type"].ToString(), 
						fish["rooms"].ToObject<List<string>>(),
						fish["fish"].ToObject<List<string>>()
					);
					fishingHoles.Add(fishHole);
					fishHole.holeId = fishingHoles.Count.ToString();
					foreach(string fishName in fishHole.containsFish)
					{
						if(!uniqueFish.Contains(fishName))
						{
							uniqueFish.Add(fishName);
						}
					}
				}
				Program.WriteToLog($"Associated {uniqueFish.Count} fish with {fishingHoles.Count} fishing holes. Done.");
			}
		}

		internal override void LoadConfig()
		{
			/*
			string loadFile = @"modules/aetolia/data/channel_ids.json";
			if(File.Exists(loadFile))
			{
				foreach(KeyValuePair<string, string> guildAndChannel in JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(loadFile)))
				{
					try
					{
						aetoliaChannelIds.Add((ulong)Convert.ToInt64(guildAndChannel.Key), (ulong)Convert.ToInt64(guildAndChannel.Value));
					}
					catch(Exception e)
					{
						Program.WriteToLog($"Exception when loading Aetolia API channel config: {e.Message}");
					}
				}
			}
			*/
		}
		internal override bool Register(List<BotCommand> commands)
		{
			fishingHoles = new List<FishingHole>();
			//commands.Add(new CommandNstat());    // Disabled pending API update, may be redundant 
			//commands.Add(new CommandReadnews()); // in view of official Aet Discord integration
			//commands.Add(new CommandHonours());
			//commands.Add(new CommandWho());
			commands.Add(new CommandFish());
			commands.Add(new CommandAltprompt());
			return true;
		}

		/*	
		internal HttpWebResponse GetAPIResponse(string responseType)
		{
			HttpWebResponse s = null;
			try
			{
				string endPoint = $"http://api.aetolia.com/{responseType}.json";
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endPoint);
				request.Method = "Get";
				s = (HttpWebResponse)request.GetResponse();
				if(s != null && s.StatusCode.ToString() != "OK")
				{
					Program.WriteToLog($"Non-OK statuscode: {s.StatusCode.ToString()}");
					s = null;
				}
			}
			catch(Exception e)
			{
				Program.WriteToLog($"Exception in Aetolia auth: {e.ToString()}.");
			}
			return s;
		}

		internal override string PingAPI()
		{
			return PingAPI("checkauth");
		}

		internal override string PingAPI(string token)
		{
			string result = "Malformed, null or invalid API response, or something broke. Check logs.";
			try
			{
				HttpWebResponse aetInfo = GetAPIResponse(token);
				if(aetInfo != null)
				{
					var s = aetInfo.GetResponseStream();
					if(s != null)
					{
						StreamReader sr = new StreamReader(s);
						string sr_result = sr.ReadToEnd();
						if(sr_result == null)
						{
							result = "aetInfo stream contents are null.";
						}
						else
						{
							try
							{
								var jToken = JToken.Parse(sr_result);
								if(jToken is JArray)
								{
									JArray jArray = (JArray)jToken;
									result = $"Array:\n{jArray.ToString()}";
								}
								else if(jToken is JObject)
								{
									JObject jObject = (JObject)jToken;
									result = $"Object:\n{jObject.ToString()}";
								}
								else
								{
									result = $"Token:\n{jToken.ToString()}";
								}
							}
							catch(Exception e)
							{
								result = $"Exception when reading JSON object: {e.Message}\n\nRaw string is:\n\n{sr_result}";
							}
						}
					}
					else
					{
						result = "Null return from GetResponseStream().";
					}
				}
			}
			catch(Exception e)
			{
				Program.WriteToLog("Done2");
				result = $"Exception when calling out to Aetolia API:\n```{e.Message}```";
			}
			return $"`{token}.json` reply: ```\n{result}```";
		}
		internal override void DoPeriodicEvent()
		{

			if(aetoliaChannelIds.Count <= 0)
			{
				return;
			}

			HttpWebResponse aetInfo = GetAPIResponse("gamefeed");
			List<string> resultDescriptions = new List<string>();
			List<string> resultTitles = new List<string>();
			if(aetInfo != null)
			{
				{
					var s = aetInfo.GetResponseStream();
					if(s != null)
					{
						StreamReader sr = new StreamReader(s);
						foreach(JToken x in JToken.Parse(sr.ReadToEnd()))
						{
							string tokenId = x["id"].ToString().ToLower().Trim();
							if(!seenEvents.Contains(tokenId))
							{
								seenEvents.Add(tokenId);
								resultDescriptions.Add(x["description"].ToString().Trim());
								string resultTitle = x["caption"].ToString();
								if(!resultTitles.Contains(resultTitle))
								{
									resultTitles.Add(resultTitle);
								}
							}
						}
						if(seenEvents.Count > 25)
						{
							seenEvents.RemoveRange(0, seenEvents.Count - 25);
						}
					}
				}
			}

			if(resultDescriptions.Count > 0)
			{
				EmbedBuilder embedBuilder = new EmbedBuilder();
				embedBuilder.Title = string.Join(", ", resultTitles.ToArray());
				embedBuilder.Description = string.Join("\n", resultDescriptions.ToArray());
				Embed embed = embedBuilder.Build();
				foreach(KeyValuePair<ulong, ulong> channelId in aetoliaChannelIds)
				{
					IMessageChannel channel = Program.Client.GetChannel(channelId.Value) as IMessageChannel;
					if(channel != null)
					{
						Task.Run( () => ( Program.SendReply(channel, embedBuilder) ));
					}
				}
			}
		}
		*/
	}
}
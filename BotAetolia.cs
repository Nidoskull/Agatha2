using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ProtoBuf;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Agatha2
{
	internal static class BotAetolia 
	{
   		internal static HttpWebResponse GetAetoliaAPIResponse(string responseType)
		{
			string endPoint = $"http://api.aetolia.com/{responseType}.json";
			Console.WriteLine($"IRE API: Hitting {endPoint}.");
		    HttpWebResponse s = null;
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endPoint);
			request.Method = "Get";
            try
            {
                s = (HttpWebResponse)request.GetResponse();
                if(s != null && s.StatusCode.ToString() != "OK")
				{
	                Console.WriteLine($"Bot is not authed with Aetolia.");
					s = null;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Exception in Aetolia auth: {e.ToString()}.");
            }
			return s;
		}
        
        internal static async Task CommandAetoliaNstat(SocketMessage message)
        {
            string result = "Unknown error.";
            HttpWebResponse aetInfo = GetAetoliaAPIResponse("news");
            if(aetInfo != null)
            {
                var s = aetInfo.GetResponseStream();
                if(s != null)
                {
                    result = "```\n-- The Aetolian News --------------------------------------";
                    StreamReader sr = new StreamReader(s);
                    foreach(var x in JToken.Parse(sr.ReadToEnd()))
                    {
                        string padding = new String(' ', 49 - (x["name"].ToString().Length + x["total"].ToString().Length));
                        result = $"{result}\n {x["name"]}:{padding}{x["total"]} posts.";
                    }
                    result = $"{result}\n-----------------------------------------------------------";
                    result = $"{result}\n Read individual posts using {Program.Config.CommandPrefix}READNEWS [SECTION] [NUMBER].";
                    result = $"{result}\n-----------------------------------------------------------\n```";
                }
            }

            await message.Channel.SendMessageAsync($"{message.Author.Mention}: {result}");	
        }

        internal static async Task CommandAetoliaReadnews(SocketMessage message)
        {

  			string[] message_contents = message.Content.Substring(1).Split(" ");
			string result;
            if(message_contents.Length < 2)
            {
                result = "Please specify a news section name.";
            }
            else if(message_contents.Length < 3)
            {
                result = "Please specify an article number.";
            }
            else
            {
                result = "Invalid article number or news section.";
                HttpWebResponse aetInfo = GetAetoliaAPIResponse($"news/{message_contents[1].ToLower()}/{message_contents[2].ToLower()}");
                if(aetInfo != null)
                {
                    var s = aetInfo.GetResponseStream();
                    if(s != null)
                    {
                        StreamReader sr = new StreamReader(s);
                        JObject postInfo = JObject.Parse(sr.ReadToEnd());
                        JToken ci = postInfo["post"];
                        DateTime postDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        postDate = postDate.AddSeconds((int)ci["date"]);

                        result = $"```\n{ci["section"].ToString().ToUpper()} NEWS {ci["id"].ToString().ToUpper()}";
                        result = $"{result}\nDate: {postDate}"; // x/y/z at 00:00
                        result = $"{result}\nFrom: {ci["from"]}";
                        result = $"{result}\nTo:   {ci["to"]}";
                        result = $"{result}\nSubj: {ci["subject"]}\n";
                        string postString = ci["message"].ToString();
                        if(postString.Length > 1500)
                        {
                            string newsURL = $"https://www.aetolia.com/news/?game=Aetolia&section={ci["section"].ToString()}&number={ci["id"].ToString()}";
                            postString = $"{postString.Substring(0,1500)}...```\nPost has been trimmed for Discord: see the full text at {newsURL}.";
                        }
                        else
                        {
                            postString = $"{postString}```";
                        }
                        result = $"{result}\n{postString}";
                    }
                }
            }
            await message.Channel.SendMessageAsync($"{message.Author.Mention}: {result}");	
        }

		internal static async Task CommandAetoliaHonours(SocketMessage message)
		{
			string result = "There is no such person, I'm afraid.";
			string[] message_contents = message.Content.Substring(1).Split(" ");
			if(message_contents.Length < 2)
			{
				result = "Who do you wish to know about?";
			}
			else
			{
				HttpWebResponse aetInfo = GetAetoliaAPIResponse($"characters/{message_contents[1].ToLower()}");
				if(aetInfo != null)
				{
					var s = aetInfo.GetResponseStream();
					if(s != null)
					{
						StreamReader sr = new StreamReader(s);
						JObject ci = JObject.Parse(sr.ReadToEnd());
						result = "```\n-------------------------------------------------------------------------------";
						result = $"{result}\n{ci["fullname"]}";
						result = $"{result}\n-------------------------------------------------------------------------------";
						if(ci["class"].ToString().Equals("(none)"))
						{
							result = $"{result}\nThey are a level {ci["level"]} {ci["race"]} {ci["class"]}.";
						}
						else
						{
							result = $"{result}\nThey are a level {ci["level"]} {ci["race"]} with no class.";
						}
						if(ci["city"].ToString().Equals("(none)"))
						{
							result = $"{result}\nThey hold no citizenship.";
						}
						else
						{
							result = $"{result}\nThey are a citizen of {ci["city"]}.";
						}
						if(ci["guild"].ToString().Equals("(none)"))
						{
							result = $"{result}\nThey hold no guild membership.";
						}
						else
						{
							result = $"{result}\nThey are a member of the {ci["guild"]}.";
						}
						result = $"{result}\nThey are {ci["xp rank"].ToString().ToLower()} in experience, {ci["explore rank"].ToString().ToLower()} in exploration and {ci["combat rank"].ToString().ToLower()} in combat.";
						result = $"{result}\n-------------------------------------------------------------------------------```";
					}
				}
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: {result}");	
			}
		}

		internal static async Task CommandAetoliaWho(SocketMessage message)
		{
			string result = "Authentication or network error.";
			HttpWebResponse aetInfo = GetAetoliaAPIResponse("characters");
			if(aetInfo != null)
				{
				var s = aetInfo.GetResponseStream();
				if(s != null)
				{
					StreamReader sr = new StreamReader(s);
					List<string> characterNames = new List<string>();
					foreach(JToken player in JObject.Parse(sr.ReadToEnd())["characters"])
					{
						characterNames.Add($"{player["name"]}");
					}

					if(characterNames.Count == 0)
					{
						result = "None";
					}
					else if(characterNames.Count == 1)
					{
						result = $"{characterNames[1]}";
					}
					else
					{
						result = "";
						for(int i = 0;i < characterNames.Count;i++)
						{
							if(i != 0)
							{
								result += ", ";
							}
							if(i == (characterNames.Count-1))
							{
								result += "and ";
							}
							result += characterNames[i];
						}
					}

					string playerTerm;
					if(characterNames.Count != 1)
					{
						playerTerm = $"are {characterNames.Count} people";
					}
					else 
					{
						playerTerm = $"is {characterNames.Count} person";

					}

                    result = $"```{result}.\nThere {playerTerm} total online.```";
				}
			}
			await message.Channel.SendMessageAsync($"{message.Author.Mention}: {result}");			
		}
   	}
}
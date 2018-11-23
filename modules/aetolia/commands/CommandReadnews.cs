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

namespace Agatha2
{
	internal class CommandReadnews : BotCommand
	{
		public CommandReadnews()
		{
			usage = "readnews <section|random> <#|random>";
			description = "Shows a post from an Aetolian news board. Use nstat to check for post numbers.";
			aliases = new List<string>() {"readnews"};
		}
		public override async Task ExecuteCommand(SocketMessage message)
		{
			ModuleAetolia aetolia = (ModuleAetolia)parent;
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
				string postSection = message_contents[1].ToLower();
				string postNumber = message_contents[2].ToLower();

				if(postSection.Equals("random") || postNumber.Equals("random"))
				{
					HttpWebResponse newsInfo = aetolia.GetAPIResponse("news");
					if(newsInfo != null)
					{
						var s = newsInfo.GetResponseStream();
						if(s != null)
						{
							Dictionary<string, string> sections = new Dictionary<string, string>();
							StreamReader sr = new StreamReader(s);
							foreach(var x in JToken.Parse(sr.ReadToEnd()))
							{
								sections.Add(x["name"].ToString().ToLower(), x["total"].ToString());
							}
							if(postSection.Equals("random"))
							{
								int randInd = Program.rand.Next(sections.Count);
								postSection = sections.ElementAt(randInd).Key.ToString();
							}
							if(postNumber.Equals("random") && sections.ContainsKey(postSection))
							{
								int tempValue = Convert.ToInt32(sections[postSection]);
								postNumber = Program.rand.Next(tempValue).ToString();
							}
						}
					}
				}

				HttpWebResponse aetInfo = aetolia.GetAPIResponse($"news/{postSection}/{postNumber}");
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
						result = $"{result}\nDate: {postDate}";
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
	}
}
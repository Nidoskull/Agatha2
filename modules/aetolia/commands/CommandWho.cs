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
	internal class CommandWho : BotCommand
	{
        public CommandWho()
        {
            usage = "who";
            description = "Shows a list of characters currently logged in to Aetolia.";
            aliases = new List<string>(new string[] {"who", "qw"});
        }
        public override async Task ExecuteCommand(SocketMessage message)
        {
            ModuleAetolia aetolia = (ModuleAetolia)parent;
			string result = "Authentication or network error.";
			HttpWebResponse aetInfo = aetolia.GetAPIResponse("characters");
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
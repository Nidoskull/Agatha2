using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Nett;
using System.Linq;
using Discord.Rest;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;

namespace Agatha2
{

	internal class GuildConfig
	{
		internal ulong guildId;
		internal string adminRole = "bot wrangler";
		internal List<string> enabledModules = new List<string>();
		internal string commandPrefix = ".";
		internal string markovTrigger = "agatha";
		internal int markovChance = 0;
		internal int adHocPollCharacterLimit = 60;
		public int AdHocPollCharacterLimit { get => adHocPollCharacterLimit; set => adHocPollCharacterLimit = value; }
		public string MarkovTrigger { get => markovTrigger; set => markovTrigger = value; }
		public int MarkovChance { get => markovChance; set => markovChance = value; }
		public string AdminRole { get => adminRole; set => adminRole = value; }
		public ulong GuildId { get => guildId; set => guildId = value; }
		public List<string> EnabledModules { get => enabledModules; set => enabledModules = value; }
		public string CommandPrefix { get => commandPrefix; set => commandPrefix = value; }

		internal void Save()
		{
			string savePath = @"data/guilds";
			if(!Directory.Exists(savePath))
			{
				Directory.CreateDirectory(savePath);
			}
			File.WriteAllText($"{savePath}/{guildId}.json", JsonConvert.SerializeObject((GuildConfig)this));
		}

		internal Embed GetConfigSettings()
		{
			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.Title = "Guild configuration.";
			embedBuilder.AddField("adminRole",    AdminRole);
			embedBuilder.AddField("commandPrefix", CommandPrefix);
			embedBuilder.AddField("markovChance",  MarkovChance);
			embedBuilder.AddField("markovTrigger", MarkovTrigger);
			embedBuilder.AddField("adHocPollCharacterLimit", AdHocPollCharacterLimit);
			embedBuilder.Description = $"Use {commandPrefix}gconf [setting] [value] to modify guild configuration.";
			return embedBuilder.Build();
		}

		internal string SetConfig(string[] message_contents)
		{

			if(message_contents.Length <= 1)
			{
				return "No guild setting supplied, how did you even get to this block?";
			}

			string cmdArg = message_contents[1].ToLower();
			if(message_contents.Length <= 2)
			{
				return $"No configuration value supplied for setting '{cmdArg}'.";
			}
			else
			{

				string fullMsg = "";
				for(int i = 2;i < message_contents.Length; i++)
				{
					fullMsg += $" {message_contents[i]}";
				}
				fullMsg = fullMsg.ToLower().Trim();

				string resultString = "Unknown configuration error.";

				switch(cmdArg)
				{
					case "adminrole":
						AdminRole = fullMsg;
						resultString = $"Admin role is now '{AdminRole}'.";
						break;
					case "commandprefix":
						CommandPrefix = fullMsg;
						resultString = $"Command prefix is now '{CommandPrefix}'.";
						break;
					case "adhocpollcharacterlimit":
						try
						{
							int newReplyRate = Convert.ToInt32(fullMsg);
							if(newReplyRate < 0)
							{
								newReplyRate = 0;
							}
							AdHocPollCharacterLimit = newReplyRate;
							resultString = $"Poll character threshold is now {AdHocPollCharacterLimit}.";
							break;
						}
						catch
						{
							return "Enter a numerical value, insect.";
						}

					case "markovchance":
						try
						{
							int newReplyRate = Convert.ToInt32(fullMsg);
							if(newReplyRate >= 0 && newReplyRate <= 100)
							{
								MarkovChance = newReplyRate;
								resultString = $"Reply rate is now {MarkovChance}.";
								break;
							}
							else
							{
								return "Enter a value between 0 and 100, insect.";
							}
						}
						catch
						{
							return "Enter a value between 0 and 100, insect.";
						}
					case "markovtrigger":
						MarkovTrigger = message_contents[2].ToLower();
						resultString = $"Markov trigger word is now '{MarkovTrigger}'.";
						break;
					default:
						return $"Unknown guild configuration option '{cmdArg}'.";
				}
				Save();
				return resultString;
			}
		}
		internal string EnableModule(BotModule module)
		{
			if(enabledModules.Contains(module.moduleName))
			{
				return $"Module {module.moduleName} is already enabled for this guild.";
			}
			else
			{
				enabledModules.Add(module.moduleName);
				Save();
				return $"Enabled module {module.moduleName} for this guild.";
			}		
		}

		internal string DisableModule(BotModule module)
		{
			if(!enabledModules.Contains(module.moduleName))
			{
				return $"Module {module.moduleName} is already disabled for this guild.";
			}
			else
			{
				enabledModules.Remove(module.moduleName);
				Save();
				return $"Disabled module {module.moduleName} for this guild.";
			}		
		}

	}
}
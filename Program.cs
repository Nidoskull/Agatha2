﻿using Discord;
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
	}
	
	internal abstract class BotModule
	{
		internal string moduleName;
		internal string description;
		internal int hasPeriodicEventInSeconds = -1;
		internal abstract bool Register(List<BotCommand> commands);
		internal virtual void ListenTo(SocketMessage message, GuildConfig guild) {}
		internal virtual void StartModule() {}
		internal virtual void ReactionAdded(SocketGuild guild, SocketReaction reaction) {}
		internal virtual void ReactionRemoved(SocketGuild guild, SocketReaction reaction) {}
		internal virtual void LoadConfig() {}
		internal virtual void DoPeriodicEvent() {}
	}

	internal abstract class BotCommand
	{
		internal List<string> aliases;
		internal string usage;
		internal string description;
		internal BotModule parent;

		internal void Register(BotModule _parent)
		{
			parent = _parent;
		}
		internal abstract Task ExecuteCommand(SocketMessage message, GuildConfig guild);
	}
	
	internal class Program
	{
		internal static Random rand = new Random();
		private static DiscordSocketClient _client;
		internal static List<BotCommand> commands = new List<BotCommand>();
		internal static Dictionary<ulong, GuildConfig> guilds = new Dictionary<ulong, GuildConfig>();
		internal static Dictionary<string, BotCommand> commandAliases = new Dictionary<string, BotCommand>();
		internal static List<BotModule> modules = new List<BotModule>();
		internal static DiscordSocketClient Client {get => _client; set => _client = value; }
		internal static string token;
		internal static string _usageInformation = "dotnet run Agatha2.csproj --token=SOMETOKEN [--verbose --version]";
		internal static string UsageInformation { get => _usageInformation; set => _usageInformation = value; }
		internal static bool verbose = false;
		internal static string _sourceAuthor;
		internal static string _sourceVersion;
		internal static string _sourceLocation;
		internal static string SourceAuthor { get => _sourceAuthor; set => _sourceAuthor = value; }
		internal static string SourceVersion { get => _sourceVersion; set => _sourceVersion = value; }
		internal static string SourceLocation { get => _sourceLocation; set => _sourceLocation = value; }

		internal static void WriteToLog(string message)
		{
			if(verbose)
			{
				Console.WriteLine(message);
			}
			else
			{
				Debug.WriteLine(message);
			}
		}
		internal static bool IsAuthorized(SocketUser user, ulong guildId)
		{
			GuildConfig guild = GetGuildConfig(guildId);
			var guildUser = user as SocketGuildUser;
			var role = (guildUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == guild.adminRole);
			return guildUser.Roles.Contains(role);
		}

		internal static int Clamp(int value, int min, int max)  
		{  
			return (value < min) ? min : (value > max) ? max : value;  
		}

		internal static void Main(string[] args)
		{

			try
			{
				TomlTable configTable = Toml.ReadFile("data/config.tml");
				SourceAuthor =          configTable.Get<string>("SourceAuthor");
				SourceVersion =         configTable.Get<string>("SourceVersion");
				SourceLocation =        configTable.Get<string>("SourceLocation");
			}
			catch(Exception e)
			{
				WriteToLog($"Exception when loading version information: {e}");
				SourceAuthor =   "Someone";
				SourceVersion =  "0.:shrug:.:shrug:";
				SourceLocation = "unspecified";
			}

			for(int i = 0;i < args.Length;i++)
			{
				string check_token = args[i];
				if(check_token.ToLower().Contains("version"))
				{
					Console.WriteLine($"agatha2 v{Program.SourceVersion}");
					return;
				}
				else if(check_token.ToLower().Contains("verbose"))
				{
					verbose = true;
				}
				else if(check_token.ToLower().Contains("token"))
				{
					if(token != null)
					{
						Console.WriteLine("Duplicate token supplied.");
						Console.WriteLine(UsageInformation);
						return;
					}
					int splitpoint = check_token.IndexOf('=')+1;
					if(check_token.Length >= splitpoint)
					{
						token = check_token.Substring(splitpoint);
						break;
					}
				}
			}

			if(token == null)
			{
				Console.WriteLine("Please specify a Discord secrets token to use when connecting with this bot.");
				Console.WriteLine(UsageInformation);
				return;
			}
			Console.WriteLine($"Connecting with token '{token}'.");

			// Load all modules.
			WriteToLog("Loading modules.");
			modules.Add(new ModuleAetolia());
			modules.Add(new ModuleBartender());
			modules.Add(new ModuleChumhandle());
			modules.Add(new ModuleDice());
			modules.Add(new ModuleMarkov());
			modules.Add(new ModuleTwitch());
			modules.Add(new ModuleUtility());
			modules.Add(new ModuleWarframe());

			foreach(BotModule module in modules)
			{
				module.LoadConfig();
			}
			WriteToLog($"Registering {modules.Count} modules.");
			foreach(BotModule module in modules)
			{
				List<BotCommand> tmpCmds = new List<BotCommand>();
				if(module.Register(tmpCmds))
				{
					foreach(BotCommand command in tmpCmds)
					{
						command.Register(module);
						commands.Add(command);
					}
					WriteToLog($"Registered module {module.moduleName} with {tmpCmds.Count} commands.");
				}
			}
			WriteToLog("Done.");

			WriteToLog($"Starting {modules.Count} modules.");
			foreach(BotModule module in modules)
			{
				Task.Run(() => module.StartModule());

				if(module.hasPeriodicEventInSeconds > 0)
				{
					IObservable<long> periodicSaveTimer = Observable.Interval(TimeSpan.FromSeconds(module.hasPeriodicEventInSeconds));
					CancellationTokenSource source = new CancellationTokenSource();
					Action action = (() => 
					{
						module.DoPeriodicEvent();
					}
					);
					periodicSaveTimer.Subscribe(x => { Task task = new Task(action); task.Start();}, source.Token);
				}

			}
			WriteToLog("Done.");

			commands.Add(new CommandAbout());
			commands.Add(new CommandHelp());
			commands.Add(new CommandModules());
			commands.Add(new CommandGuild());

			WriteToLog($"Registering {commands.Count} commands with aliases.");
			foreach(BotCommand cmd in commands)
			{
				foreach(string alias in cmd.aliases)
				{
					WriteToLog($"Registered command {cmd.aliases[0]} to {alias}.");
					commandAliases.Add(alias, cmd);
				}
			}
			WriteToLog("Done.");

			// Load guild configuration.
			if(!Directory.Exists(@"data/guilds"))
			{
				Directory.CreateDirectory(@"data/guilds");
			}
			foreach (var f in (from file in Directory.EnumerateFiles(@"data/guilds", "*.json", SearchOption.AllDirectories) select new { File = file }))
			{
				WriteToLog($"Loading guild config from {f.File}.");
				try
				{
					GuildConfig guild = JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(f.File));
					guilds.Add(guild.guildId, guild);
				}
				catch(Exception e)
				{
					WriteToLog($"Exception when loading guild config: {e.Message}");
				}
			}
			WriteToLog("Done.");
			Console.WriteLine("Connected.");
			new Program().MainAsync().GetAwaiter().GetResult();
		}

		private Task Log(LogMessage msg)
		{
			WriteToLog(msg.ToString());
			return Task.CompletedTask;
		}

		internal async Task MainAsync()
		{

			// Create client.
			Client = new DiscordSocketClient();
			Client.Log += Log;
			Client.MessageReceived += MessageReceived;
			Client.ReactionAdded += ReactionAdded;
			Client.ReactionRemoved += ReactionRemoved;

			while(true)
			{
				try
				{
					WriteToLog("Logging in client.");
					await Client.LoginAsync(TokenType.Bot, token);
					WriteToLog("Starting main loop.");
					await Client.StartAsync();
					await Task.Delay(-1);
				}
				catch(Exception e)
				{
					WriteToLog($"Core loop exception: {e.Message}.");
					break;
				}
			}
		}
		private static Task ReactionRemoved(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
		{
			Task.Run(() => HandleReactionRemoved(cache, channel, reaction));
			return Task.FromResult(0);

		}

		private static Task ReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
		{
			Task.Run(() => HandleReactionAdded(cache, channel, reaction));
			return Task.FromResult(0);
		}

		private static Task MessageReceived(SocketMessage message)
		{
			Task.Run(() => HandleMessage(message));
			return Task.FromResult(0);
		}

		private static void HandleReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				IGuildUser user = (IGuildUser)cache.Value.Author;
				SocketGuildChannel guildChannel = (SocketGuildChannel)channel;
				GuildConfig guild = GetGuildConfig(guildChannel.Guild.Id);
				foreach(BotModule module in modules)
				{
					if(guild.enabledModules.Contains(module.moduleName))
					{
						module.ReactionAdded(guildChannel.Guild, reaction);
					}
				}
			}
			catch(Exception e)
			{
				Program.WriteToLog($"Exception in HandleReactionAdded: {e.Message}");
			}
		}
		private static void HandleReactionRemoved(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				IGuildUser user = (IGuildUser)cache.Value.Author;
				SocketGuildChannel guildChannel = (SocketGuildChannel)channel;
				GuildConfig guild = GetGuildConfig(guildChannel.Guild.Id);
				foreach(BotModule module in modules)
				{
					if(guild.enabledModules.Contains(module.moduleName))
					{
						module.ReactionRemoved(guildChannel.Guild, reaction);
					}
				}
			}
			catch(Exception e)
			{
				Program.WriteToLog($"Exception in HandleReactionRemoved: {e.Message}");
			}
		}
		private static void HandleMessage(SocketMessage message)
		{
			if(!message.Author.IsBot)
			{
				try
				{
					SocketGuildChannel guildChannel = message.Channel as SocketGuildChannel;
					GuildConfig guild = GetGuildConfig(guildChannel.Guild.Id);
					if(message.Content.Length > 1 && message.Content.StartsWith(guild.commandPrefix) && !message.Content.Substring(1,1).Equals(guild.commandPrefix))
					{
						string command = message.Content.Substring(guild.commandPrefix.Length).Trim();
						string[] command_tokens = command.Split(" ");
						string commandFirst = command;
						command = command_tokens[0].ToLower();

						if(commandAliases.ContainsKey(command))
						{
							BotCommand cmd = commandAliases[command];
							if(cmd.parent == null || guild.enabledModules.Contains(cmd.parent.moduleName))
							{
								cmd.ExecuteCommand(message, guild);
							}
							else
							{
								message.Channel.SendMessageAsync("That module is disabled for this guild.");
							}
						}
						else
						{
							message.Channel.SendMessageAsync($"Unknown command '{command}', insect.");
						}
						return;
					}
					else
					{
						foreach(BotModule module in modules)
						{
							if(guild.enabledModules.Contains(module.moduleName))
							{
								Task.Run(() => module.ListenTo(message, guild));
							}
						}
					}
				}
				catch(Exception e)
				{
					WriteToLog($"Unhandled exception in command input - {e.Message}.");
				}
			}
		}

		internal static GuildConfig GetGuildConfig(ulong guildId)
		{
			if(!guilds.ContainsKey(guildId))
			{
				GuildConfig guild = new GuildConfig();
				guild.guildId = guildId;
				guilds.Add(guild.guildId, guild);
				WriteToLog($"Created GuildConfig for guild {guildId}.");
			}
			return guilds[guildId];
		}

		internal static bool EnableModuleForGuild(BotModule module, ulong guildId)
		{
			GuildConfig guild = GetGuildConfig(guildId);
			if(!guild.enabledModules.Contains(module.moduleName))
			{
				guild.enabledModules.Add(module.moduleName);
				guild.Save();
				return true;
			}
			return false;
		}

		internal static bool DisableModuleForGuild(BotModule module, ulong guildId)
		{
			GuildConfig guild = GetGuildConfig(guildId);
			if(guild.enabledModules.Contains(module.moduleName))
			{
				guild.enabledModules.Remove(module.moduleName);
				guild.Save();
				return true;
			}
			return false;
		}
	}
}
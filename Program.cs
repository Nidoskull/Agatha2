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

namespace Agatha2
{

	internal class GuildConfig
	{
		internal ulong guildId;
		internal string adminRole = "bot wrangler";
		internal List<string> enabledModules = new List<string>();
		internal string commandPrefix = ".";

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
	}
	
	internal abstract class BotModule
	{
		internal string moduleName;
		internal string description;
		internal abstract bool Register(List<BotCommand> commands);
		internal virtual void ListenTo(SocketMessage message) {}
		internal virtual void StartModule() {}
		internal virtual void ReactionAdded(SocketGuild guild, SocketReaction reaction) {}
		internal virtual void ReactionRemoved(SocketGuild guild, SocketReaction reaction) {}
		internal virtual void LoadConfig() {}
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

		internal static string _sourceAuthor;
		internal static string _sourceVersion;
		internal static string _sourceLocation;
		internal static string SourceAuthor { get => _sourceAuthor; set => _sourceAuthor = value; }
		internal static string SourceVersion { get => _sourceVersion; set => _sourceVersion = value; }
		internal static string SourceLocation { get => _sourceLocation; set => _sourceLocation = value; }

		internal static bool IsAuthorized(SocketUser user, ulong guildId)
		{
			GuildConfig guild = GetGuildConfig(guildId);
			var guildUser = user as SocketGuildUser;
			var role = (guildUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == guild.adminRole);
			return (guildUser.Roles.Contains(role));
		}

		internal static int Clamp(int value, int min, int max)  
		{  
			return (value < min) ? min : (value > max) ? max : value;  
		}

		internal static void Main(string[] args)
		{

			for(int i = 0;i < args.Length;i++)
			{
				string check_token = args[i];
				if(check_token.ToLower().Contains("token"))
				{
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
				return;
			}
			Console.WriteLine($"Connecting with token '{token}'.");

			// Load all modules.
			Debug.WriteLine("Loading modules.");
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
			Debug.WriteLine($"Registering {modules.Count} modules.");
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
					Debug.WriteLine($"Registered module {module.moduleName} with {tmpCmds.Count} commands.");
				}
			}
			Debug.WriteLine("Done.");

			Debug.WriteLine($"Starting {modules.Count} modules.");
			foreach(BotModule module in modules)
			{
				Task.Run( () => module.StartModule());
			}
			Debug.WriteLine("Done.");

			commands.Add(new CommandAbout());
			commands.Add(new CommandHelp());
			commands.Add(new CommandModules());
			commands.Add(new CommandGuild());

			Debug.WriteLine($"Registering {commands.Count} commands with aliases.");
			foreach(BotCommand cmd in commands)
			{
				foreach(string alias in cmd.aliases)
				{
					Debug.WriteLine($"Registered command {cmd.aliases[0]} to {alias}.");
					commandAliases.Add(alias, cmd);
				}
			}
			Debug.WriteLine("Done.");

			// Load guild configuration.
			if(!Directory.Exists(@"data/guilds"))
			{
				Directory.CreateDirectory(@"data/guilds");
			}
			foreach (var f in (from file in Directory.EnumerateFiles(@"data/guilds", "*.json", SearchOption.AllDirectories) select new { File = file }))
			{
				Debug.WriteLine($"Loading guild config from {f.File}.");
				try
				{
					GuildConfig guild = JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(f.File));
					guilds.Add(guild.guildId, guild);
				}
				catch(Exception e)
				{
					Debug.WriteLine($"Exception when loading guild config: {e.Message}");
				}
			}
			Debug.WriteLine("Done.");

			new Program().MainAsync().GetAwaiter().GetResult();
		}

		private Task Log(LogMessage msg)
		{
			Debug.WriteLine(msg.ToString());
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
					Debug.WriteLine("Logging in client.");
					await Client.LoginAsync(TokenType.Bot, token);
					Debug.WriteLine("Starting main loop.");
					await Client.StartAsync();
					await Task.Delay(-1);
				}
				catch(Exception e)
				{
					Debug.WriteLine($"Core loop exception: {e.Message}.");
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
		private static void HandleReactionRemoved(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
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
						string command = message.Content.Substring(1).Split(" ")[0].ToLower();
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
							message.Channel.SendMessageAsync("Unknown command, insect.");
						}
						return;
					}
					else
					{
						foreach(BotModule module in modules)
						{
							if(guild.enabledModules.Contains(module.moduleName))
							{
								Task.Run( () => module.ListenTo(message));
							}
						}
					}
				}
				catch(Exception e)
				{
					Debug.WriteLine($"Unhandled exception in command input - {e.Message}.");
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
				Debug.WriteLine($"Created GuildConfig for guild {guildId}.");
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
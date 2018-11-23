using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Nett;
using System.Linq;

namespace Agatha2
{

	internal abstract class BotModule
	{
		internal string moduleName;
		public List<BotCommand> commands;
		public List<UInt64> enabledForGuilds = new List<UInt64>();
		public string description;
		public abstract bool Register(List<BotCommand> commands);
		public virtual void ListenTo(SocketMessage message) {}
		public virtual void StartModule() {}

		public virtual void LoadGuildSettings()
		{
			if(File.Exists($"data/{moduleName}_guilds.txt"))
			{
				foreach(string guildId in File.ReadAllLines($"data/{moduleName}_guilds.txt"))
				{
					try
					{
						UInt64 guildIdSnowflake = Convert.ToUInt64(guildId);
						if(guildIdSnowflake > 0)
						{
							enabledForGuilds.Add(guildIdSnowflake);
						}
					}
					catch(Exception e)
					{
						Console.WriteLine($"Exception in module guild auth: {e.ToString()}.");
					}
				}
			}
		}
		public virtual void SaveGuildSettings()
		{
			List<string> guildIDs = new List<string>();
			foreach(UInt64 guildIdUlong in enabledForGuilds)
			{
				guildIDs.Add(guildIdUlong.ToString());
			}
			System.IO.File.WriteAllLines($"data/{moduleName}_guilds.txt", guildIDs);
		}
	}

	internal abstract class BotCommand
	{
		internal List<string> aliases;
		internal string usage;
		internal string description;
		internal BotModule parent;

		public void Register(BotModule _parent)
		{
			parent = _parent;
		}
		public abstract Task ExecuteCommand(SocketMessage message);
	}
	
	public class Program
	{
		internal static Random rand = new Random();
		private static DiscordSocketClient _client;

		internal static List<BotCommand> commands;
		internal static Dictionary<string, BotCommand> commandAliases;
		internal static List<BotModule> modules;
		public static DiscordSocketClient Client {get => _client; set => _client = value; }

		private static string _commandPrefix;
		private static int _markovChance;
		private static string _token;
		private static ulong _streamChannelId;
		private static string _streamAPIClientID;

		public static string Token { get => _token; set => _token = value; }
		public static string CommandPrefix { get => _commandPrefix; set => _commandPrefix = value; }
		public static string StreamAPIClientID { get => _streamAPIClientID; set => _streamAPIClientID = value; }
		public static int MarkovChance { get => _markovChance; set => _markovChance = value; }
		public static ulong StreamChannelID { get => _streamChannelId; set => _streamChannelId = value; }

		internal static string _sourceAuthor;
		internal static string _sourceVersion;
		internal static string _sourceLocation;
		internal static string SourceAuthor { get => _sourceAuthor; set => _sourceAuthor = value; }
		internal static string SourceVersion { get => _sourceVersion; set => _sourceVersion = value; }
		internal static string SourceLocation { get => _sourceLocation; set => _sourceLocation = value; }

		internal static bool IsAuthorized(SocketUser user)
		{
			var guildUser = user as SocketGuildUser;
			var role = (guildUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "bot wrangler");
			return (guildUser.Roles.Contains(role));
		}

		internal static int Clamp(int value, int min, int max)  
		{  
			return (value < min) ? min : (value > max) ? max : value;  
		}

		public static void Main(string[] args)
		{
 			new Program().MainAsync().GetAwaiter().GetResult();
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		public async Task MainAsync()
		{

			TomlTable configTable = Toml.ReadFile("data/config.tml");
			Token =			 configTable.Get<string>("Token");
			SourceAuthor =	  configTable.Get<string>("SourceAuthor");
			SourceVersion =	 configTable.Get<string>("SourceVersion");
			SourceLocation =	configTable.Get<string>("SourceLocation");
			CommandPrefix =	 configTable.Get<string>("CommandPrefix");
			MarkovChance =	  configTable.Get<int>("MarkovChance");
			StreamAPIClientID = configTable.Get<string>("StreamAPIClientID");
			string streamID =   configTable.Get<string>("StreamChannelID");
			StreamChannelID =   Convert.ToUInt64(streamID);

			Client = new DiscordSocketClient();
			Client.Log += Log;
			Client.MessageReceived += MessageReceived;

			Console.WriteLine("Loading modules.");

			modules = new List<BotModule>();
			modules.Add(new ModuleAetolia());
			modules.Add(new ModuleBartender());
			modules.Add(new ModuleChumhandle());
			modules.Add(new ModuleDice());
			modules.Add(new ModuleMarkov());
			modules.Add(new ModuleTwitch());
			modules.Add(new ModuleWarframe());

			Console.WriteLine($"Registering {modules.Count} modules.");
			commands = new List<BotCommand>();
			foreach(BotModule module in modules)
			{
				List<BotCommand> tmpCmds = new List<BotCommand>();
				if(module.Register(tmpCmds))
				{
					module.LoadGuildSettings();
					foreach(BotCommand command in tmpCmds)
					{
						command.Register(module);
						commands.Add(command);
					}
					Console.WriteLine($"Registered module {module.moduleName} with {tmpCmds.Count} commands.");
				}
			}
			Console.WriteLine("Done.");

			Console.WriteLine($"Starting {modules.Count} modules.");
			foreach(BotModule module in modules)
			{
				Task.Run( () => module.StartModule());
			}
			Console.WriteLine("Done.");

			commands.Add(new CommandAbout());
			commands.Add(new CommandHelp());
			commands.Add(new CommandModules());
			Console.WriteLine($"Registering {commands.Count} commands with aliases.");
			commandAliases = new Dictionary<string, BotCommand>();

			foreach(BotCommand cmd in commands)
			{
				foreach(string alias in cmd.aliases)
				{
					Console.WriteLine($"Registered command {cmd.aliases[0]} to {alias}.");
					commandAliases.Add(alias, cmd);
				}
			}
			Console.WriteLine("Done.");
			Console.WriteLine("Logging in client.");

			await Client.LoginAsync(TokenType.Bot, Token);

			Console.WriteLine("Starting main loop.");
			await Client.StartAsync();
			await Task.Delay(-1);
		}
		private async Task MessageReceived(SocketMessage message)
		{

			if(!message.Author.IsBot)
			{
				if(message.Content.Length > 1 && message.Content.StartsWith(CommandPrefix) && !message.Content.Substring(1,1).Equals(CommandPrefix))
				{
					string command = message.Content.Substring(1).Split(" ")[0].ToLower();
					if(commandAliases.ContainsKey(command))
					{
						BotCommand cmd = commandAliases[command];
						SocketGuildChannel guildChannel = message.Channel as SocketGuildChannel;
						if(cmd.parent == null || cmd.parent.enabledForGuilds.Contains(guildChannel.Guild.Id))
						{
							await cmd.ExecuteCommand(message);
						}
						else
						{
							await message.Channel.SendMessageAsync("That module is disabled for this guild.");
						}
					}
					else
					{
						await message.Channel.SendMessageAsync("Unknown command, insect.");
					}
					return;
				}
				else
				{
					foreach(BotModule module in modules)
					{
						SocketGuildChannel guildChannel = message.Channel as SocketGuildChannel;
						if(module.enabledForGuilds.Contains(guildChannel.Guild.Id))
						{
							Task.Run( () => module.ListenTo(message));
						}
					}
				}
			} 	
		}
	}
}
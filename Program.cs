using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Nett;

namespace Agatha2
{

	internal abstract class BotModule
	{
        internal string moduleName;
        public List<BotCommand> commands;
        public string description;
        public abstract bool Register(List<BotCommand> commands);
		public abstract Task ListenTo(SocketMessage message);
		public abstract Task StartModule();
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

		private static bool _isAwake;
		private static string _commandPrefix;
		private static int _markovChance;
		private static string _token;
		private static ulong _streamChannelId;
		private static string _streamAPIClientID;

		public static string Token { get => _token; set => _token = value; }
		public static bool Awake { get => _isAwake; set => _isAwake = value; }
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
			return true;
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
			Token =             configTable.Get<string>("Token");
			SourceAuthor =      configTable.Get<string>("SourceAuthor");
			SourceVersion =     configTable.Get<string>("SourceVersion");
			SourceLocation =    configTable.Get<string>("SourceLocation");
			Awake =             configTable.Get<bool>("Awake");
			CommandPrefix =     configTable.Get<string>("CommandPrefix");
			MarkovChance =      configTable.Get<int>("MarkovChance");
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
			commands.Add(new CommandWakeup());
			commands.Add(new CommandShutup());
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
						await commandAliases[command].ExecuteCommand(message);
					}
					else
					{
						await message.Channel.SendMessageAsync("Unknown command, insect.");
					}
					return;
				}
				else if(Program.Awake)
				{
					foreach(BotModule module in modules)
					{
						await module.ListenTo(message);
					}
				}
			} 	
		}
	}
}
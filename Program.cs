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
using System.Globalization;

namespace Agatha2
{	
	internal class Program
	{
		internal static Random rand = new Random();
		private static DiscordSocketClient _client;
		internal static List<BotCommand> commands = new List<BotCommand>();
		internal static Dictionary<ulong, GuildConfig> guilds = new Dictionary<ulong, GuildConfig>();
		internal static Dictionary<string, BotCommand> commandAliases = new Dictionary<string, BotCommand>();
		internal static List<BotModule> modules = new List<BotModule>();
		internal static DiscordSocketClient Client {get => _client; set => _client = value; }
		internal static Dictionary<string, BotCypher> cyphers = new Dictionary<string, BotCypher>();

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

			cyphers.Add("profanityfilter", new CypherProfanityFilter());

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
			commands.Add(new CommandInterrogate());
			commands.Add(new CommandPingAPI());
			commands.Add(new CommandPoke());

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
			catch {}
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
			catch {}
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
								Task.Run(() => Program.SendReply(message, "That module is disabled for this guild."));
							}
						}
						else
						{
							Task.Run(() => Program.SendReply(message, $"Unknown command '{command}', insect."));
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

		internal static string ApplyCurrentCypher(string incoming, GuildConfig guild)
		{
			return (guild.useCypher != null && cyphers.ContainsKey(guild.useCypher)) ? ApplyCypher(incoming, guild.useCypher) : incoming;
		}

		internal static string ApplyCypher(string incoming, string cypher)
		{
			string outgoing = incoming;
			if(cyphers.ContainsKey(cypher))
			{
				BotCypher cypherObj = Program.cyphers[cypher];
				outgoing = cypherObj.ApplyPreSubstitution(outgoing);
				outgoing = cypherObj.ApplySubstition(outgoing);
				outgoing = cypherObj.ApplyPostSubstitution(outgoing);
			}
			return outgoing;
		}

		internal static async Task SendReply(SocketMessage replyingTo, string outgoing)
		{
			try
			{
				SocketGuildChannel guildChannel = replyingTo.Channel as SocketGuildChannel;
				await replyingTo.Channel.SendMessageAsync($"{replyingTo.Author.Mention}: {ApplyCurrentCypher(outgoing, Program.GetGuildConfig(guildChannel.Guild.Id))}");
			}
			catch {}
		}

		internal static async Task SendReply(IMessageChannel channel, string outgoing)
		{
			try
			{
				SocketGuildChannel guildChannel = channel as SocketGuildChannel;
				await channel.SendMessageAsync(ApplyCurrentCypher(outgoing, Program.GetGuildConfig(guildChannel.Guild.Id)));
			}
			catch {}
		}

		internal static async Task SendReply(IMessageChannel channel, string outgoing, EmbedBuilder embed)
		{
			try
			{
				SocketGuildChannel guildChannel = channel as SocketGuildChannel;
				GuildConfig guildConfig = Program.GetGuildConfig(guildChannel.Guild.Id);
				embed.Title = ApplyCurrentCypher(embed.Title, guildConfig);
				embed.Description = ApplyCurrentCypher(embed.Title, guildConfig);
				await channel.SendMessageAsync(ApplyCurrentCypher(outgoing, guildConfig), false, embed.Build());
			}
			catch {}
		}

		internal static async Task SendReply(IMessageChannel channel, EmbedBuilder embed)
		{
			try
			{
				SocketGuildChannel guildChannel = channel as SocketGuildChannel;
				GuildConfig guildConfig = Program.GetGuildConfig(guildChannel.Guild.Id);
				embed.Title = ApplyCurrentCypher(embed.Title, guildConfig);
				embed.Description = ApplyCurrentCypher(embed.Description, guildConfig);
				await channel.SendMessageAsync("", false, embed.Build());
			}
			catch {}
		}

		internal static async Task SendReply(SocketMessage replyingTo, string outgoing, EmbedBuilder embed)
		{
			try
			{
				SocketGuildChannel guildChannel = replyingTo.Channel as SocketGuildChannel;
				GuildConfig guildConfig = Program.GetGuildConfig(guildChannel.Guild.Id);
				embed.Title = ApplyCurrentCypher(embed.Title, guildConfig);
				embed.Description = ApplyCurrentCypher(embed.Title, guildConfig);
				await replyingTo.Channel.SendMessageAsync($"{replyingTo.Author.Mention}: {ApplyCurrentCypher(outgoing, guildConfig)}", false, embed.Build());
			}
			catch {}
		}
		internal static async Task SendReply(SocketMessage replyingTo, EmbedBuilder embed)
		{
			try
			{
				SocketGuildChannel guildChannel = replyingTo.Channel as SocketGuildChannel;
				GuildConfig guildConfig = Program.GetGuildConfig(guildChannel.Guild.Id);
				embed.Title = ApplyCurrentCypher(embed.Title, guildConfig);
				embed.Description = ApplyCurrentCypher(embed.Title, guildConfig);
				await replyingTo.Channel.SendMessageAsync($"{replyingTo.Author.Mention}:", false, embed.Build());
			}
			catch {}
		}
	}
}
using Discord;
using Discord.WebSocket;
using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;

namespace Agatha2
{
	internal class FishingHole
	{
		internal string holeName;
		internal string holeType;
		internal string holeId;
		internal string vNum;
		internal List<string> containsFish;
		internal FishingHole(string _name, string _type, string _vnum, List<string> _fish)
		{
				holeName = _name;
				holeType = _type;
				vNum = _vnum;
				containsFish = _fish;
		}
	}

	internal class ModuleAetolia : BotModule
	{
		internal ModuleAetolia()
		{
			moduleName = "Aetolia";
			description = "A character lookup and news-reading module for the IRE MUD Aetolia: The Midnight Age.";
		}

		private string fishDbPath = @"modules\aetolia\data\fish.db";
		internal List<FishingHole> fishingHoles;
		internal override void StartModule()
		{
			Console.WriteLine("Loading Aetolia fish database.");
			if(!File.Exists(fishDbPath))
			{
				Console.WriteLine($"No database found, creating an empty one at {fishDbPath}.");
				SQLiteConnection.CreateFile(fishDbPath);				
			}
			SQLiteConnection fishDbConnection = new SQLiteConnection($"Data Source={fishDbPath};Version=3;");
			fishDbConnection.Open();

			List<string> uniqueFish = new List<string>();
			Dictionary<string, List<string>> tmpFish = new Dictionary<string, List<string>>();
			SQLiteCommand command = new SQLiteCommand("SELECT * FROM fish_types;", fishDbConnection);
			SQLiteDataReader reader = command.ExecuteReader();
			while(reader.Read())
			{
				string fishName = reader["fishName"].ToString();
				string holeName = reader["fishingHoleName"].ToString();
				
				if(!uniqueFish.Contains(fishName))
				{
					uniqueFish.Add(fishName);
				}
				if(!tmpFish.ContainsKey(holeName))
				{
					tmpFish.Add(holeName, new List<string>());
				}
				tmpFish[holeName].Add(fishName);
			}
			command = new SQLiteCommand("SELECT * FROM fishing_holes;", fishDbConnection);
			reader = command.ExecuteReader();
			while(reader.Read())
			{
				string holeName = reader["fishingHoleName"].ToString();
				string holeType = reader["fishingHoleType"].ToString();
				string holeVnum = reader["fishingHoleVnum"].ToString();
				FishingHole fishHole = new FishingHole(holeName, holeType, holeVnum, tmpFish[holeName]);
				fishingHoles.Add(fishHole);
				fishHole.holeId = fishingHoles.Count.ToString();
			}
			Console.WriteLine($"Associated {uniqueFish.Count} fish with {fishingHoles.Count} fishing holes. Done.");
		}

		internal override bool Register(List<BotCommand> commands)
		{
			fishingHoles = new List<FishingHole>();
			commands.Add(new CommandNstat());
			commands.Add(new CommandReadnews());
			commands.Add(new CommandHonours());
			commands.Add(new CommandWho());
			commands.Add(new CommandFish());
			commands.Add(new CommandAltprompt());
			return true;
		}

		internal HttpWebResponse GetAPIResponse(string responseType)
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
		internal override void ListenTo(SocketMessage message)
		{
		}
	}
}
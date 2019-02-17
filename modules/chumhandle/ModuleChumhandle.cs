using Discord;
using Discord.WebSocket;
using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Agatha2
{
	internal class ModuleChumhandle : BotModule
	{
		private static Dictionary<string, List<string>> chumStrings;

		internal ModuleChumhandle()
		{
			moduleName = "Chumhandle";
			description = "Provides randomly generated Homestuck character profiles.";
		}
		internal override void StartModule()
		{
		}
		internal override bool Register(List<BotCommand> commands)
		{
			chumStrings = new Dictionary<string, List<string>>();
			chumStrings.Add("chum_prefix",	  new List<string>(File.ReadAllLines(@"modules\chumhandle\data\chumhandle_prefixes.txt")));
			chumStrings.Add("chum_suffix",	  new List<string>(File.ReadAllLines(@"modules\chumhandle\data\chumhandle_suffixes.txt")));
			chumStrings.Add("chum_aspect",	  new List<string>(File.ReadAllLines(@"modules\chumhandle\data\chumhandle_aspects.txt")));
			chumStrings.Add("chum_class",	   new List<string>(File.ReadAllLines(@"modules\chumhandle\data\chumhandle_classes.txt")));
			chumStrings.Add("chum_lands",	   new List<string>(File.ReadAllLines(@"modules\chumhandle\data\chumhandle_lands.txt")));
			chumStrings.Add("consort_colour",   new List<string>(File.ReadAllLines(@"modules\chumhandle\data\consort_colours.txt")));
			chumStrings.Add("consort_interest", new List<string>(File.ReadAllLines(@"modules\chumhandle\data\consort_interests.txt")));
			chumStrings.Add("consort_quirk",	new List<string>(File.ReadAllLines(@"modules\chumhandle\data\consort_quirks.txt")));
			chumStrings.Add("consort_type",	 new List<string>(File.ReadAllLines(@"modules\chumhandle\data\consort_types.txt")));
			commands.Add(new CommandChum());
			return true;
		}

		internal string GetRand(string from)
		{
			return chumStrings[from][Program.rand.Next(chumStrings[from].Count)];
		}

		internal List<string> GetMultiRand(string from, int count)
		{
			int n = chumStrings[from].Count;  
			while(n > 1) 
			{
				n--;  
				int k = Program.rand.Next(n + 1);  
				string value = chumStrings[from][k];  
				chumStrings[from][k] = chumStrings[from][n];  
				chumStrings[from][n] = value;  
			}  
			List<string> results = new List<string>();
			for(int i = 0;i<count;i++)
			{
				results.Add(chumStrings[from][i]);
			}
			return results;
		}
		internal override void ListenTo(SocketMessage message)
		{
		}
	}
}
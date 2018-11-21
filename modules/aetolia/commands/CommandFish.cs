using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SQLite;

/*
 * Corresponding Mudlet alias for this command:

local dumpPath = getMudletHomeDir() .. "/fishdb.txt"
local dumpFile = io.open(dumpPath, "w")

dumpFile:write("DROP TABLE fishing_holes;\nDROP TABLE fish_types;")
dumpFile:write("\nCREATE TABLE fishing_holes (fishingHoleName TEXT PRIMARY_KEY, fishingHoleType TEXT NOT NULL, fishingHoleVnum TEXT NOT NULL);")
dumpFile:write("\nCREATE TABLE fish_types    (fishName TEXT NOT NULL, fishingHoleName TEXT NOT NULL);")
saved_data.fishing_holes = saved_data.fishing_holes or {}
for k,v in pairs(saved_data.fishing_holes) do
	dumpFile:write("\n\nINSERT INTO fishing_holes (fishingHoleName, fishingHoleType, fishingHoleVnum) VALUES ('" .. v.name:gsub("'","''") .. "' ,'" .. v.type:gsub("'","''") .. "', '" .. v.rooms[1] .. "');")
	for _,fishie in pairs(v.fish) do
		dumpFile:write("\nINSERT INTO fish_types (fishName, fishingHoleName) VALUES ('" .. fishie:gsub("'","''") .. "', '" .. v.name:gsub("'","''") .. "');")
	end
end
dumpFile:close()
echo("\nDumped fishing database to SQL at path " .. dumpPath .. ".")

 */
namespace Agatha2
{
	internal class CommandFish : BotCommand
	{
        public CommandFish()
        {
            usage = "fish <string to search for>";
            description = "Shows information about Aetolian fishing holes.";
            aliases = new List<string>(new string[] {"fish", "fsearch"});
        }


		private bool TryAddMatch(Dictionary<FishingHole, string> matches, FishingHole fishingHole, string fishy, string searchText)
		{
			int foundAt = fishy.ToLower().IndexOf(searchText);
			if(foundAt != -1)
			{
				if(!matches.ContainsKey(fishingHole))
				{
					matches.Add(fishingHole, $"{fishy.Substring(0, foundAt)}**{fishy.Substring(foundAt, searchText.Length)}**{fishy.Substring(foundAt + searchText.Length)}");
				}
				return true;
			}
			return false;
		}

        public override async Task ExecuteCommand(SocketMessage message)
        {
			string[] message_contents = message.Content.Substring(1).Split(" ");
			if(message_contents.Length < 2)
			{
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: Usage: {usage}");
			}
			else
			{
				Dictionary<FishingHole, string> matches = new Dictionary<FishingHole, string>();
				string searchText = message.Content.Substring(message_contents[0].Length+2).ToLower();
				ModuleAetolia aetolia = (ModuleAetolia)parent;
				foreach(FishingHole fishHole in aetolia.fishingHoles)
				{
					if(!TryAddMatch(matches, fishHole, fishHole.holeName, searchText) && !TryAddMatch(matches, fishHole, fishHole.holeType, searchText))
					{
						foreach(string fishy in fishHole.containsFish)
						{
							if(TryAddMatch(matches, fishHole, fishy, searchText))
							{
								break;
							}
						}
					}
				}

				string result = $"No matches found.";
				if(matches.Count != 0)
				{
					result = "Matches:";
					foreach(KeyValuePair<FishingHole, string> fishHole in matches)
					{
						result = $"{result}\n{fishHole.Key.holeName} - {fishHole.Key.holeType} - v{fishHole.Key.vNum} [{fishHole.Value.ToString()}]";
					}
				}
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: {result}");
			}
        }
    }
}
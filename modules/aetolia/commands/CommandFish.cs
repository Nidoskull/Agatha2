using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Linq;

/*
 * Corresponding Mudlet alias for this command:

local dumpPath = getMudletHomeDir() .. "/fishdb.txt"
local dumpFile = io.open(dumpPath, "w")

dumpFile:write("DROP TABLE fishing_holes;\nDROP TABLE fish_types;")
dumpFile:write("\nCREATE TABLE fishing_holes (fishingHoleName TEXT PRIMARY_KEY, fishingHoleType TEXT NOT NULL, fishingHoleVnum TEXT NOT NULL);")
dumpFile:write("\nCREATE TABLE fish_types	(fishName TEXT NOT NULL, fishingHoleName TEXT NOT NULL);")
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
		internal CommandFish()
		{
			usage = "fish <string to search for>";
			description = "Shows information about Aetolian fishing holes.";
			aliases = new List<string>() {"fish", "fsearch"};
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

		internal override async Task ExecuteCommand(SocketMessage message, GuildConfig guild)
		{
			string[] message_contents = message.Content.Substring(1).Split(" ");
			if(message_contents.Length < 2)
			{
				await Program.SendReply(message, $"Usage: {usage}");
			}
			else
			{
				EmbedBuilder embedBuilder = new EmbedBuilder();
				embedBuilder.Title = "Search Results";
				Dictionary<FishingHole, string> matches = new Dictionary<FishingHole, string>();
				string searchText = message.Content.Substring(message_contents[0].Length+2).ToLower();
				ModuleAetolia aetolia = (ModuleAetolia)parent;
				foreach(FishingHole fishHole in aetolia.fishingHoles)
				{
					if(fishHole.holeId.Equals(searchText))
					{
						matches.Add(fishHole, "");
						break;
					}
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

				if(matches.Count == 0)
				{
					embedBuilder.Description = "No matches found.";
				}
				else if(matches.Count == 1)
				{
					KeyValuePair<FishingHole, string> fishHole = matches.First();
					embedBuilder.Title = fishHole.Key.holeName;
					embedBuilder.AddField("Type", fishHole.Key.holeType);
					embedBuilder.AddField("Vnum", fishHole.Key.vNum);
					embedBuilder.AddField("Fish", string.Join(", ", fishHole.Key.containsFish.ToArray()));
				}
				else
				{
					string fishResults = "Multiple matches found:\n";
					foreach(KeyValuePair<FishingHole, string> fishHole in matches)
					{
						fishResults = $"{fishResults}\n{fishHole.Key.holeId}. {fishHole.Key.holeName} - {fishHole.Key.holeType} - v{fishHole.Key.vNum} [{fishHole.Value.ToString()}]";
					}
					embedBuilder.Description = $"{fishResults}\n\nSpecify an ID number or a more specific search string for detailed information on a fishing hole.";
				}
				await Program.SendReply(message, embedBuilder);
			}
		}
	}
}
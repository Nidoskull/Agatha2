using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Agatha2
{
	internal class CommandAltprompt : BotCommand
	{
		public CommandAltprompt()
		{
			usage = "altprompt";
			description = "Builds a randomly generated alt to play.";
			aliases = new List<string>() {"altprompt", "alt"};
		}

		private List<string> Shuffle(List<string> shuffling)
		{
			int n = shuffling.Count;  
			while (n > 1) {  
				n--;  
				int k = Program.rand.Next(n + 1);  
				string value = shuffling[k];  
				shuffling[k] = shuffling[n];  
				shuffling[n] = value;  
			}  
			return shuffling;
		}
		public override async Task ExecuteCommand(SocketMessage message)
		{
			EmbedBuilder embedBuilder = new EmbedBuilder();
			string aetHis = "Their";
			string aethis = "their";
			string aetHe =  "They";
			string aethe =  "they";
			string aethim = "them";
			string aetIs =  "are";
			string aetLives = "live";

			List<string> _genders = new List<string>() {"a male", "a female", "an androgynous", "a genderfluid"};
			string aetGender = _genders[Program.rand.Next(4)];
			if(aetGender.Equals("a female"))
			{
				aetHis = "Her";
				aethis = "her";
				aetHe =  "She";
				aethe =  "she";
				aethim = "her";
				aetIs =  "is";
				aetLives = "lives";
			}
			else if(aetGender.Equals("a male"))
			{
				aetHis = "His";
				aethis = "his";
				aetHe =  "He";
				aethe =  "he";
				aethim = "him";
				aetIs =  "is";
				aetLives = "lives";
			}

			List<string> _races = new List<string>() {"Arqeshi","Atavian","Dwarf","Grecht","Grook","Human","Horkval","Imp","Kelki","Kobold","Mhun","Nazetu","Ogre","Orc","Rajamala","Troll","Tsol'aa","Xoran"};
			string aetRace = _races[Program.rand.Next(_races.Count)];

			List<string> _regions = new List<string>() {"Mournhold", "Arbothia", "Tasur'ke", "Siha Dylis", "Tainhelm", "Kentorakro", "Stormcaller Crag", "Jaru", "the wilderness", "the eastern isles", "another continent", "another plane", "the western isles", "the deep forests", "the tundra", "the mountains", "the desert", "the southern coast", "the eastern coast", "the western coast", "beneath the hills", "a bustling town"};
			string aetRegion = _regions[Program.rand.Next(_regions.Count)];

			List<string> _backgrounds = new List<string>() {"an outcast", "a noble", "a farmer", "a hunter", "a guard", "a tracker", "a serf", "a slave", "a trader", "a feral child", "a nomad", "an apprentice smith", "an unwanted burden"};
			string aetBackground = _backgrounds[Program.rand.Next(_backgrounds.Count)];

			List<string> _cities = new List<string>() {"in Spinesreach", "in Enorian", "in Duiran", "in Bloodloch", "in Delve", "in Esterport", "nowhere in particular"};
			string aetCity = _cities[Program.rand.Next(_cities.Count)];
			List<string> _guilds;
			switch(aetCity)
			{
				case "in Spinesreach":
					_guilds = new List<string>() {"Archivists", "Sciomancers", "Syssin", "Praenomen"};
					break;
				case "in Enorian":
					_guilds = new List<string>() {"Ascendril", "Templars", "Illuminai"};
					break;
				case "in Bloodloch":
					_guilds = new List<string>() {"Carnifex", "Indorani", "Teradrim", "Praenomen"};
					break;
				case "in Duiran":
					_guilds = new List<string>() {"Shamans", "Sentinels", "Monks"};
					break;
				default: 
					_guilds = new List<string>() {"Archivists", "Sciomancers", "Syssin", "Ascendril", "Templars", "Illuminai", "Carnifex", "Indorani", "Teradrim", "Praenomen", "Shamans", "Sentinels", "Monks"};
					break;
			}
			string aetGuild = _guilds[Program.rand.Next(_guilds.Count)];

			string otherClass = "";
			if(Program.rand.Next(4) == 4)
			{
				List<string> _otherClasses = new List<string>() {"Werecroc", "Wereboar", "Wereraven", "Werebear", "Werewolf", "Wayfarer"};
				otherClass = $" However, despite {aethis} guild membership, {aethe} {aetIs} actually a {_otherClasses[Program.rand.Next(_otherClasses.Count)]}.";
			}

			List<string> _interests = new List<string>() {"architecture", "art", "beauty", "friendship", "gadgetry", "knowledge", "magic", "mining", "money", "nature", "politics", "power", "stories", "tomfoolery", "gemstones", "science", "exploration", "adventure", "agriculture", "pranks", "music", "dancing", "swimming", "philosophy"};
			_interests = Shuffle(_interests);
			string aetInterestOne =	_interests[0];
			string aetInterestTwo =	_interests[1];
			string aetInterestThree =  _interests[2];

			List<string> _personality = new List<string>() {"agreeable", "neurotic", "calm", "foolish", "focused", "witty", "blunt", "dull", "arrogant", "awesome", "depressed", "easily distracted", "eccentric", "gullible", "hyperactive", "insulting", "irrational", "joyful", "lazy", "mysterious", "shy", "stupid", "violent", "warlike", "weird", "naive", "timid", "rambunctious", "swindling", "helpful", "brazen", "neurotic", "impressionable", "blithe", "forgetful", "uncoordinated"};
			_personality = Shuffle(_personality);
			string aetPersonalityOne =   _personality[0];
			string aetPersonalityTwo =   _personality[1];
			string aetPersonalityThree = _personality[2];

			string result = $"You should play {aetGender} {aetRace} from {aetRegion}.";
			result = $"{result}\n{aetHe} grew up as {aetBackground} and now {aetLives} {aetCity} as a member of the {aetGuild}.{otherClass}";
			result = $"{result}\n{aetHe} {aetIs} interested in {aetInterestOne}, {aetInterestTwo} and {aetInterestThree}.";
			result = $"{result}\n{aetHis} friends describe {aethim} as {aetPersonalityOne}, {aetPersonalityTwo} and {aetPersonalityThree}.";  
			embedBuilder.Description = result;
			await message.Channel.SendMessageAsync($"{message.Author.Mention}:", false, embedBuilder);		
		}
	}
}
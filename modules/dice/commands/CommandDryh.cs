using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Agatha2
{
	internal class CommandDryh : BotCommand
	{
        public CommandDryh()
        {
            usage = "dryh <Discipline #> <Exhaustion #> <Madness #> <Pain #>";
            description = "Rolls dice in the Don't Rest Your Head schema.";
            aliases = new List<string>(new string[] {"dryh"});
        }
        public override async Task ExecuteCommand(SocketMessage message)
        {
			EmbedBuilder embedBuilder = new EmbedBuilder();
			string responseMessage =  "";
			Match m = Regex.Match(message.Content, "(\\d+) (\\d+) (\\d+) (\\d+)");
			if(m.Success)
			{
				List<DicePool> pools = new List<DicePool>();
				pools.Add(new DicePool($"{m.Groups[1].ToString()}d6", "Discipline"));
				pools.Add(new DicePool($"{m.Groups[2].ToString()}d6", "Exhaustion"));
				pools.Add(new DicePool($"{m.Groups[3].ToString()}d6", "Madness"));
				DicePool painPool = new DicePool($"{m.Groups[4].ToString()}d6", "Pain");

				int playerSuccess = 0;
				DicePool dominantPool = pools[0];

				foreach(DicePool entry in pools)
				{
					int success = entry.CountAtOrBelow(3);
					playerSuccess += success;
					if(dominantPool.HighestValue() < entry.HighestValue())
					{
						dominantPool = entry;
					}
					embedBuilder.AddField($"{entry.Label}", $"{entry.SummarizePoolRoll(0)} = {success}");
				}
				embedBuilder.AddField($"{painPool.Label}", $"{painPool.SummarizePoolRoll(0)} = {painPool.CountAtOrBelow(3)}");

				if(painPool.HighestValue() > dominantPool.HighestValue())
				{
					dominantPool = painPool;
				}
				string winString = (playerSuccess >= painPool.CountAtOrBelow(3)) ? "wins" : "loses";
				string successNoun = (playerSuccess == 1) ? "success" : "successes";
				embedBuilder.Description = $"**The player {winString}** the conflict with {playerSuccess} {successNoun} versus {painPool.CountAtOrBelow(3)}. **{dominantPool.Label} dominates** with a {dominantPool.HighestValue()}.";
				await message.Channel.SendMessageAsync($"{message.Author.Mention}:", false, embedBuilder);		
			}
			else
			{
				await message.Channel.SendMessageAsync($"{message.Author.Mention}: Dice syntax is `{Program.CommandPrefix}dryh [Discipline] [Exhaustion] [Madness] [Pain]`.");		
			}
        }
    }
}
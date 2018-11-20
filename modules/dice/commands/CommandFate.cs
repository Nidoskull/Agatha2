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
	internal class CommandFate : BotCommand
	{
        private static Dictionary<string, string> fateTiers;
        public CommandFate()
        {
            usage = "fate <+/->#";
            description = "Rolls dice in the FATE schema.";
            aliases = new List<string>(new string[] {"fate"});
            fateTiers = new Dictionary<string, string>();
            fateTiers.Add("-2", "Terrible...");
            fateTiers.Add("-1", "Poor.");
            fateTiers.Add("+0", "Mediocre.");
            fateTiers.Add("+1", "Average.");
            fateTiers.Add("+2", "Fair.");
            fateTiers.Add("+3", "Good.");
            fateTiers.Add("+4", "Great!");
            fateTiers.Add("+5", "Superb!");
            fateTiers.Add("+6", "Fantastic!");
            fateTiers.Add("+7", "Epic!");
        }
        public override async Task ExecuteCommand(SocketMessage message)
        {
			int mod = 0;
            Match m = Regex.Match(message.Content, "([-+])(\\d+)");
			if(m.Success)
			{
                mod = Convert.ToInt32(m.Groups[2].ToString());
                if(m.Groups[1].ToString().Equals("-"))
                {
                    mod = -(mod);
                }
            }

            DicePool dice = new DicePool($"4d3", "FATE");
            int resultValue = (dice.CountAtOrAbove(3) - dice.CountAtOrBelow(1)) + mod;
            string modString = (resultValue >= 0) ? $"+{mod}" : $"{mod}";
            string descriptiveResult;
            if(resultValue > 7)
            {
                descriptiveResult = $"+{resultValue}, Legendary!";
            } 
            else if(resultValue < -2)
            {
                descriptiveResult = $"{resultValue}, Abysmal...";
            }
            else
            {
                string resultKey = (resultValue < 0) ? $"{resultValue}" : $"+{resultValue}";
                descriptiveResult = $"{resultKey}, {fateTiers[resultKey]}";
            }
			await message.Channel.SendMessageAsync($"{message.Author.Mention}: ```{dice.SummarizePoolRoll(-2)} ({modString}) = {descriptiveResult}```");
        }
    }
}
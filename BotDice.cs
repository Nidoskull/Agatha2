using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Agatha2
{

    internal static class BotDice
    {

        private static Dictionary<string, string> fateTiers = new Dictionary<string, string>();

        static BotDice()
        {
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
		public static string dieRegexString = "(\\d*)(#*)d(\\d+)([+-]\\d+)*";

		public static async Task RollDiceDRYH(SocketMessage message)
		{
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
				string poolSummary = "";
				DicePool dominantPool = pools[0];

				foreach(DicePool entry in pools)
				{
					int success = entry.CountAtOrBelow(3);
					playerSuccess += success;
					if(dominantPool.HighestValue() < entry.HighestValue())
					{
						dominantPool = entry;
					}
					poolSummary = $"{poolSummary}```{entry.SummarizePoolRoll(0)} = {success}```\n";
				}
				poolSummary = $"{poolSummary}```{painPool.SummarizePoolRoll(0)} = {painPool.CountAtOrBelow(3)}```\n";

				if(painPool.HighestValue() > dominantPool.HighestValue())
				{
					dominantPool = painPool;
				}
				string winString = (playerSuccess >= painPool.CountAtOrBelow(3)) ? "wins" : "loses";
				string successNoun = (playerSuccess == 1) ? "success" : "successes";
				responseMessage = $"{poolSummary}**The player {winString}** the conflict with {playerSuccess} {successNoun} versus {painPool.CountAtOrBelow(3)}. **{dominantPool.Label} dominates** with a {dominantPool.HighestValue()}.";
			}

			if(responseMessage.Equals(""))
			{
				responseMessage = $"Dice syntax is `{Program.Config.CommandPrefix}dryh [Discipline] [Exhaustion] [Madness] [Pain]`.";
			}
			await message.Channel.SendMessageAsync($"{message.Author.Mention}: {responseMessage}");			
		}

        public static async Task RollDiceFate(SocketMessage message)
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

		public static async Task RollDiceStandard(SocketMessage message)
		{
			string responseMessage =  "";
			foreach(Match m in Regex.Matches(message.Content.Substring(6), dieRegexString))
			{
				DicePool dice = new DicePool(m);
				responseMessage = $"{responseMessage}```{dice.SummarizeStandardRoll()}```\n";
			}
			if(responseMessage.Equals(""))
			{
				responseMessage = $"Dice syntax is `{Program.Config.CommandPrefix}roll [1-100]d[1-100]<+/-[modifier]> <reroll/explode [value]>` separated by spaces or commas. Separate dice count from number of sides with `#` for individual rolls.";
			}
			await message.Channel.SendMessageAsync($"{message.Author.Mention}: {responseMessage}");			
		}

    }

	internal class Die  
	{
        private int sides;
        private int result;
        public int Result { get => result; set => result = value; }

        public Die(int _sides)
        {
            sides = BotUtilities.Clamp(_sides, 1, 100);
            Roll();
        }
        public void Roll()
        {
            result = BotUtilities.rnjesus.Next(0, sides)+1;
        }
    }

    internal class DicePool
    {
        private List<Die> dice = new List<Die>();
        private int modifier = 0;
        private bool individualRolls = false;
        private string label;

        public string Label { get => label; set => label = value; } 

        public DicePool(string _input, string _label)
        {
            label = _label;
            ParseDice(Regex.Match(_input, BotDice.dieRegexString));
        }

        public DicePool(Match match)
        {
            label = match.Groups[0].ToString();
            ParseDice(match);
        }

        private void ParseDice(Match m)
        {
			string dieMatch = m.Groups[0].ToString();
			string dieCountField = m.Groups[1].ToString();
			string dieSidesField = m.Groups[3].ToString();
			string dieModifierField = m.Groups[4].ToString();
			int dieSides = BotUtilities.Clamp(Convert.ToInt32(dieSidesField), 1, 100);

			individualRolls = m.Groups[2].ToString().Equals("#");

			if(dieCountField.Equals("")) 
			{
                dice.Add(new Die(dieSides));
            }
            else
            {
				try
				{
                    for(int i = 1; i <= BotUtilities.Clamp(Convert.ToInt32(dieCountField), 1, 100); i++)
                    {
                        dice.Add(new Die(dieSides));
                    }
				}
				catch
				{
					dice.Add(new Die(dieSides));
				}
			}

			if(!dieModifierField.Equals(""))
			{
				try
				{
					bool positiveMod = false;
					if(dieModifierField.StartsWith('+'))
					{
						positiveMod = true;
					}
					dieModifierField = dieModifierField.Substring(1);
					modifier = Convert.ToInt32(dieModifierField);
					if(!positiveMod)
					{
						modifier = -(modifier);
					}
				}
				catch
				{
					modifier = 0;
				}
			}
        }

        public int CountAtOrBelow(int value)
        {
            int total = 0;
            foreach(Die die in dice)
            {
                if(die.Result <= value)
                {
                    total++;
                }
            }
            return total;
        }

        public int CountAtOrAbove(int value)
        {
            int total = 0;
            foreach(Die die in dice)
            {
                if(die.Result >= value)
                {
                    total++;
                }
            }
            return total;
        }

        public int HighestValue()
        {
            int total = 0;
            foreach(Die die in dice)
            {
                if(die.Result >= total)
                {
                    total = die.Result;
                }
            }
            return total;
        }

        public string SummarizePoolRoll(int offset)
        {
            return SummarizeRoll(false, offset);
        }

        public string SummarizeStandardRoll()
        {
            return SummarizeRoll(true, 0);
        }

        private string SummarizeRoll(bool show_total, int offset_results)
        {   
			string resultString = $"{label} [ ";
            int total = 0;

			foreach(Die die in dice)
            {
                resultString = $"{resultString}{die.Result + offset_results} ";
                total += die.Result + offset_results;
                if(individualRolls && modifier != 0)
                {
                    resultString = $"{resultString}({die.Result + offset_results + modifier}) ";
                    total += modifier;
                }
            }
            if(!individualRolls)
            {
                total += modifier;
            }
            return show_total ? $"{resultString}] = {total}" : $"{resultString}]";
        }
    }
}
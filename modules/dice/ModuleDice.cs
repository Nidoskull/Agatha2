using Discord;
using Discord.WebSocket;
using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Agatha2
{
	internal class ModuleDice : BotModule
	{
        public ModuleDice()
        {
            moduleName = "Dice";
            description = "Provides several dice-rolling functions.";
        }
        public override bool Register(List<BotCommand> commands)
        {
            commands.Add(new CommandFate());
            commands.Add(new CommandDryh());
            commands.Add(new CommandRoll());
            return true;
        }
		public override async Task ListenTo(SocketMessage message)
		{
        }
    }

	internal class Die  
	{
        private int sides;
        private int result;
        public int Result { get => result; set => result = value; }

        public Die(int _sides)
        {
            sides = Program.Clamp(_sides, 1, 100);
            Roll();
        }
        public void Roll()
        {
            result = Program.rand.Next(0, sides)+1;
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
            ParseDice(Regex.Match(_input, "(\\d*)(#*)d(\\d+)([+-]\\d+)*"));
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
			int dieSides = Program.Clamp(Convert.ToInt32(dieSidesField), 1, 100);

			individualRolls = m.Groups[2].ToString().Equals("#");

			if(dieCountField.Equals("")) 
			{
                dice.Add(new Die(dieSides));
            }
            else
            {
				try
				{
                    for(int i = 1; i <= Program.Clamp(Convert.ToInt32(dieCountField), 1, 100); i++)
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
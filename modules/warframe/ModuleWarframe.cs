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
using System.Threading;

namespace Agatha2
{
	internal class ModuleWarframe : BotModule
	{
        private static List<string> hekPostStrings;
		private static List<string> ordisPostStrings;
		private static List<string> vorPostStrings;

        public ModuleWarframe()
        {
            moduleName = "Warframe";
            description = "A pointless module for interjecting Warframe quotes into innocent conversations.";
        }

		public override async Task StartModule()
		{
		}
        public override bool Register(List<BotCommand> commands)
        {
            vorPostStrings =   new List<string>(File.ReadAllLines("data/vor_strings.txt"));
			hekPostStrings =   new List<string>(File.ReadAllLines("data/hek_strings.txt"));
			ordisPostStrings = new List<string>(File.ReadAllLines("data/ordis_strings.txt"));
            return true;
		}
		public override async Task ListenTo(SocketMessage message)
		{
        	string searchSpace =  message.Content.ToLower();
            if(searchSpace.Contains("hek"))
            {
                await message.Channel.SendMessageAsync(hekPostStrings[Program.rand.Next(hekPostStrings.Count)]);
            }
            else if(searchSpace.Contains("operator"))
            {
                await message.Channel.SendMessageAsync(ordisPostStrings[Program.rand.Next(ordisPostStrings.Count)]);
            }
            else if(searchSpace.Contains("look at them"))
            {
                await message.Channel.SendMessageAsync(vorPostStrings[Program.rand.Next(vorPostStrings.Count)]);	
            }
        }
    }
}
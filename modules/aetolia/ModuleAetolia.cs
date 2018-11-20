using Discord;
using Discord.WebSocket;
using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agatha2
{
	internal class ModuleAetolia : BotModule
	{
        public ModuleAetolia()
        {
            moduleName = "Aetolia";
            description = "A character lookup and news-reading module for the IRE MUD Aetolia: The Midnight Age.";
        }
        public override bool Register(List<BotCommand> commands)
        {
            commands.Add(new CommandNstat());
            commands.Add(new CommandReadnews());
            commands.Add(new CommandHonours());
            commands.Add(new CommandWho());
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
		public override async Task ListenTo(SocketMessage message)
		{
        }
    }
}
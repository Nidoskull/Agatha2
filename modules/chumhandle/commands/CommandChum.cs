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

namespace Agatha2
{
	internal class CommandChum : BotCommand
	{
        public CommandChum()
        {
            usage = "chum";
            description = "Creates a random Homestuck character, including land and consorts.";
            aliases = new List<string>(new string[] {"chum"});
        }
        public override async Task ExecuteCommand(SocketMessage message)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            ModuleChumhandle chum = (ModuleChumhandle)parent;
			string result = $"Your chumhandle is **{chum.GetRand("chum_prefix")}{chum.GetRand("chum_suffix")}**.";
			List<string> landTerms = chum.GetMultiRand("chum_lands", 2);
			result = $"{result}\nIn the **Land of {landTerms[0]} and {landTerms[1]}**, you are the **{chum.GetRand("chum_class")} of {chum.GetRand("chum_aspect")}**. It's you.";
			result = $"{result}\nThe Consorts of your land are **{chum.GetRand("consort_quirk")} {chum.GetRand("consort_colour")} {chum.GetRand("consort_type")}** who like **{chum.GetRand("consort_interest")}**.";
			embedBuilder.Description = result;
            await message.Channel.SendMessageAsync($"{message.Author.Mention}:", false, embedBuilder);
        }
    }
}
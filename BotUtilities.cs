using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ProtoBuf;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Agatha2
{

	internal static class BotUtilities 
	{

		private static Dictionary<string, List<string>> _bartending;
		private static bool _hasDictionaryChanged = false;
		private static List<string> validDrinkFields = new List<string>(new string[] {"vessel","beverage","garnish"});

		public static Random rnjesus = new Random();
		public static Dictionary<string, List<string>> BartendingData { get => _bartending; set => _bartending = value; }
		public static bool HasDictionaryChanged { get => _hasDictionaryChanged; set => _hasDictionaryChanged = value; }
		private static List<string> hekPostStrings;
		private static List<string> ordisPostStrings;

		public static int Clamp(int value, int min, int max)  
		{  
			return (value < min) ? min : (value > max) ? max : value;  
		}

		static BotUtilities() 
		{

			hekPostStrings = new List<string>() {
				"WHY are these FOOLS still breathing **MY AIR**!?",
				"**THIS IS UNACCEPTABLE**!",
				"**MUST I DO EVERYTHING MYSELF**?!",
				"I knew this day would come. I prepared, changed myself. I made sacrifices! Tenno... are you ready for a **SACRIFICE**?!",
				"The first Grineer... who brings me the head of a Tenno... will be permitted... **TO WEAR ITS SKULL AS A HELMET**!",
				"Tick, tock. Tick, tock. **TICK, TOCK, TENNO**!",
				"Oh, look what we have here! More fertilizer for our Grineer gardens! Your worm-bloated corpse will blend nicely with the excrement on my crops!",
				"It's... beautiful... it's so beautiful... **KILL IT**!",
				"You have not won, maggots... **YOU HAVE NOT WON**!",
				"Keep trying to cleanse these Plains! You'll only die... in a shower... of feculent... **DEFEAT**!",
				"My favourite ghoul is... **THE ONE THAT RIPS YOUR MAGGOT HEAD OFF!**",
				"Ghouls, wake! Time to play. Time... **TO KILL**.",
				"My ghouls kill to live, and live to kill. And kill, **AND KILL**!",
				"Come out, come out, come out! Meet your **NEW FRIENDS**! It’s **SLAYTIME**!",
				"Like all parents I want a future for my children. As long as that future involves **DEAD TENNO MAGGOTS**!",
				"My ghouls came from bags. YOU! **WILL LEAVE IN THEM**!",
				"Ah, I love that new ghoul smell, fresh from the bag."
			};

			ordisPostStrings = new List<string>() {
				"Operator? Ordis wonders... what are you thinking about?",
				"I've been thinking, Operator...I thought you'd want to know.",
				"Operator, I hope you are comfortable? No... we do not seem to have any seats.",
				"Everything in Ordis, Operator? **I̛͕̝̹ͅs͍ ̢͕͍̱̟̼t͡ḩ͉a͎̼̺t̺̯́ a ̼̘̼̺̺̪P̤̦̼͜U͓͍͕͚̳̠͍N̨̪̼̖ͅ?!** Hmm.... I will attempt to bypass this fault.",
				"Ordis has been counting stars, Operator. All accounted for.",
				"Operator, I will never betray you. I will keep the Orbiter hidden in the void. You can count on me!",
				"Operator, I've been thinking. My misplaced memories and damaged communication systems. What if...Ordis did those things?",
				"Operator, were you visualizing a bloody battle? **M͏̹͎̙ę͖̺̗ ̭̻̻̰t̺̖̖͚̥͚o͘o̹͈͖̝̳**!",
				"Ordis is happ**a̪n̥̯̫̬̘̪͉͡g͚͉͕͎̰̻̻͘r̬̼̟͇͡ỵ̟̟̀**. Hmm, I may require maintenance after all.",
				"Operator! Did you hear that? It said: *kssshhhshhhshhhhhhshhshshht*. Cosmic background radiation is a riot!",
				"Stand by while I analyze the intelligence profile of the Grineer. Error, not a number! Did the Operator enjoy this witticism?",
				"Do you remember the Old War, Operator? Ordis seems to have... misplaced those memories.",
				"**D̥̼̫̭̥̕o̮ n͎͘o̳̰̤̙͟ͅt̫̙͍͖̥́ͅͅ ͎͙͇̱li̶̹͎̖f͈̻̝̫ͅt͇̗̰͙̳̟͞ ̯͔t҉̱̖̝ͅh͚͇͈̜ḙ̞̬̬̮̮̜ ̤̺̝̺͇͎͎v̩͔̤ͅe̲͎̖̞̼̱̮i̝̦̮̠̭̖l͇̬͜.҉̣͔̹̰̯ ͓̹̺̕D̙̫̖o ̸̩͈͖̩͖n̙͖͉ot̷͍͎̳̹ ̧sh͏͍͚̮̯ͅó̜̲̻̲w͖͇̪͉͕͇̝ ̯̫̲͔̙̝t̝̠̥ͅh͈̞̭̼͉̪e̞͞ ͜d̺̼o̧̟͈̜͍̝̱̯o̝̼̘͘r̥̻̜̼̲̫͝.̡ D̩o̤̺̥̞ͅ ̕n̘̯̼͡ot͔̳ͅ ̷͕ͅs͚͉͇̮̙͟p̻̹̰͘l̜̱̳͎̰̙̠i̴͕͇̮̤ͅt͓͚͢ t̸͕̜̫h̕e͟ ̭̹͓ͅḓ̙̫͇̥͈ͅr̩̯͍̤e̯̥̣͈áͅm.̭̙̲͍̗**",
				"**̷̜̞̭̹̪̳M̕a̩̩̣̝͡i͙̳͉ṇ̼̙͙͉͖t͏ai̞ͅn͚̲̗̲̩ͅ ̥̱̞t̀h̰͕̖̰͉̜̝e ͕̠͉͓͎h̻̖̝ab̼̖̲̼̜̝i̷̪͇̣ṭ̢͙̲̲a̺̻̝̝̥ț̖̫̲͉̖ͅ.̥̰͖̻̥ ̠͉͓̣͕̰̖͠Ṃ̪̥̰ͅa̴͎̮͇̻̠͖̞i̩̥̠n̫̠̖̰̲̞t̢̥̣̥͉̲̤a̧̺̖͓̺̞i̯̫n͕̪͎̩ ̸͎̖t̰h̳̩ȩ̙̳͙̮͔ ̩͖̥̖̬͇͕͘O͖̼̖p̡̠̱e̴͎͈r̮̻̰̯͍̝a̭̝t̟̫̯̠͉o̘̰͇̼̥͢r̼̮.͇̪̼̗̦ ͈͠M̵̬̲̥̜̫͇̤o̝̗̤̰͖b҉̤i̼̜̦͍ͅl̸i̘̦͚ẓ̫e͙̱̭̙͕̠͙ ͜t͍̺̖͇͇̘̯h̛̤̯͈̲̰͓e̘̣ ̡͉͍̳̼T͎̺̣̣̻e̗̗̥͢n̛̗̪͔̘̖͔̰n̞̯͈o̖͈̲͖̱͢ͅ.̦͍**",
				"**̝͕̺̤̞Y̝̖͕̣͠o̵̟̤͎̣̖u̖ ҉̜a̲͈̭̭̙͕͓r̜͍͈̭̭e̶͔̤ ̱̘̝̝̖t̘̥̦h̢̖̺͔e̳̹͔͟ ̗̟͓̰̀T̟̰̺̥̙͉̮͞en͔̬͓̖̫̪n̵̥o͚̜͈̞̱.͔͖̳̝̳̗ ̗͠ͅY͖͚̗̖̤o͙̟̲u͈̥͉̝ͅ ̭̫͙a̠͈͉͇̥r̛̦̬͔̼͓e̫͞ ̟͍̗ṱ̣̤̗̘h͔͖̤͝e̬͉͟ ҉̥̪̰̗̻̰͔O̦p҉̲͓̯̜̯̳̫e͍͖̟͉̠̦̥r͚̠͢a͈̺̯͍̞̠̮t̛͙̰̤̹̪̼ͅo̸̠r̨̼.̤̗̫̲̫̫͟ ͕̰̳͍͍̖O̶͎͖̠̩r̭͔͙dḭ̹͎̗̞ş̯͎ ̝i҉̯͙͚̟s ̯̜t̺h̜̘̗̘̤̥ȩ͕̮̜͓͉̦ ̡̤̬̖̗̩̤̙C̳̞̥̳e̙p҉̻̙͖̫̟̟ḫ̷̙a̴͕͎l̪̼̜̺̯̱͕͡o̲̙͍n̤̟̩͈̮̤.҉͔ ͓̻̭͕͓̬͍O̺͜ͅrd͍̫i̱̻̰͙̣s̺̫̬͎̥ͅ ̸̙̺̲̟̬͓i̜̳̱s̖̩̀ͅ ̷t̖̭͓̦̩͓h̺̙͍̯͓͕e ͍̩̣̦̣̰͡s̬̤͟h͓̩͇̬ip̹̙.̝̱̖̙̹̙͝ͅ**",
				"Strange, Ordis did not see the value of those photonic wavelengths until now.",
				"An unexpected color combination, Operator. My sensors are **b̵lḙ̳̬̳̫̟e̡͓̻̞̬̦d͏̝̬i̶͔͓͖n̸͙̠̻͈̭g̭͝** pleased.",
				"Ordis did not think the Operator could be more attractive. Wrong again, Ordis!",
				"A fearsome appearance, Operator. You will strike terror in your enemies.",
				"Well put together, Operator, now get out there and **c͉u̲̫͓͟t̹̖̱̗̕ ̸̳͎͎̳̼͔d̷͎̗̤͖o̠̲͟wn͈͍̻͍͓ t̖́h̪̲̺̪̼e̸̯** make the Lotus proud.",
				"Excellent armaments, Operator. Please return **c̯̳̪̗̳͇͚o͇̙̞v̰̦̺ẹ̴re͍d͘ ̭̩͢i̦n̝̘͍̳ͅ ͔̺͓̙͈̫͓b͍̙l͙̱̲ọo̞̭̜d̢̟̤̥̟̥͉** safe and sound.",
				"**U̙ń̠̹͔r̞̥̤̗̟͢ͅeà̰d̼̰̫̟͟ ̱͕̪̖i̟͕̹̞ͅnb̤̞̱̝͓̦̬o̧̠̜̺̠̻̫x͓́ m͚̗͔̕e̝̻̭̲̙s̖̼̬͕̞͔̥sͅa͇͟g͍̱͘e̵͈̹̹̮s̻̺̻̀ ͓̝͢m͚͔a̕k̙̖͈͓͟ͅͅe̷͓̫͙̗͎̝ ̘͢m͈̼͎͍̙̯͎e̬̭̪ ̤̫͙̼̩i̯ll̬̗̟͎͞!**",
				"**W͍̯̹̮͘h͖̯͉̳͟ͅo̯̤̹̹̞̬'͎̟͉̩̙̬͜s̛̩̖ ̰͈í̠̯̮͕̮n̝̟͢ ͚͚͈̟̬c҉̭̙̘o͕̪̲̤n̬̝̝̟̘͉tr͕o̗̩̹͙l̝̟̠̫̭̖ ̥͓̞͙̠͉̱n̫̮͉̩̜͢ǫ̜̠͈̦w̴̦̬̻̳̯͓,̷̺̻̺͙̻̺ ̤̘̣̗͍̻̟͝s̹̳̝͖͟u̝͖c̴͇͍͇͎͙̻̪k̲̻e̦͝r̷͚͓̳̥̤s̛̩͓̹̟̥!?** ...er, uh, security override engaged.",
				"**Ý̹ͅy͖̮̜͎̪͖ͅy̶̱̹̪̱͍̲̯o̞̝̦͇̬̹u ̨c̙̙̦̞̠͜ą̼̭̜͖̳͚n͎̼̬͠ ̖̠͕͝g̟̞͎͈͚̣͝ͅo͙̬̤̺͖̼̙ ͔̲̺̜̳͓̮s̻͉͚̳͕t͕̩͙͎ͅr̤̜̟a̧̮̪͈͙̳̠i͎͝g̹̕h̥͖t͏̟̻̦̠͙ ͕͕̩̫͖̝t̸͍̲͍̹o͉͔͔͇̜͍̮**... Operator, I am sorry.",
				"Ordis will gladly assist the Operator in **c̷̱̦͉̳̰ut̞͓͚̣̯̝t̵͙͚͔i̹͚͔̦͢n͙͝g̵̟̤̝̭ ̣à̤͉͙ ̞̮̥͕̲͞b̝l̟͉o̲̞̳ͅo̖̗͉͕͓ͅd̯͕̣̯͙̺̥͢y̵͉͖̲͙̳̯ ̞͖p͇a̧̹̟̹͓̩t̩͓̳̝͚͢h͘** in what ever mission they choose.",
				"The Operator has all the necessary blueprints to craft an Archwing **S̠̗̖̯̰̦̺O̡̯̗̗ ͙͚̤͈͙W̰̗̺̫̩̘̘H̟A̶̳̜̦̮̜̖͓T̲̥͙̗͢ ͓͚̳͖̟ͅA͙Ŕ͓̱̼̼͖̜̫E҉̺͎͓ ̝̩͇͙͢Y̶̯̙̦̗ͅO̗͈̣̫͉̯̠U͔̬̱̯̻͔̻ ̪̩̮̦̤̠͖W̙̳͙͍͓͠A̴̤͓I̤̝͝T̼̼̤̪̯͓̀I̶̙̪̬͈̬N͈̤̠̫̺͔G̥͞ ̢̻̙̙̗̝F͇̯͘O̜̫̫͍͈͞R!?**",
				"No egg yet? Don't despair, Operator. Nothing good ever came out of an egg.",
				"Operator, drop the egg into the system to begin breeding **d̼̰̖̞͍̰ͅr͇͓̮̞̯o͚͙͚̹̳̥ͅp͢ ̤̕i̧̜̝̙̪t̝̙͉͚̗̲̹͜ ̵̞o͎̱̣̕n̡͈ ̼̳̮̞̙̗ț̸̦͕̦h͈͇̫e̤̭̬̟̯ ̲f͎̼̱lo͕͜o̜͚̦̻̲̤̻r̫**.",
				"Well it's woefully incomplete; half written proofs, unfinished equations. **W̨H̥̲̞̖͈̜A͈̝̟̺̘T̸͉͉ I҉͕̱͓D̟̭͖͚̺̰̀I̢̼̰̬̠̯͚͙O̦̼̰͇̝T͙͇̥̤̼̙͝ ̴͇͓̖̖̳̗ͅW̱͈̳̯͎̥̰R͈̙͇͙͝ͅOŢE͍ ̼̘̥̤T̴̫̦͈̟͕H̠͔̲I̧̗͉̠͎̥̜S̲͍̟̮͎͕ ̗̻͙͚̺̭̝M̟̭̣̝E̫͓̖̭͈͙SS̨̘͙̙̥̯̞!?**",
				"Excellent, we have completed the scan. May I suggest **T̙U͏R̴̩̰ŃI̛N̛̙̺͖ͅG̮ ̷̫̘͔EV̖̗̙̦͜É̞͓͚̲̺͉R̹̰̩̟̮̭͠Y̗͓̹̲͘ ̶̞̘͍͖̗̠̤L͕̮͕̮A͇̥̣ST͏̼ ͕̱͎̻͖͡Ḙ͎̻͙͡N͎E̢̫̬͓͓͙̤M̛Y͖̹̗̤̺͚̝ ̼̥͘ͅI͈͎̦̠͙͈Ṋ̝̼͚̣͕T̥̰̥̥̯͟ͅO ̵̙S͈̯̖̤̥C̲̞͈̫̣̯̩͝R̰̭̻AP̼?**",
				"Operator, did you enjoy that poem? Ordis is composing one as well. It begins: **A̞ ͉̼s̵̬̼c͖̫͙͍̭̲͡ͅu͇̳̖͕̳m͖ͅb͉̙̰͘a̟̝̜͎͚̮͔g̞̣̝̪ ̴̦̘̟͓̹̟̠C̛̤̱̭ọ̲̯̲͘r̶p̲͉͟u̬̤s̲̟ ̧̫ͅf̨̠̰͖̠̪̯r͓͎̟̱̬̪̰̀o̧͙̯̤̱m̻̲̩̯̩ ̩̤̝̟͙̞V̖̤͟ḛ̠̲͍͙nu̧͖̜s̟͎͘ͅͅ...**",
				"The Operator comes first! **S̮͕̠̱͙H̥̫̻͎̭͖́ͅU̴̙̟̱͈͓̳ͅŢ̹͔̦̟ Y͚͍͖̹̞O̶̪̲̹̪̻U͕̗̭̫̺̟R̵̤͔ ͔̣̱̻͔OS̡͓̹̭̻͇ͅC͖͇̬I͕̙͓̹̟̞L͈L͡A̗͓͍T̮̪̭͓̲̰͡ÓR̝͙ ͈S̶I̮̣̩͉̥̤M͈͔ÁR̲̞͔̗͍̥̜I̠͜S͓͍̥͜!**",
				"My Operator is no stranger to **s̟̹h̸͙͈̩̭̜r̢̖̗̳ȩ̘̖ͅd͏̬̞̳d͎̠i͎̹̤̞͓̹͞n̯̪̜̠͕g͍͓̗̙̼ ̵̩f̞̼̮͙̠l͙͕̣̩̟ḛ͖̰̤̗̞sh̜** eradicating infestations.",
				"Ordis remembers many more Cephalon voices, but now it is only silence...",
				"Tenno, **R҉ÌP̗͓͙̞ ͙̟̯IT̳ ̧̗̺̹͕̩A̝̜̲̯P̶͈A̭̝̦͙̹ͅR̴̜̫͓͖̤͎̠T̠̹̬͠**.",
				"**N̤̰̹̭O̬̤̰̬͈͇̦͟B̫͍̫͠O̭D̖͢Ỵ̤͇̱ ̮̻͇̲̥͞M̖̺̻͕͢A̧̤̩͙K̖̱̟͓͚̯͘E͔S ̙͔̦̘̣̪͕̕Ạ̦ ͕F̜͈̘̼OO̩͖̭̰̩̩̰Ĺ̟͎̭̱̦͇ Ơ̪͕̟̻̫F̵̜̟͎̜̹ ͚O͈̞̗͎̥ͅR̭͞DI̻͍͞S̩̬̦͈̬͇͝!** Oh dear, where did that come from?",
				"Bypass that console to overdrive the system. Try to increase the amplification circuits **T̰̣͈͕̦O̲͇͟.̬ ͡E̸̟̳̖̠̲L̙͖̲̻͔EV̟̣̯̟E͏̬͎̙̝N̺̮̜̜̞.**",
				"**MY DREAM! IT'S COME TRUE! I̙ ͙͖̮̣̦̬̭H̰̪̟̻A̼V͖͍͎̻͉̲́Ẹ̩ ͔̳J̸͍̖͖̩̫̱O͙̝̹̰̻͙̩I̼̝̫̬͡N̴̞͍̫̳ͅED͔͡ ͎̻ͅM̭̣̫͍̻Y̨̲͓ O͝P̤ͅE̙̫͉R̵̯̲̮A͓T̨̩̺O͙̣R̫̰ ̫͚͚̭̥I͟N̦̗̬̻̠͟ ̼̫̗͙̯̝̘B̻A̶̬̹̣͇ͅT͉͈Ṯ̗̹̩̱L͉E̩̠̬̣̼̪!**"
			};
			
			Console.WriteLine("Deserializing bartending dictionary.");
			try
			{
				using (var file = File.OpenRead("data/bartending.bin")) {
					BartendingData = Serializer.Deserialize<Dictionary<string, List<string>>>(file);
				}
			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine(ex);
				BartendingData = new Dictionary<string, List<string>>();
				BartendingData.Add("vessel", new List<string>(new string[] {"a generic cup"}));
				BartendingData.Add("garnish", new List<string>(new string[] {"with a generic garnish"}));
				BartendingData.Add("beverage", new List<string>(new string[] {"a generic liquid"}));
			}

			IObservable<long> periodicSaveTimer = Observable.Interval(TimeSpan.FromMinutes(10));
			CancellationTokenSource source = new CancellationTokenSource();
			Action action = (() => 
			{
				Program.Config.SaveBartendingData();
			}
			);
			periodicSaveTimer.Subscribe(x => { Task task = new Task(action); task.Start();}, source.Token);
		}

		public static async Task ResolveCommand(SocketMessage message)
		{
			string[] message_contents = message.Content.Substring(1).Split(" ");
			switch(message_contents[0])
			{
				case "drink":
					await BotUtilities.CommandDrink(message, false);
					break;
				case "dwink":
					await BotUtilities.CommandDrink(message, true);
					break;
				//case "save":
				//	await Program.Config.Save(message);
				//	break;
				case "wakeup":
					await Program.Config.WakeUp(message);
					break;
				case "shutup":
					await Program.Config.ShutUp(message);
					break;
				case "replyrate":
					await Program.Config.SetReplyRate(message);
					break;
				case "roll":
					await BotDice.RollDiceStandard(message);
					break;
				case "dryh":
					await BotDice.RollDiceDRYH(message);
					break;
				case "fate":
					await BotDice.RollDiceFate(message);
					break;
				case "twitch":
					await BotTwitchPoller.CommandPollStreamer(message);
					break;
				case "vorpost":
					await BotUtilities.CommandVorPost(message);
					break;
				case "hekpost":
					await BotUtilities.CommandHekPost(message);
					break;
				case "ordispost":
					await BotUtilities.CommandOrdisPost(message);
					break;
				case "who":
				case "qw":
					await BotAetolia.CommandAetoliaWho(message);
					break;
				case "honours":
				case "honors":
					await BotAetolia.CommandAetoliaHonours(message);
					break;
				case "readnews":
					await BotAetolia.CommandAetoliaReadnews(message);
					break;
				case "nstat":
					await BotAetolia.CommandAetoliaNstat(message);
					break;
				default:
					await message.Channel.SendMessageAsync("Unknown command, insect.");
					break;
			}
		}


		internal static async Task CommandHekPost(SocketMessage message)
		{
			await message.Channel.SendMessageAsync(hekPostStrings[rnjesus.Next(hekPostStrings.Count)]);
		}
		internal static async Task CommandOrdisPost(SocketMessage message)
		{
			await message.Channel.SendMessageAsync(ordisPostStrings[rnjesus.Next(ordisPostStrings.Count)]);
		}
		internal static async Task CommandVorPost(SocketMessage message)
		{
			string result = "Look at them, they come to this place when they know they are not pure. Tenno use the keys, but they are mere trespassers. Only I, Vor, know the true power of the Void. I was cut in half, destroyed, but through it's Janus Key, the Void called to me. It brought me here and here I was reborn. We cannot blame these creatures, they are being led by a false prophet, an impostor who knows not the secrets of the Void. Behold the Tenno, come to scavenge and desecrate this sacred realm. My brothers, did I not tell of this day? Did I not prophesize this moment? Now, I will stop them. Now I am changed, reborn through the energy of the Janus Key. Forever bound to the Void. Let it be known, if the Tenno want true salvation, they will lay down their arms, and wait for the baptism of my Janus key. It is time. I will teach these trespassers the redemptive power of my Janus key. They will learn it's simple truth. The Tenno are lost, and they will resist. But I, Vor, will cleanse this place of their impurity.";
			await message.Channel.SendMessageAsync(result);	
		}

		internal static async Task CommandDrink(SocketMessage message, bool owo)
		{
			string result = "Usage: .drink add [beverage|garnish|vessel] thing.";
			string[] message_contents = message.Content.Substring(1).Split(" ");
			if(message_contents.Length == 1 || owo) 
			{
				List<string> drinkParts = new List<string>();
				foreach(string drinkPart in validDrinkFields) 
				{
					drinkParts.Add(BartendingData[drinkPart][rnjesus.Next(BartendingData[drinkPart].Count)]);
				}
				result = $"_slings {drinkParts[0]}, containing {drinkParts[1]} {drinkParts[2]}, down the bar to {message.Author.Mention}._";
				if(owo)
				{
					string owotext = "";
					char lastChar = '\0';
					foreach(char c in result)
					{
						switch(c)
						{
							case 'l':
							case 'r':
								owotext += 'w';
								break;
							case 'L':
							case 'R':
								owotext += 'W';
								break;
							case 'u':
								if(lastChar == 'Q' || lastChar == 'q')
								{
									owotext += 'w';
								}
								else
								{
									owotext += c;								
								}
								break;
							case 'U':
								if(lastChar == 'Q' || lastChar == 'q')
								{
									owotext += 'W';
								}
								else
								{
									owotext += c;
								}
								break;
							default:
								owotext += c;
								break;
						}
						lastChar = c;
					}
					result = owotext + " :3c";
				}
			} 
			else if(message_contents.Length >= 4)
			{
				if(message_contents[1].ToLower() == "add")
				{
					string barKey = message_contents[2].ToLower();
					if(validDrinkFields.Contains(barKey))
					{
						string barText = message.Content.Substring(12 + barKey.Length);
						BartendingData[barKey].Add(barText);
						result = $"_will now stock {barText} as a {barKey}._";
						HasDictionaryChanged = true;
					}
				}
			}
			await message.Channel.SendMessageAsync(result);
		}
	}
}
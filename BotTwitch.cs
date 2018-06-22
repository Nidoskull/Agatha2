using Discord;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using System.Reactive.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Agatha2
{
	internal static class BotTwitchPoller
	{

        private static Dictionary<String, Boolean> streamStatus;
        private static Dictionary<String, String> streamIDtoUserName;
        private static Dictionary<String, String> streamIDToDisplayName;
        private static List<String> streamers;

                static BotTwitchPoller()
        {
            Debug.WriteLine("Initializing stream module.");
            streamStatus = new Dictionary<String, Boolean>();
            streamIDtoUserName = new Dictionary<String, String>();
            streamIDToDisplayName = new Dictionary<String, String>();
            streamers = new List<String>();

            var logFile = File.ReadAllLines("data/streamers.txt");
            foreach(String streamer in new List<string>(logFile))
            {
                JToken jData = RetrieveUserIdFromUserName(streamer);
                string streamerID = jData["id"].ToString();
                streamers.Add(streamerID);
                streamStatus.Add(streamerID, false);
                streamIDtoUserName.Add(streamerID, streamer);
                streamIDToDisplayName.Add(streamerID, jData["display_name"].ToString());
            }
            Debug.WriteLine("Done loading streamers. Initializing poll timer.");
			IObservable<long> pollTimer = Observable.Interval(TimeSpan.FromMinutes(1));
			CancellationTokenSource source = new CancellationTokenSource();
			Action action = (() => 
			{
				PollStreamers(null);
			}
			);
			pollTimer.Subscribe(x => { Task task = new Task(action); task.Start();}, source.Token);
            Debug.WriteLine("Stream poller initialized.");
        }

        internal static JToken RetrieveUserIdFromUserName(String streamer)
        {
            Debug.WriteLine($"Retrieving streamer info for {streamer}.");
            var request = (HttpWebRequest)WebRequest.Create($"https://api.twitch.tv/helix/users?login={streamer}");
            request.Method = "Get";
            request.Timeout = 12000;
            request.ContentType = "application/vnd.twitchtv.v5+json";
            request.Headers.Add("Client-ID", Program.Config.StreamAPIClientID);

            try
            {
                var s = request.GetResponse();
                if(s != null)
                {
                    System.IO.Stream sStream = s.GetResponseStream();
                    if(sStream != null)
                    {
                        var sr = new System.IO.StreamReader(sStream);
                        if(sr != null)
                        {
                            var jsonObject = JObject.Parse(sr.ReadToEnd());
                            if(jsonObject != null)
                            {
                                return jsonObject["data"][0];
                            }
                        }
                    }
                }
            }
            catch(WebException e)
            {
                Debug.WriteLine("Streamer does not exist.");
            }
            return null;
        }

        internal static async Task CommandPollStreamer(SocketMessage message)
        {
            string[] message_contents = message.Content.Substring(1).Split(" ");
            string msg = "No user supplied for lookup.";
			if(message_contents.Length >= 2)
			{
                String streamer = message_contents[1];
                JToken jData = RetrieveUserIdFromUserName(streamer);
                if(jData != null && jData.HasValues)
                {
                    String lineOne = $"{message.Author.Mention}: {jData["display_name"].ToString()} - http://twitch.tv/{streamer}";
                    String desc = jData["description"].ToString();
                    if(desc == null || desc == "")
                    {
                        desc = "No description supplied.";
                    }
                    await message.Channel.SendMessageAsync($"{lineOne}\n`{desc}`");
                }
                else
                {
                     await message.Channel.SendMessageAsync($"{message.Author.Mention}: No user found for '{streamer}'.");
                }
            }
            else
            {
                await PollStreamers(message);
                await message.Channel.SendMessageAsync($"{message.Author.Mention}: done.");
            }
        }

        internal static async Task PollStreamers(SocketMessage message)
        {
            Debug.WriteLine("Polling streamers.");
            IMessageChannel channel = message == null ? Program.Client.GetChannel(Program.Config.StreamChannelID) as IMessageChannel : message.Channel;
            foreach(string streamer in streamers)
            {
                Debug.WriteLine($"Looking up user id {streamer}");
                var request = (HttpWebRequest)WebRequest.Create($"https://api.twitch.tv/helix/streams?user_id={streamer}");
                request.Method = "Get";
                request.Timeout = 12000;
                request.ContentType = "application/vnd.twitchtv.v5+json";
                request.Headers.Add("Client-ID", Program.Config.StreamAPIClientID);

                try
                {
                    using (var s = request.GetResponse().GetResponseStream())
                    {
                        using (var sr = new System.IO.StreamReader(s))
                        {
                            var jsonObject = JObject.Parse(sr.ReadToEnd());
                            var jsonStream = jsonObject["data"];
                            Boolean streamActive = jsonStream.HasValues;
                            String streamerName = streamIDtoUserName[streamer];
                            String streamerDisplayName = streamIDToDisplayName[streamer];
                            if(!streamStatus[streamer] && streamActive)
                            {   
                                streamStatus[streamer] = true;
                                await channel.SendMessageAsync($"{streamerDisplayName} has started streaming '{jsonObject["data"][0]["title"].ToString()}' at http://twitch.tv/{streamerName}!");
                            }
                            else if(streamStatus[streamer] && !streamActive)
                            {
                                streamStatus[streamer] = false;
                                await channel.SendMessageAsync($"{streamerDisplayName} has stopped streaming.");
                            }
                        }
                    }
                }
                catch(WebException e)
                {
                    Debug.WriteLine(e);
                }
            }
        }
    }
}

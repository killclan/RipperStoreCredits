using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using XDMessaging;

namespace Ripper.Store.External
{
    public class _message_json
    {
        public string name { get; set; }
    }

    internal class Program
    {
        private static HttpClient _http = new HttpClient();
        private static XDMessagingClient _messaging_client;
        private static IXDListener _listener;
        public static Config Config { get; set; }
        [STAThread]
        static void Main(string[] args)
        {

            log("RipperStoreCredits has started!");
            if (!File.Exists("RipperStoreCredits.txt")) { Application.EnableVisualStyles(); Application.Run(new AskForKey()); }
            else { Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("RipperStoreCredits.txt")); }

            try
            {
                var status = _http.GetAsync($"https://api.ripper.store/clientarea/credits/validate?apiKey={Config.apiKey}").Result;
                if ((int)status.StatusCode != 200)
                {
                    Application.EnableVisualStyles();
                    Application.Run(new AskForKey());
                }
            }
            catch { };

            _messaging_client = new XDMessagingClient();
            _listener = _messaging_client.Listeners.GetListenerForMode(XDTransportMode.Compatibility);
            _listener.MessageReceived += Listener_MessageReceived;
            _listener.RegisterChannel("AvatarChannel");

            System.Timers.Timer timer = new System.Timers.Timer(1000) { AutoReset = true, Enabled = true };
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            Thread.Sleep(-1);
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Process.GetProcessesByName("VRChat").Length == 0)
                Process.GetCurrentProcess().Kill();
        }

        private static void Listener_MessageReceived(object sender, XDMessageEventArgs e)
        {
            try
            {
                if (Config != null)
                {
                    var data = new StringContent(e.DataGram.Message, Encoding.UTF8, "application/json");

                    var __0 = JsonConvert.DeserializeObject<_message_json>(e.DataGram.Message, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore, Error = (se, ev) => ev.ErrorContext.Handled = true });
                    var res = _http.PostAsync($"https://api.ripper.store/clientarea/credits/submit?apiKey={Config.apiKey}&v=10&t={DateTimeOffset.Now.ToUnixTimeSeconds()}", data).GetAwaiter().GetResult();
                    var name = __0.name.Length > 32 ? __0.name.Substring(0, 32) : __0.name;

                    if (Config.LogToConsole)
                    {
                        switch (res.StatusCode)
                        {
                            case (HttpStatusCode)201:
                                log($"Successfully send {name} to API, verification pending");
                                break;
                            case (HttpStatusCode)409:
                                log($"Failed to send {name}, already exists");
                                break;
                            case (HttpStatusCode)401:
                                log("Invalid API Key Provided");
                                break;
                            case (HttpStatusCode)403:
                                log("Your Account got suspended");
                                break;
                            case (HttpStatusCode)426:
                                log("You are using an old Version of this Mod, please update via our Website (https://ripper.store/clientarea) > Credits Section");
                                break;
                            case (HttpStatusCode)429:
                                log("You are sending too many Avatars at the same time, slow down..");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }

        private static void log(object v)
        {
            Console.WriteLine($"RipperStoreCredits : {v}");
            Console.ResetColor();
        }
    }
}

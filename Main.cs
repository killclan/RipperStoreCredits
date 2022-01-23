using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using MelonLoader;
using Newtonsoft.Json;
using VRC.Core;
using HarmonyLib;
using System.IO;
using System.ComponentModel;
using System.Collections;
using UnityEngine;

namespace RipperStoreCreditsUploader
{
    public static class BuildInfo
    {
        public const string Name = "RipperStoreCredits";
        public const string Author = "CodeAngel";
        public const string Company = "https://ripper.store";
        public const string Version = "7";
        public const string DownloadLink = null;
    }

    public class Main : MelonMod
    {
        private static Config Config { get; set; }
        private static Queue<ApiAvatar> _queue = new Queue<ApiAvatar>();
        private static HttpClient _http = new HttpClient();
        private static HarmonyMethod GetPatch(string name) { return new HarmonyMethod(typeof(Main).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)); }
        public override void OnApplicationStart()
        {
            if (!File.Exists("RipperStoreCredits.txt"))
            {
                MelonLogger.Msg("\n------- !! -------\n\n\nGenerated new config, please insert your apiKey into 'RipperStoreCredits.txt' and restart your Game.\n\n\n------- !! -------\n");
                File.WriteAllText("RipperStoreCredits.txt", JsonConvert.SerializeObject(new Config { apiKey = "place_apiKey_here", LogToConsole = true }, Formatting.Indented));
            }
            else { Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("RipperStoreCredits.txt")); }

            foreach (var methodInfo in typeof(AssetBundleDownloadManager).GetMethods().Where(p => p.GetParameters().Length == 1 && p.GetParameters().First().ParameterType == typeof(ApiAvatar) && p.ReturnType == typeof(void)))
            {
                HarmonyInstance.Patch(methodInfo, GetPatch("AvatarToQueue"));
            }

            MelonCoroutines.Start(CreditWorker());
        }

        private static void AvatarToQueue(ApiAvatar __0)
        {
            try
            {
                if (__0 != null) _queue.Enqueue(__0);
            }
            catch { }
        }

        private static IEnumerator CreditWorker()
        {
            while (_queue != null)
            {
                try
                {
                    if (_queue.Count != 0 && RoomManager.field_Internal_Static_ApiWorld_0 != null)
                    {
                        var __0 = _queue.Peek();
                        _queue.Dequeue();

                        var obj = new ExpandoObject() as IDictionary<String, object>;
                        foreach (PropertyDescriptor _ in TypeDescriptor.GetProperties(__0))
                        {
                            if (_.GetValue(__0) == null) _.SetValue(__0, "null");
                            obj[_.Name] = _.GetValue(__0);
                        }

                        obj["hash"] = BitConverter.ToString(Encoding.UTF8.GetBytes($"{__0.id}|{__0.assetUrl}|{__0.imageUrl}|{APIUser.CurrentUser.id}"));
                        StringContent data = new StringContent(JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), Encoding.UTF8, "application/json");

                        var res = _http.PostAsync($"https://api.ripper.store/clientarea/credits/submit?apiKey={Config.apiKey}&v={BuildInfo.Version}&t={DateTimeOffset.Now.ToUnixTimeSeconds()}", data).GetAwaiter().GetResult();
                        var name = __0.name.Length > 32 ? __0.name.Substring(0, 32) : __0.name;

                        if (Config.LogToConsole)
                        {
                            switch (res.StatusCode)
                            {
                                case (HttpStatusCode)201:
                                    MelonLogger.Msg($"Successfully send {name} to API, verification pending..");
                                    break;
                                case (HttpStatusCode)409:
                                    MelonLogger.Warning($"Failed to send {name}, already exists..");
                                    break;
                                case (HttpStatusCode)401:
                                    MelonLogger.Error("Invalid API Key Provided");
                                    break;
                                case (HttpStatusCode)403:
                                    MelonLogger.Error("Your Account got suspended.");
                                    break;
                                case (HttpStatusCode)426:
                                    MelonLogger.Error("You are using an old Version of this Mod, please update via our Website (https://ripper.store/clientarea) > Credits Section");
                                    break;
                                case (HttpStatusCode)429:
                                    MelonLogger.Warning("You are sending too many Avatars at the same time, slow down..");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Error("Error while sending Avatar to API: " + e);
                }

                yield return new WaitForSeconds(0.5f);
            }
        }
    }

}

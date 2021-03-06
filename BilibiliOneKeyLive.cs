﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sync.Plugins;
using Sync.Tools;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OneKey
{
    class BilibiliOneKeyLive
    {
        private Object configInstance;
        private PropertyInfo propertyCookies,propertyRoomId;

        private string Cookies => propertyCookies.GetValue(configInstance).ToString();
        private string RoomId => propertyRoomId.GetValue(configInstance).ToString();

        public BilibiliOneKeyLive()
        {
            Type config_manager_type = typeof(PluginConfigurationManager);
            var config_manager_list = config_manager_type.GetField("ConfigurationSet", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null) as IEnumerable<PluginConfigurationManager>;

            //each configuration manager
            foreach (var manager in config_manager_list)
            {
                //get plguin name
                var plguin = config_manager_type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager) as Plugin;
                if (plguin.Name == "Default Plug-ins")
                {

                    //get List<PluginConfiuration>
                    var config_items_field = config_manager_type.GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);
                    var config_items_list = config_items_field.GetValue(manager);

                    //List<PluginConfiuration>.GetEnumerator
                    var enumerator = config_items_field.FieldType.GetMethod("GetEnumerator", BindingFlags.Public | BindingFlags.Instance)
                        .Invoke(config_items_list, null) as IEnumerator;

                    //each List<PluginConfiuration>
                    while (enumerator.MoveNext())
                    {
                        var config_item = enumerator.Current;
                        var instance = config_item.GetType().GetField("config", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(config_item);
                        if (instance.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance)?.GetValue(instance).ToString()=="Bilibili")
                        {
                            configInstance = instance;
                            var config_type = configInstance.GetType();

                            foreach (var prop in config_type.GetProperties())
                            {
                                if (prop.Name == "Cookies")
                                    propertyCookies = prop;
                                else if (prop.Name == "RoomID")
                                    propertyRoomId = prop;
                            }
                        }
                    }

                    break;
                }
            }
        }

        public async void StartLive()
        {
            JObject json = await PostAsync("/room/v1/Room/startLive",new Dictionary<string, string>
            {
                { "platform", "pc"},
                { "area_v2", "107"}
            });

            if (json != null)
            {
                Sync.Tools.IO.CurrentIO.WriteColor($"Status:{json["data"]["status"]}", ConsoleColor.Green);
                Sync.Tools.IO.CurrentIO.WriteColor($"RTMP Address:{json["data"]["rtmp"]["addr"]}", ConsoleColor.Green);
                Sync.Tools.IO.CurrentIO.WriteColor($"RTMP Code:{json["data"]["rtmp"]["code"]}", ConsoleColor.Green);
            }
        }

        public async void StopLive()
        {
            JObject json = await PostAsync("/room/v1/Room/stopLive");

            Sync.Tools.IO.CurrentIO.WriteColor($"Status:{json["data"]["status"]}", ConsoleColor.Green);
        }

        public async void SetRoomName(string name)
        {
            JObject json = await PostAsync("/room/v1/Room/update",new Dictionary<string, string>()
            {
                ["title"]=name,
            });

            if(json!=null)
            {
                Sync.Tools.IO.CurrentIO.WriteColor("[OneKey]Success!",ConsoleColor.Green);
            }
            else
            {
                Sync.Tools.IO.CurrentIO.WriteColor("[OneKey]Fail!", ConsoleColor.Red);
            }
        }

        public async Task<string> GetRoomName()
        {
            JObject json = await PostAsync($"/room/v1/Room/get_info?room_id={RoomId}");

            return json?["data"]["title"].ToString();
        }

        private async Task<JObject> PostAsync(string path, Dictionary<string,string> dict=null)
        {
            var baseAddress = new Uri("http://api.live.bilibili.com");
            var cookieContainer = new CookieContainer();

            var keyValueDict = dict ?? new Dictionary<string, string>();
            keyValueDict.Add("room_id", RoomId);

            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var content = new FormUrlEncodedContent(keyValueDict);

                var cookies = Cookies.Split(';').Select(s => s.Trim().Split('=').Select(ss => ss.Trim()));
                foreach (var item in cookies)
                {
                    if (item.Count() == 2)
                    {
                        string key = item.ElementAt(0);
                        string value = item.ElementAt(1);
                        if(value.IndexOf(',')==-1)
                            cookieContainer.Add(baseAddress, new Cookie(key,value));
                    }
                }
                var result = client.PostAsync(path, content).Result;
                result.EnsureSuccessStatusCode();

                string json_str = await result.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(json_str);

                if((int)json["code"]!=0)
                {
                    Sync.Tools.IO.CurrentIO.WriteColor($"Message:{json["msg"]}", ConsoleColor.Red);
                    return null;
                }
                return json;
            }
        }

        private async Task<JObject> GetAsync(string path)
        {
            var baseAddress = new Uri("http://api.live.bilibili.com");
            var cookieContainer = new CookieContainer();

            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var cookies = Cookies.Split(';').Select(s => s.Trim().Split('=').Select(ss => ss.Trim()));
                foreach (var item in cookies)
                {
                    if (item.Count() == 2)
                    {
                        string key = item.ElementAt(0);
                        string value = item.ElementAt(1);
                        if (value.IndexOf(',') == -1)
                            cookieContainer.Add(baseAddress, new Cookie(key, value));
                    }
                }

                var result = client.GetAsync(path).Result;
                result.EnsureSuccessStatusCode();

                string json_str = await result.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(json_str);

                if ((int)json["code"] != 0)
                {
                    Sync.Tools.IO.CurrentIO.WriteColor($"Message:{json["msg"]}", ConsoleColor.Red);
                    return null;
                }
                return json;
            }
        }
    }
}

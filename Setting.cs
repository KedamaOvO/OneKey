using Newtonsoft.Json;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneKey
{
    public class SettingIni : IConfigurable
    {
        public ConfigurationElement CommandBindedDictionary
        {
            set => Setting.CommandBindedDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(value)?? new Dictionary<string, string>();
            get => JsonConvert.SerializeObject(Setting.CommandBindedDictionary);
        }

        public void onConfigurationLoad()
        {
        }

        public void onConfigurationReload()
        {
        }

        public void onConfigurationSave()
        {
        }
    }

    internal static class Setting
    {
        public static Dictionary<string,string> CommandBindedDictionary= new Dictionary<string, string>();
    }
}

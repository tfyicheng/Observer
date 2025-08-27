using Newtonsoft.Json;
using System;
using System.IO;

namespace Observer
{
    public static class ConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        private static MainModel defModel = new MainModel()
        {
            Enable1 = false,
            RequestApi0 = "ceshi",
            RequestType0 = "Post"
        };


        public static MainModel Load()
        {
            if (File.Exists(ConfigPath))
            {
                try
                {
                    string json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<MainModel>(json) ?? new MainModel();
                }
                catch
                {
                    // 如果解析失败，返回默认配置
                    return defModel;
                }
            }
            else
            {
                var defaultConfig = defModel;
                Save(defaultConfig);
                return defaultConfig;
            }
        }

        public static void Save(MainModel config)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
        }
    }
}

using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;

namespace Observer
{
    public static class ConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "observercfg.json");
        private static Timer _saveTimer;
        private static MainModel _pendingConfig;
        private static readonly object _lock = new object();


        private static MainModel defModel = new MainModel()
        {
            Port = "8086"
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


        // 节流保存：延迟 1 秒写入
        public static void SaveThrottled(MainModel config)
        {
            lock (_lock)
            {
                _pendingConfig = config;

                // 如果已有定时器，重置
                _saveTimer?.Dispose();

                // 延迟 1 秒再执行保存
                _saveTimer = new Timer(_ =>
                {
                    lock (_lock)
                    {
                        if (_pendingConfig != null)
                        {
                            Save(_pendingConfig);
                            _pendingConfig = null;
                        }
                    }
                }, null, 1000, Timeout.Infinite);
            }
        }

        public static void Save(MainModel config)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
        }
    }
}

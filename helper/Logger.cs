using System;
using System.IO;
using System.Text;

namespace Observer
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static readonly string _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");

        static Logger()
        {
            if (!Directory.Exists(_logDir))
            {
                Directory.CreateDirectory(_logDir);
            }
        }

        private static string GetLogFilePath()
        {
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            return Path.Combine(_logDir, fileName);
        }

        public static void Write(string message)
        {
            WriteInternal(message, false);
        }

        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
            WriteInternal(message, true);
        }

        private static void WriteInternal(string message, bool appendLine)
        {
            try
            {
                if (Common.model != null && Common.model.Run2)
                {
                    lock (_lock)
                    {
                        string logFile = GetLogFilePath();
                        using (var sw = new StreamWriter(logFile, true, Encoding.UTF8))
                        {
                            string timePrefix = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            if (appendLine)
                                sw.WriteLine($"[{timePrefix}] {message}");
                            else
                                sw.Write($"[{timePrefix}] {message}");
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 删除指定天数之前的日志文件，默认保留30天
        /// </summary>
        /// <param name="days">保留天数</param>
        public static void CleanOldLogs(int days = 30)
        {
            try
            {
                if (!Directory.Exists(_logDir)) return;

                var files = Directory.GetFiles(_logDir, "*.txt");
                foreach (var file in files)
                {
                    DateTime lastWriteTime = File.GetLastWriteTime(file);
                    if (lastWriteTime < DateTime.Now.AddDays(-days))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                // 避免清理出错影响主逻辑
                Console.WriteLine("清理日志时出错: " + ex.Message);
            }
        }
    }
}

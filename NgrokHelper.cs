using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;

namespace Observer
{
    public class NgrokHelper
    {
        private Process ngrokProcess;
        private readonly string ngrokPath;

        public NgrokHelper(string ngrokPath)
        {
            if (string.IsNullOrEmpty(ngrokPath))
                throw new ArgumentException("ngrok 路径不能为空");

            this.ngrokPath = ngrokPath;
        }

        /// <summary>
        /// 启动 Ngrok 隧道并返回公网地址（带重试机制）
        /// </summary>
        /// <param name="port">本地端口号</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="delayMs">每次重试等待时间（毫秒）</param>
        /// <returns>公网地址</returns>
        public string StartTunnel(int port, int maxRetries = 10, int delayMs = 1000)
        {
            // 如果已有进程，先关掉
            StopTunnel();

            var psi = new ProcessStartInfo
            {
                FileName = ngrokPath,
                Arguments = $"http {port} --log=stdout",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            ngrokProcess = Process.Start(psi);

            // 尝试多次请求 127.0.0.1:4040
            string url = null;
            using (var client = new HttpClient())
            {
                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        string apiUrl = "http://127.0.0.1:4040/api/tunnels";
                        var response = client.GetStringAsync(apiUrl).Result;

                        var obj = JObject.Parse(response);
                        url = obj["tunnels"]?[0]?["public_url"]?.ToString();

                        if (!string.IsNullOrEmpty(url))
                            break; // 成功获取
                    }
                    catch
                    {
                        // 忽略异常，等待重试
                    }

                    Thread.Sleep(delayMs);
                }
            }

            if (string.IsNullOrEmpty(url))
                throw new Exception("启动 ngrok 成功，但未获取到公网地址（可能超时）");

            return url;
        }

        /// <summary>
        /// 停止 Ngrok 隧道
        /// </summary>
        public void StopTunnel()
        {
            if (ngrokProcess != null && !ngrokProcess.HasExited)
            {
                ngrokProcess.Kill();
                ngrokProcess.Dispose();
                ngrokProcess = null;
            }
        }
    }
}

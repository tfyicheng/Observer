using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;

namespace Observer
{
    public static class Server
    {
        private static SimpleServer _server;

        /// <summary>
        /// 启动内置 HTTP 服务
        /// </summary>
        public static void Start(int port = 8086)
        {
            if (_server != null && _server.IsRunning)
            {
                Console.WriteLine("服务器已在运行中...");
                return;
            }

            var config = new Config { Port = port };
            _server = new SimpleServer(config);
            _server.Start();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public static void Stop()
        {
            if (_server != null && _server.IsRunning)
            {
                _server.Stop();
                Console.WriteLine("服务器已停止");
            }
        }

        /// <summary>
        /// 获取服务状态
        /// </summary>
        public static object Status()
        {
            if (_server == null)
                return new { Running = false };

            return new
            {
                Running = _server.IsRunning,
                Port = _server.Port
            };
        }
    }

    class Config
    {
        public int Port { get; set; }
    }

    class SimpleServer
    {
        private readonly HttpListener listener = new HttpListener();
        private readonly Config config;

        public bool IsRunning { get; private set; }
        public int Port => config.Port;

        public SimpleServer(Config config)
        {
            this.config = config;
            listener.Prefixes.Add($"http://+:{config.Port}/");
        }

        public void Start()
        {
            listener.Start();
            IsRunning = true;
            Console.WriteLine($"服务器已启动，监听端口 {config.Port}");
            listener.BeginGetContext(OnRequest, null);
        }

        public void Stop()
        {
            IsRunning = false;
            listener.Stop();
        }

        private void OnRequest(IAsyncResult ar)
        {
            if (!listener.IsListening) return;

            var context = listener.EndGetContext(ar);
            listener.BeginGetContext(OnRequest, null);

            string path = context.Request.Url.AbsolutePath.Trim('/').ToLower();
            string responseText = "";

            switch (path)
            {
                case "dianliang":
                    responseText = GetBatteryStatus();
                    break;
                case "status":
                    responseText = JsonConvert.SerializeObject(Server.Status(), Formatting.Indented);
                    break;
                default:
                    responseText = JsonConvert.SerializeObject(new { Error = "未知命令" });
                    break;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(responseText);
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        private string GetBatteryStatus()
        {
            var status = System.Windows.Forms.SystemInformation.PowerStatus;
            var info = new
            {
                Percent = status.BatteryLifePercent * 100,
                Charging = status.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online,
                RemainingSeconds = status.BatteryLifeRemaining
            };
            return JsonConvert.SerializeObject(info, Formatting.Indented);
        }
    }
}

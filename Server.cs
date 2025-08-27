using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

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
                Port = _server.Port,
                Runtime = Common.GetRunTime()
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
            try
            {

                if (!listener.IsListening) return;

                var context = listener.EndGetContext(ar);
                listener.BeginGetContext(OnRequest, null);

                string path = context.Request.Url.AbsolutePath.Trim('/').ToLower();
                string responseText = "";

                string catchDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "catch");

                switch (path)
                {
                    case "":
                    case "/":
                        // 默认跳转到 help
                        context.Response.Redirect("/help");
                        break;


                    case "all":
                        responseText = Common.AllStatus();
                        break;

                    case "wl":
                        responseText = GetNetStatus();
                        break;

                    case "dl":
                        responseText = GetBatteryStatus();
                        break;

                    case "wz":
                        responseText = Common.GetLocationStatus();
                        break;

                    case "zt":
                        responseText = JsonConvert.SerializeObject(Server.Status(), Formatting.Indented);
                        break;

                    case "getcamera":
                        responseText = TakePhoto();
                        break;

                    case "getphoto":
                        string file = context.Request.QueryString["file"];
                        if (string.IsNullOrEmpty(file))
                        {
                            WriteJsonResponse(context, JsonConvert.SerializeObject(new { success = false, error = "缺少 file 参数" }));
                            return;
                        }

                        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "catch", file);
                        if (!File.Exists(filePath))
                        {
                            WriteJsonResponse(context, JsonConvert.SerializeObject(new { success = false, error = "文件不存在" }));
                            return;
                        }

                        WriteImageResponse(context, filePath);
                        return;

                    case "getphotonow":
                        TakePhoto();
                        Thread.Sleep(2000);
                        if (!Directory.Exists(catchDir))
                        {
                            WriteJsonResponse(context, JsonConvert.SerializeObject(new { success = false, error = "还没有任何照片" }));
                            return;
                        }

                        var files = new DirectoryInfo(catchDir)
                            .GetFiles("*.jpg")
                            .OrderByDescending(f => f.LastWriteTime)
                            .ToList();

                        if (files.Count == 0)
                        {
                            WriteJsonResponse(context, JsonConvert.SerializeObject(new { success = false, error = "没有找到照片" }));
                            return;
                        }

                        WriteImageResponse(context, files.First().FullName);
                        return;

                    case "getlatestphoto":

                        if (!Directory.Exists(catchDir))
                        {
                            WriteJsonResponse(context, JsonConvert.SerializeObject(new { success = false, error = "还没有任何照片" }));
                            return;
                        }

                        var filess = new DirectoryInfo(catchDir)
                            .GetFiles("*.jpg")
                            .OrderByDescending(f => f.LastWriteTime)
                            .ToList();

                        if (filess.Count == 0)
                        {
                            WriteJsonResponse(context, JsonConvert.SerializeObject(new { success = false, error = "没有找到照片" }));
                            return;
                        }

                        WriteImageResponse(context, filess.First().FullName);
                        return;

                    case "help":
                        responseText = Common.getApiHtml(context.Request);
                        break;

                    default:
                        responseText = JsonConvert.SerializeObject(new { Error = "未知命令, 请求/help获取全部接口" });
                        break;
                }

                byte[] buffer = Encoding.UTF8.GetBytes(responseText);
                context.Response.ContentType = path == "help" ? "text/html; charset=utf-8" : "application/json";
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        private string GetBatteryStatus()
        {
            var status = System.Windows.Forms.SystemInformation.PowerStatus;
            var info = new
            {
                Percent = status.BatteryLifePercent * 100,
                Charging = status.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online,
                RemainingMin = status.BatteryLifeRemaining > 0 ? status.BatteryLifeRemaining / 60 : -1,
            };
            return JsonConvert.SerializeObject(info, Formatting.Indented);
        }

        private string GetNetStatus()
        {
            string re = $"{(Common.IsInternetAvailable() ? "在线" : "离线")}\n";
            return re;
        }

        private string TakePhoto()
        {
            string catchDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "catch");
            if (!System.IO.Directory.Exists(catchDir))
                System.IO.Directory.CreateDirectory(catchDir);

            string filename = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            string filepath = System.IO.Path.Combine(catchDir, filename);

            try
            {
                // 打开第一个可用摄像头
                var videoDevices = new Accord.Video.DirectShow.FilterInfoCollection(Accord.Video.DirectShow.FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                    throw new Exception("没有检测到摄像头");

                var videoSource = new Accord.Video.DirectShow.VideoCaptureDevice(videoDevices[0].MonikerString);

                System.Drawing.Bitmap frame = null;
                var wait = new System.Threading.AutoResetEvent(false);

                int frameCount = 0;

                // 绑定事件：丢掉前几帧，等画面稳定
                videoSource.NewFrame += (s, e) =>
                {
                    frameCount++;
                    if (frameCount > 3 && frame == null) // 丢弃前3帧
                    {
                        frame = (System.Drawing.Bitmap)e.Frame.Clone();
                        wait.Set();
                    }
                };

                videoSource.Start();

                // 等待最多 5 秒钟获取画面
                if (wait.WaitOne(5000) && frame != null)
                {
                    frame.Save(filepath, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                else
                {
                    throw new Exception("摄像头未能捕获画面");
                }

                videoSource.SignalToStop();
                videoSource.WaitForStop();

                return JsonConvert.SerializeObject(new { success = true, path = filepath }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { success = false, error = ex.Message }, Formatting.Indented);
            }
        }


        private static void WriteImageResponse(HttpListenerContext context, string filePath)
        {
            byte[] buffer = File.ReadAllBytes(filePath);
            context.Response.ContentType = "image/jpeg";
            context.Response.ContentLength64 = buffer.Length;

            using (var output = context.Response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
        }


        private static void WriteJsonResponse(HttpListenerContext context, string json)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

    }
}

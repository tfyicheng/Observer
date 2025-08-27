using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Interop;
using Application = System.Windows.Application;
using PowerLineStatus = System.Windows.Forms.PowerLineStatus;

namespace Observer
{
    public static class Common
    {
        public static DateTime? _startTime;

        public static NgrokHelper ngrok;

        public static myIcon ic = new myIcon();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <summary>
        /// 设置窗体前置
        /// </summary>
        /// <param name="windowname"></param>
        public static void SetWindowTop(string windowname)
        {
            try
            {
                foreach (var item in Application.Current.Windows)
                {
                    //Console.WriteLine(item.ToString());
                    if (item.ToString() == windowname)
                    {
                        //Console.WriteLine("设置主窗体");
                        System.Windows.Window w = (System.Windows.Window)item;
                        w.WindowState = System.Windows.WindowState.Normal;
                        Common.SetWindowToForegroundWithAttachThreadInput(w);
                    }
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }

        }

        /// <summary>
        /// 设置窗体激活前置
        /// </summary>
        /// <param name="window"></param>
        public static void SetWindowToForegroundWithAttachThreadInput(System.Windows.Window window)
        {
            var interopHelper = new WindowInteropHelper(window);
            var thisWindowThreadId = GetWindowThreadProcessId(interopHelper.Handle, IntPtr.Zero);
            var currentForegroundWindow = GetForegroundWindow();
            var currentForegroundWindowThreadId = GetWindowThreadProcessId(currentForegroundWindow, IntPtr.Zero);

            AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, true);

            window.Show();
            window.Activate();
            // 去掉和其他线程的输入链接
            AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, false);
            // 用于踢掉其他的在上层的窗口
            window.Topmost = true;
            window.Topmost = false;
        }


        public static string ApiStatus => getApi(); // 自动调用方法

        //获取接口说明
        public static string getApi()
        {
            return $"/all: 全部信息\n/dl: 电量\n/wz: 位置\n/wl: 网络\n/zt: 服务状态\n/getcamera: 拍照\n/getphotonow: 拍照并返回结果\n/getphoto: 获取拍照结果(?file=文件名)\n/getlatestphoto: 获取最后一次拍照结果\n";
        }

        //获取接口说明（html）
        public static string getApiHtml(HttpListenerRequest request)
        {
            string host = request.Url.Host;
            int port = request.Url.Port;

            return $@"
<html>
<head>
    <meta charset='utf-8'>
    <title>API 帮助</title>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 20px; }}
        h2 {{ color: #333; }}
        ul {{ line-height: 1.8; }}
        a {{ text-decoration: none; color: #0078d7; }}
        a:hover {{ text-decoration: underline; }}
    </style>
</head>
<body>
    <h2>API 接口说明</h2>
    <ul>
        <li><a href='http://{host}/all'>/all</a> : 全部信息</li>
        <li><a href='http://{host}/dl'>/dl</a> : 电量</li>
        <li><a href='http://{host}/wz'>/wz</a> : 位置</li>
        <li><a href='http://{host}/wl'>/wl</a> : 网络</li>
        <li><a href='http://{host}/zt'>/zt</a> : 服务状态</li>
        <li><a href='http://{host}/getcamera'>/getcamera</a> : 拍照</li>
        <li><a href='http://{host}/getphotonow'>/getphotonow</a> : 拍照并返回结果</li>
        <li>/getphoto?file=文件名 : 获取拍照结果</li>
        <li><a href='http://{host}/getlatestphoto'>/getlatestphoto</a> : 获取最后一次拍照结果</li>
    </ul>
</body>
</html>";
        }


        //获取电量情况
        public static void PrintBatteryStatus()
        {
            PowerStatus status = SystemInformation.PowerStatus;

            // 电量百分比（0.0 ~ 1.0）
            float batteryLifePercent = status.BatteryLifePercent * 100;

            // 电池剩余使用时间（秒），-1 表示未知
            int batteryLifeRemaining = status.BatteryLifeRemaining;

            // 电源状态：Online / Offline
            PowerLineStatus lineStatus = status.PowerLineStatus;

            Console.WriteLine($"电量: {batteryLifePercent}%");
            Console.WriteLine($"剩余使用时间: {(batteryLifeRemaining > 0 ? batteryLifeRemaining / 60 + "分钟" : "未知")}");
            Console.WriteLine($"电源状态: {lineStatus}");
        }

        //查询网络状态
        public static bool IsInternetAvailable()
        {
            string[] testHosts = { "114.114.114.114", "223.5.5.5", "180.76.76.76" };

            try
            {
                using (var ping = new Ping())
                {
                    foreach (var host in testHosts)
                    {
                        PingReply reply = ping.Send(host, 3000);
                        if (reply.Status == IPStatus.Success)
                        {
                            return true; // 任意一个通就算网络可用
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public static TimeSpan GetIdleTime()
        {
            LASTINPUTINFO info = new LASTINPUTINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            if (GetLastInputInfo(ref info))
            {
                uint idleTicks = (uint)Environment.TickCount - info.dwTime;
                return TimeSpan.FromMilliseconds(idleTicks);
            }
            return TimeSpan.Zero;
        }

        public static bool HasUserActivityWithin(int seconds)
        {
            return GetIdleTime().TotalSeconds < seconds;
        }

        //查询定位（api）
        public class IpApiResponse
        {
            public string Country { get; set; }
            public string RegionName { get; set; }
            public string City { get; set; }
            public float Lat { get; set; }
            public float Lon { get; set; }
            public string Query { get; set; }
        }

        public static IpApiResponse GetLocation()
        {
            using (var client = new HttpClient())
            {
                var json = client.GetStringAsync("http://ip-api.com/json/").Result;
                return JsonConvert.DeserializeObject<IpApiResponse>(json);
            }
        }

        public static string GetLocationStatus()
        {
            var loc = GetLocation();
            return $"{loc.Country}/{loc.RegionName}/{loc.City}, 经纬度({loc.Lat},{loc.Lon}), 查询ip({loc.Query})";
        }

        //获取运行时间
        public static string GetRunTime()
        {
            var uptime = DateTime.Now - (_startTime ?? DateTime.Now);
            return string.Format("{0:D2}:{1:D2}:{2:D2}",
            (int)uptime.TotalHours, uptime.Minutes, uptime.Seconds);
        }

        //获取所有状态
        public static string AllStatus()
        {
            var status = System.Windows.Forms.SystemInformation.PowerStatus;
            string re = $"运行时间：{GetRunTime()}\n电量：{status.BatteryLifePercent * 100}\n充电状态：{(status.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online ? "充电中" : "放电中")}\n预计可用时间(min)：{(status.BatteryLifeRemaining > 0 ? status.BatteryLifeRemaining / 60 : -1)}\n网络状态：{(Common.IsInternetAvailable() ? "在线" : "离线")}\n键鼠状态：{(Common.HasUserActivityWithin(5) ? "触发" : "静置")}\n网络定位：{GetLocationStatus()}";
            return re;
        }



    }
}

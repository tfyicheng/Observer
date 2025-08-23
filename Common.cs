using System;
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

    }
}

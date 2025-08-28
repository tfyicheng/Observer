using System;
using System.Runtime.InteropServices;
namespace Observer
{
    public static class LockScreenHelper
    {
        private const int UOI_USER_SID = 4;

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("user32.dll")]
        private static extern bool LockWorkStation();

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSRegisterSessionNotification(IntPtr hWnd, int dwFlags);

        [DllImport("user32.dll")]
        private static extern bool OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll")]
        private static extern IntPtr GetThreadDesktop(uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetProcessWindowStation();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SwitchDesktop(IntPtr hDesktop);

        // 判断系统是否锁屏
        public static bool IsLocked()
        {
            IntPtr hwnd = GetProcessWindowStation();
            return !SwitchDesktop(hwnd);
        }

        // 获取用户空闲时长
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

        // 🔑 关键方法：判断锁屏下是否有操作
        public static bool HasLockedInputWithin(int seconds)
        {
            if (!IsLocked()) return false; // 没有锁屏直接 false

            return GetIdleTime().TotalSeconds < seconds;
        }
    }
}

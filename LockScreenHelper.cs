using Observer;
using System;
using System.Runtime.InteropServices;

public static class LockScreenHelper
{
    // ------------------ WTS (锁屏状态) ------------------
    private enum WTS_INFO_CLASS
    {
        // 文档：WTSSessionInfoEx 的枚举值是 25
        WTSSessionInfoEx = 25
    }

    // SessionFlags 的取值（Win7/2008R2 与现代系统有一次反转，下方代码会处理）
    private const int WTS_SESSIONSTATE_UNKNOWN = unchecked((int)0xFFFFFFFF);
    private const int WTS_SESSIONSTATE_LOCK = 0;
    private const int WTS_SESSIONSTATE_UNLOCK = 1;

    [DllImport("wtsapi32.dll", SetLastError = true)]
    private static extern bool WTSQuerySessionInformation(
        IntPtr hServer,
        int sessionId,
        WTS_INFO_CLASS wtsInfoClass,
        out IntPtr ppBuffer,
        out int pBytesReturned);

    [DllImport("wtsapi32.dll")]
    private static extern void WTSFreeMemory(IntPtr pMemory);

    [DllImport("kernel32.dll")]
    private static extern int WTSGetActiveConsoleSessionId();

    // 只把我们关心的头几个字段映射出来即可（顺序/大小与原生一致）
    [StructLayout(LayoutKind.Sequential)]
    private struct WTSINFOEX_LEVEL1
    {
        public int SessionId;
        public int SessionState;   // WTS_CONNECTSTATE_CLASS
        public int SessionFlags;   // LOCK/UNLOCK 状态就看这个
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WTSINFOEX_LEVEL
    {
        public WTSINFOEX_LEVEL1 WTSInfoExLevel1;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WTSINFOEX
    {
        public int Level;              // 期望为 1
        public WTSINFOEX_LEVEL Data;   // union，但这里只有 Level1
    }

    /// <summary>是否处于锁屏状态（可靠查询当前状态，不是事件）。</summary>
    public static bool IsWorkstationLocked()
    {
        IntPtr buffer = IntPtr.Zero;
        int bytes = 0;

        // 读当前活动会话的扩展信息
        int sessionId = WTSGetActiveConsoleSessionId();
        if (sessionId < 0) return false;

        if (!WTSQuerySessionInformation(IntPtr.Zero, sessionId,
                WTS_INFO_CLASS.WTSSessionInfoEx, out buffer, out bytes) || buffer == IntPtr.Zero)
        {
            return false;
        }

        try
        {
            var info = (WTSINFOEX)Marshal.PtrToStructure(buffer, typeof(WTSINFOEX));

            if (info.Level != 1) return false;

            int flags = info.Data.WTSInfoExLevel1.SessionFlags;

            // Win7/Server 2008 R2（6.1）有一次反转：LOCK/UNLOCK 值相反
            bool isWin7 = Environment.OSVersion.Version.Major == 6 &&
                          Environment.OSVersion.Version.Minor == 1;

            if (flags == WTS_SESSIONSTATE_UNKNOWN) return false; // 不知道就按未锁处理

            bool locked = (flags == WTS_SESSIONSTATE_LOCK);
            if (isWin7) locked = !locked;

            return locked;
        }
        finally
        {
            WTSFreeMemory(buffer);
        }
    }

    // ------------------ 最后输入时间（是否有活动） ------------------
    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    private static TimeSpan GetIdleTime()
    {
        LASTINPUTINFO li = new LASTINPUTINFO { cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO)) };
        if (!GetLastInputInfo(ref li)) return TimeSpan.Zero;

        uint tick = (uint)Environment.TickCount;
        uint idle = tick - li.dwTime;
        return TimeSpan.FromMilliseconds(idle);
    }

    /// <summary>
    /// 锁屏中且 N 秒内有键鼠活动
    /// </summary>
    public static bool LockedAndActiveWithinSeconds(int seconds)
    {
        if (!Common.lockStatus) return false;
        return GetIdleTime().TotalSeconds < seconds;
    }

    public static bool ActiveWithinSeconds(int seconds)
    {
        return GetIdleTime().TotalSeconds < seconds;
    }
}

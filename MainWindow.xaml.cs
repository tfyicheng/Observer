using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Observer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int NOTIFY_FOR_THIS_SESSION = 0;
        private HwndSource _hwndSource;
        public MainWindow()
        {
            InitializeComponent();
            Common.model = ConfigManager.Load();
            Common.model.initTimer();
            this.DataContext = Common.model;
            Common.ic.Icon();
            //订阅属性变化，触发保存
            Common.model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(MainModel.RunTime))
                {
                    ConfigManager.SaveThrottled(Common.model);
                }
            };

            Common.model.initServer();
            Logger.WriteLine("程序启动");
            if (Common.model.Run3)
            {
                Logger.CleanOldLogs();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public void StartProCheck()
        {
            System.Diagnostics.Process[] myProcess = System.Diagnostics.Process.GetProcessesByName("Observer");
            if (myProcess == null)
            {
                return;

            }

            if (myProcess.Length > 1)
            {
                MessageBoxResult result = HandyControl.Controls.MessageBox.Show("是否继续运行？", "检测到后台存在其他实例", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                if (result == MessageBoxResult.OK)
                {
                    Process proceMain = Process.GetCurrentProcess();
                    //Console.WriteLine(proceMain.ProcessName +"="+ proceMain.Id);
                    Process[] processes = Process.GetProcesses();
                    foreach (Process process in processes)//获取所有同名进程id
                    {
                        //Console.WriteLine(process.ProcessName+"="+process.Id);
                        if (process.ProcessName == "Observer")
                        {
                            if (process.Id != proceMain.Id)//根据进程id删除所有除本进程外的所有相同进程
                            {
                                process.Kill();
                                return;
                            }
                        }
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true; // 标记为已处理

                this.Close();
            }
        }


        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            StartProCheck();
            _hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            _hwndSource.AddHook(WndProc);
            IntPtr hwnd = _hwndSource.Handle;
            bool success = WTSRegisterSessionNotification(hwnd, NOTIFY_FOR_THIS_SESSION);
            if (!success)
            {
                Console.WriteLine("注册会话通知失败。");
                Common.lockStatus = false;
            }
        }

        private void Main_Closed(object sender, EventArgs e)
        {
            if (_hwndSource != null)
            {
                IntPtr hwnd = _hwndSource.Handle;
                WTSUnRegisterSessionNotification(hwnd);
                _hwndSource.RemoveHook(WndProc);
                _hwndSource = null;
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x02B1) // WM_WTSSESSION_CHANGE
            {
                switch (wParam.ToInt32())
                {
                    case 0x7: // WTS_SESSION_LOCK
                        //MessageBox.Show("系统已锁屏"); 
                        Common.lockStatus = true;
                        break;
                    case 0x8: // WTS_SESSION_UNLOCK
                        //MessageBox.Show("系统已解锁");
                        Common.lockStatus = false;
                        break;
                }
            }
            return IntPtr.Zero;
        }

        #region Windows API
        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSRegisterSessionNotification(IntPtr hWnd, int dwFlags);

        [DllImport("wtsapi32.dll")]
        private static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);

        private const int WM_WTSSESSION_CHANGE = 0x02B1;
        #endregion
    }
}

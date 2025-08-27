using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Timer = System.Timers.Timer;

namespace Observer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainModel model;
        private static Timer timer;
        private static float lastBatteryPercent = -1;
        public MainWindow()
        {
            InitializeComponent();
            model = ConfigManager.Load();
            this.DataContext = model;
            Common.ic.Icon();
            //订阅属性变化，触发保存
            model.PropertyChanged += (s, e) => ConfigManager.Save(model);
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            StartProCheck();
            startObserver();
        }


        private void startObserver()
        {
            //timer = new Timer(5000); // 5 秒检查一次
            //timer.Elapsed += CheckStatus;
            //timer.AutoReset = true;
            //timer.Start();
            Observer.Server.Start(8086);  // 启动
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private static void CheckStatus(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("----------");
            Common.PrintBatteryStatus();
            if (Common.HasUserActivityWithin(5)) // 5秒内有输入
            {
                Console.WriteLine("检测到键盘/鼠标被操作！");
            }
            Console.WriteLine(Common.IsInternetAvailable());
            return;
            PowerStatus status = SystemInformation.PowerStatus;
            float currentPercent = status.BatteryLifePercent * 100;

            if (lastBatteryPercent != -1 && currentPercent != lastBatteryPercent)
            {
                Console.WriteLine($"电量变化: {currentPercent}%");
                // TODO: 在这里触发 Webhook 通知
            }

            lastBatteryPercent = currentPercent;
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


        public void StartNgrok()
        {
            // 从配置读取
            string ngrokPath = @"D:\soft\devTools\Ngrok\ngrok.exe";
            int localPort = 8086;

            Common.ngrok = new NgrokHelper(ngrokPath);
            string publicUrl = Common.ngrok.StartTunnel(localPort);

            Console.WriteLine("公网地址: " + publicUrl);
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true; // 标记为已处理

                this.Close();
            }
        }

        private void test(object sender, RoutedEventArgs e)
        {
            StartNgrok();
        }

        private void test2(object sender, RoutedEventArgs e)
        {
            Common.ngrok.StopTunnel();
        }
    }
}

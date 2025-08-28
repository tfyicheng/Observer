using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Observer
{
    public class MainModel : System.ComponentModel.INotifyPropertyChanged
    {
        private static Timer _timer;
        private DateTime _startTime;
        private bool _isRunning = true;
        private int _tickCount = 0; // 记录定时器触发次数
        private int limite4Count = 0;
        private int limite4CountTotal = 0;

        public MainModel()
        {
            Command();
        }

        public void initTimer()
        {
            // 初始化开始时间
            _startTime = DateTime.Now;
            Common._startTime = DateTime.Now; // 记录启动时间
            RunTime = "00:00:00";
            // 创建 DispatcherTimer（可在 UI 线程更新）
            _timer = new Timer(1000); // 每秒执行一次
            _timer.Elapsed += Timer_Tick;
            _timer.AutoReset = true;
            _timer.Start();
        }

        public void initServer()
        {
            if (Run1)
            {
                StartServer();
            }
        }

        public String[] requestTypes { get; set; } = new String[] { "Get", "Post" };

        private string _runTime;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
       PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string RunTime
        {
            get => _runTime;
            set
            {
                _runTime = value;
                OnPropertyChanged(nameof(RunTime));
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                TimeSpan elapsed = DateTime.Now - _startTime;
                RunTime = FormatTimeSpan(elapsed);
            }

            _tickCount++;
            limite4Count++;

            if (_tickCount >= 10)
            {
                _tickCount = 0; // 重置计数

                CheckAllEnable();

                Console.WriteLine("----------");
                Common.PrintBatteryStatus();
                if (Common.HasUserActivityWithin(5)) // 5秒内有输入
                {
                    Console.WriteLine("检测到键盘/鼠标被操作！");
                }
                Console.WriteLine(Common.IsInternetAvailable() ? "联网" : "断网");
            }

            if (Enable4 && Limite4 * 60 > 0)
            {
                if (limite4Count >= Limite4 * 60)
                {
                    CheckEnable4();
                    limite4Count = 0;
                }
            }
        }

        private void CheckAllEnable()
        {
            if (Enable1)
            {
                CheckEnable1();
            }

            if (Enable2)
            {
                CheckEnable2();
            }


            if (Enable3)
            {
                CheckEnable3();
            }

        }

        private void CheckEnable1()
        {
            string dl = Common.PrintBatteryStatus("电量");

            if (int.Parse(dl) < Limite1)
            {
                Common.trigger = "电量";
                Common.result = dl;
                if (Enable11)
                {
                    SendReqt(RequestType1, RequestApi1, RequestBody1);
                }
                else
                {
                    SendReq();
                }
            }
        }

        private string lastEb2 = "";

        private void CheckEnable2()
        {
            string dl = Common.PrintBatteryStatus("电源状态");

            if (lastEb2 != dl)
            {
                if (lastEb2 == "")
                {
                    lastEb2 = dl;
                    return;
                }
                lastEb2 = dl;
                Common.trigger = "充电状态变化";
                Common.result = dl;
                if (Enable22)
                {
                    SendReqt(RequestType2, RequestApi2, RequestBody2);
                }
                else
                {
                    SendReq();
                }
            }
        }

        private void CheckEnable3()
        {
            if (Common.HasUserActivityWithin(5))
            {
                Common.trigger = "键鼠状态";
                Common.result = "检测到键盘/鼠标被操作！";
                if (Enable33)
                {
                    SendReqt(RequestType3, RequestApi3, RequestBody3);
                }
                else
                {
                    SendReq();
                }
            }
        }

        private void CheckEnable4()
        {
            limite4CountTotal += 1;
            Common.trigger = "定时触发";
            Common.result = $"累计执行次数{limite4CountTotal}";
            if (Enable44)
            {
                SendReqt(RequestType4, RequestApi4, RequestBody4);
            }
            else
            {
                SendReq();
            }
        }

        async private void SendReq()
        {
            if (String.IsNullOrEmpty(RequestType0) || String.IsNullOrEmpty(RequestApi0))
            {
                return;
            }
            string result = await Server.SendAsync(RequestType0, RequestApi0, RequestBody0);
            //HandyControl.Controls.Growl.Info(result);
        }

        async private void SendReqt(string t, string a, string b = "")
        {
            if (String.IsNullOrEmpty(t) || String.IsNullOrEmpty(a))
            {
                return;
            }
            string result = await Server.SendAsync(t, a, b);
            //HandyControl.Controls.Growl.Info(result);
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        #region 页面配置字段

        private string requestApi0;
        private string requestType0;
        private string requestBody0;

        private bool enable1;
        private bool enable11;
        private int limite1;
        private string requestApi1;
        private string requestType1;
        private string requestBody1;

        private bool enable2;
        private bool enable22;
        private string requestApi2;
        private string requestType2;
        private string requestBody2;

        private bool enable3;
        private bool enable33;
        private string requestApi3;
        private string requestType3;
        private string requestBody3;


        private bool enable4;
        private bool enable44;
        private int limite4;
        private string requestApi4;
        private string requestType4;
        private string requestBody4;

        private int runStatus;//0 未启动，1 启动中，2 已启动
        private string port;
        private string path;
        private string link;

        private bool run0;
        private bool run1;


        public string RequestApi0
        {
            get => requestApi0;
            set { requestApi0 = value; OnPropertyChanged(nameof(RequestApi0)); }
        }

        public string RequestType0
        {
            get => requestType0;
            set { requestType0 = value; OnPropertyChanged(nameof(RequestType0)); }
        }

        public string RequestBody0
        {
            get => requestBody0;
            set { requestBody0 = value; OnPropertyChanged(nameof(RequestBody0)); }
        }


        public bool Enable1
        {
            get => enable1;
            set { enable1 = value; OnPropertyChanged(nameof(Enable1)); }
        }

        public bool Enable11
        {
            get => enable11;
            set { enable11 = value; OnPropertyChanged(nameof(Enable11)); }
        }

        public int Limite1
        {
            get => limite1;
            set { limite1 = value; OnPropertyChanged(nameof(Limite1)); }
        }


        public string RequestApi1
        {
            get => requestApi1;
            set { requestApi1 = value; OnPropertyChanged(nameof(RequestApi1)); }
        }

        public string RequestType1
        {
            get => requestType1;
            set { requestType1 = value; OnPropertyChanged(nameof(RequestType1)); }
        }

        public string RequestBody1
        {
            get => requestBody1;
            set { requestBody1 = value; OnPropertyChanged(nameof(RequestBody1)); }
        }

        public bool Enable2
        {
            get => enable2;
            set { enable2 = value; OnPropertyChanged(nameof(Enable2)); }
        }

        public bool Enable22
        {
            get => enable22;
            set { enable22 = value; OnPropertyChanged(nameof(Enable22)); }
        }

        public string RequestApi2
        {
            get => requestApi2;
            set { requestApi2 = value; OnPropertyChanged(nameof(RequestApi2)); }
        }

        public string RequestType2
        {
            get => requestType2;
            set { requestType2 = value; OnPropertyChanged(nameof(RequestType2)); }
        }

        public string RequestBody2
        {
            get => requestBody2;
            set { requestBody2 = value; OnPropertyChanged(nameof(RequestBody2)); }
        }

        public bool Enable3
        {
            get => enable3;
            set { enable3 = value; OnPropertyChanged(nameof(Enable3)); }
        }

        public bool Enable33
        {
            get => enable33;
            set { enable33 = value; OnPropertyChanged(nameof(Enable33)); }
        }

        public string RequestApi3
        {
            get => requestApi3;
            set { requestApi3 = value; OnPropertyChanged(nameof(RequestApi3)); }
        }

        public string RequestType3
        {
            get => requestType3;
            set { requestType3 = value; OnPropertyChanged(nameof(RequestType3)); }
        }

        public string RequestBody3
        {
            get => requestBody3;
            set { requestBody3 = value; OnPropertyChanged(nameof(RequestBody3)); }
        }

        public bool Enable4
        {
            get => enable4;
            set { enable4 = value; OnPropertyChanged(nameof(Enable4)); }
        }

        public bool Enable44
        {
            get => enable44;
            set { enable44 = value; OnPropertyChanged(nameof(Enable44)); }
        }

        public int Limite4
        {
            get => limite4;
            set { limite4 = value; OnPropertyChanged(nameof(Limite4)); }
        }


        public string RequestApi4
        {
            get => requestApi4;
            set { requestApi4 = value; OnPropertyChanged(nameof(RequestApi4)); }
        }

        public string RequestType4
        {
            get => requestType4;
            set { requestType4 = value; OnPropertyChanged(nameof(RequestType4)); }
        }

        public string RequestBody4
        {
            get => requestBody4;
            set { requestBody4 = value; OnPropertyChanged(nameof(RequestBody4)); }
        }

        public int RunStatus
        {
            get => runStatus;
            set { runStatus = value; OnPropertyChanged(nameof(RunStatus)); }
        }

        public string Port
        {
            get => port;
            set { port = value; OnPropertyChanged(nameof(Port)); }
        }

        public string Path
        {
            get => path;
            set { path = value; OnPropertyChanged(nameof(Path)); }
        }

        public string Link
        {
            get => link;
            set { link = value; OnPropertyChanged(nameof(Link)); }
        }

        public bool Run0
        {
            get => run0;
            set
            {
                if (value)
                {
                    Common.EnableStartup();
                }
                else
                {
                    Common.DisableStartup();
                }
                run0 = value;
                OnPropertyChanged(nameof(Run0));
            }
        }

        public bool Run1
        {
            get => run1;
            set { run1 = value; OnPropertyChanged(nameof(Run1)); }
        }

        #endregion

        #region 命令

        public void Command()
        {
            SendRequestCommand = new RelayCommand(_ => SendRequest());
            GetPathCommand = new RelayCommand(_ => GetPath());
            CopyLinkCommand = new RelayCommand(_ => CopyLink());
            StartServerCommand = new RelayCommand(_ => StartServer());
            StopServerCommand = new RelayCommand(_ => StopServer());
        }

        public ICommand SendRequestCommand { get; set; }
        public ICommand GetPathCommand { get; set; }
        public ICommand CopyLinkCommand { get; set; }
        public ICommand StartServerCommand { get; set; }
        public ICommand StopServerCommand { get; set; }


        async private void SendRequest()
        {
            if (String.IsNullOrEmpty(RequestType0) || String.IsNullOrEmpty(RequestApi0))
            {
                HandyControl.Controls.Growl.Warning("请先设置请求配置");
                return;
            }
            string result = await Server.SendAsync(RequestType0, RequestApi0, RequestBody0);
            HandyControl.Controls.Growl.Info(result);
        }


        private void GetPath()
        {
            var dialog = new OpenFileDialog
            {
                Title = "选择程序",
                Filter = "可执行文件 (*.exe)|*.exe",
                CheckFileExists = true,
                Multiselect = false
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                Path = dialog.FileName;
            }
        }

        private void CopyLink()
        {
            if (string.IsNullOrEmpty(Link))
                return;

            Clipboard.SetText(Link);
            HandyControl.Controls.Growl.Info("已复制到剪切板");
        }

        private void StartServer()
        {
            if (String.IsNullOrEmpty(Port))
            {
                HandyControl.Controls.Growl.Warning("请先输入端口号");
                return;
            }

            if (String.IsNullOrEmpty(Path))
            {
                HandyControl.Controls.Growl.Warning("请先获取程序路径");
                return;
            }

            try
            {
                if (Common.IsValidPort(Port, out int port))
                {
                    RunStatus = 1;
                    Observer.Server.Start(port);
                    // 从配置读取
                    string ngrokPath = Path;
                    int localPort = port;
                    Common.ngrok = new NgrokHelper(ngrokPath);
                    string publicUrl = Common.ngrok.StartTunnel(localPort);
                    Link = publicUrl;
                    Console.WriteLine("公网地址: " + publicUrl);
                    //System.Threading.Thread.Sleep(3000);
                    RunStatus = 2;
                }
                else
                {
                    RunStatus = 0;
                    HandyControl.Controls.Growl.Warning("非法端口");
                }
            }
            catch (Exception ee)
            {
                RunStatus = 0;
                HandyControl.Controls.Growl.Warning("启动异常：" + ee.Message);
            }
        }


        private void StopServer()
        {
            RunStatus = 1;
            Observer.Server.Stop();
            Common.ngrok.StopTunnel();
            Link = "";
            //System.Threading.Thread.Sleep(3000);
            RunStatus = 0;
        }

        #endregion
    }
}

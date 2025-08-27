using System;
using System.ComponentModel;
using System.Timers;

namespace Observer
{
    public class MainModel : System.ComponentModel.INotifyPropertyChanged
    {
        private static Timer _timer;
        private DateTime _startTime;
        private bool _isRunning = true;
        private int _tickCount = 0; // 记录定时器触发次数

        public MainModel()
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

        public String[] requestType { get; set; } = new String[] { "Get", "Post" };

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

            if (_tickCount >= 5)
            {
                _tickCount = 0; // 重置计数

                Console.WriteLine("----------");
                Common.PrintBatteryStatus();
                if (Common.HasUserActivityWithin(5)) // 5秒内有输入
                {
                    Console.WriteLine("检测到键盘/鼠标被操作！");
                }
                Console.WriteLine(Common.IsInternetAvailable());
            }
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }



        private bool enable1;
        private string requestApi0;
        private string requestType0;

        public bool Enable1
        {
            get => enable1;
            set { enable1 = value; OnPropertyChanged(nameof(Enable1)); }
        }

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

    }
}

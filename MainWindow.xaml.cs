using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Observer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

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
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            StartProCheck();
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

    }
}

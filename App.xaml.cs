using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;


namespace Observer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : System.Windows.Application
    {
    }

    public class myIcon
    {
        //任务栏图标
        System.Windows.Forms.NotifyIcon notifyIcon = null;

        public void Icon()
        {
            //创建图标
            this.notifyIcon = new System.Windows.Forms.NotifyIcon();

            //程序打开时任务栏会有小弹窗
            //this.notifyIcon.BalloonTipText = "running...";

            //鼠标放在图标上时显示的文字
            this.notifyIcon.Text = "Observer-视奸助手V1.0👻";

            //图标图片的位置，注意这里要用绝对路径
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("Observer.logo.ico"))
            {
                if (stream != null)
                {
                    this.notifyIcon.Icon = new Icon(stream);
                }
                else
                {
                    // 资源名错误
                    throw new Exception("图标资源未找到。资源名应为：项目名.文件夹名.文件名.ico");
                }
            }

            //显示图标
            this.notifyIcon.Visible = true;

            // 添加鼠标点击事件：用于左键点击打开窗口
            this.notifyIcon.MouseClick += new MouseEventHandler(NotifyIcon_MouseClick);

            //右键菜单--退出菜单项
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("退出");
            exit.Click += new EventHandler(CloseWindow);

            //关联托盘控件
            System.Windows.Forms.MenuItem[] children = new System.Windows.Forms.MenuItem[] { exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(children);

            //this.notifyIcon.ShowBalloonTip(2000);
        }

        public void SendNotify(String title, String content, int time = 100000)
        {
            this.notifyIcon.BalloonTipTitle = title;
            this.notifyIcon.BalloonTipText = content;
            this.notifyIcon.Visible = true;
            this.notifyIcon.ShowBalloonTip(time);
        }

        //退出菜单项对应的处理方式
        public void CloseWindow(object sender, EventArgs e)
        {
            Common.ngrok?.StopTunnel();
            //Dispose()函数能够解决程序退出后图标还在，要鼠标划一下才消失的问题
            this.notifyIcon.Dispose();
            //关闭整个程序
            System.Windows.Application.Current.Shutdown();
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            // 判断是否为左键单击
            if (e.Button == MouseButtons.Left)
            {
                ShowMainWindow();
            }
        }

        private void ShowMainWindow()
        {
            // 确保在 UI 线程上执行
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var windows = System.Windows.Application.Current.Windows;

                for (int i = 0; i < windows.Count; i++)
                {
                    var win = windows[i];

                    if (win.GetType().Name == "MainWindow")
                    {
                        if (win.IsVisible)
                        {
                            Common.SetWindowTop("Observer.MainWindow");
                            //win.Activate(); // 激活窗口（置于前台）
                        }
                        else
                        {
                            win.Show();     // 显示窗口
                        }
                    }
                }
            });
        }
    }

}

using ExtendQQ_DGJModule.Exceptions;
using ExtendQQ_DGJModule.Services;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ExtendQQ_DGJModule
{
    /// <summary>
    /// Interaction logic for CookieLoginWindow.xaml
    /// </summary>
    public partial class CookieLoginWindow : Window
    {
        private readonly MainWindow _mainWindow;

        private readonly QQSession _session;

        public CookieLoginWindow(MainWindow mainWindow, QQSession session)
        {
            _mainWindow = mainWindow;
            _session = session;
            InitializeComponent();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            SubmitBtn.IsEnabled = false;
            string cookie = CookieBox.Text;
            CookieBox.Text = null;
            _ = SubmitCore(cookie);
        }

        private async Task SubmitCore(string cookie)
        {
            try
            {
                _session.SetCookie(cookie);
                _mainWindow.OnLoginStatusChanged();
                MessageBox.Show("登录成功OvO", "Cookie登录 - 管理界面 - 本地QQ音乐模块", 0, MessageBoxImage.Asterisk);
                this.Close();
                return;
            }
            catch (InvalidCookieException)
            {
                MessageBox.Show("你填写的Cookie似乎无效OAO", "Cookie登录 - 管理界面 - 本地QQ音乐模块", 0, MessageBoxImage.Warning);
            }
            catch (Exception e)
            {
                MessageBox.Show($"验证Cookie失败(´；ω；`)\r\n{e}", "Cookie登录 - 管理界面 - 本地QQ音乐模块", 0, MessageBoxImage.Error);
            }
            SubmitBtn.IsEnabled = true;
        }
    }
}

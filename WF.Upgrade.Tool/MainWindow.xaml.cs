using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WF.Upgrade.Tool.Backend;

namespace WF.Upgrade.Tool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            page_init();
            local_init();
        }

        private void page_init()
        {
            //cef.RegisterJsObject("callbackObj", new CallbackObjectForJs());
            cef.MenuHandler = new MenuHandler();
            //TODO:前端注册的JS对象统一，然后再分发到其他的Service。也可以改为new不同的cef，然后注册对应的Service。 
            //           
            cef.RegisterJsObject("CheckRuleService", new CheckRuleService());

            //cwb.Address = address;

            //cwb.RegisterJsObject("SiteInfoData", new SiteService());
            //cwb.

            //cef.PreviewTextInput += (o, e) =>
            //{
            //    foreach (var character in e.Text)
            //    {
            //        // 把每个字符向浏览器组件发送一遍
            //        cef.GetBrowser().GetHost().SendKeyEvent((int)WM.CHAR, (int)character, 0);
            //    }

            //    // 不让cef自己处理
            //    e.Handled = true;
            //};


        }

        private void local_init()
        {
            //本地库初始化            
            LocalDBService.init();
            //数据初始化
            CheckRuleService crs = new CheckRuleService();
            crs.InitCheckRule();
        }

        public class CallbackObjectForJs
        {
            public void showMessage(string msg)
            {
                MessageBox.Show(msg);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var Address = Environment.CurrentDirectory + @"\resource\html\site\index.html?Rand=" + DateTime.Now;
            //var Address = "http://10.5.106.25:1111/";
            //var Address = Environment.CurrentDirectory + @"\resource\html\site\index.html?Rand=" + DateTime.Now;

            cef.Address = Address;
            //cef.RegisterJsObject("SiteService", new SiteService());
        }

        private void before_up_Click(object sender, RoutedEventArgs e)
        {
            
            var Address = Environment.CurrentDirectory + @"\resource\html\check\index.html?Rand=" + DateTime.Now;
            //var Address = "http://10.5.106.25:1111/";
            //var Address = Environment.CurrentDirectory + @"\resource\html\site\index.html?Rand=" + DateTime.Now;

            cef.Address = Address;
        }

        private void after_up_Click(object sender, RoutedEventArgs e)
        {

        }

        private void form_check_Click(object sender, RoutedEventArgs e)
        {

        }

        private void file_check_Click(object sender, RoutedEventArgs e)
        {

        }

        private void iis_check_Click(object sender, RoutedEventArgs e)
        {

        }

        private void other_check_Click(object sender, RoutedEventArgs e)
        {

        }

        private void main_index_Click(object sender, RoutedEventArgs e)
        {

        }

        private void main_db_Click(object sender, RoutedEventArgs e)
        {

        }

        private void main_fl_Click(object sender, RoutedEventArgs e)
        {

        }

        private void main_up_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

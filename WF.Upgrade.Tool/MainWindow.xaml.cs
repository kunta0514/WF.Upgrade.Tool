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
            init();
        }

        private void init()
        {
            var Address = Environment.CurrentDirectory + @"\view\html\site\index.html?Rand=" + DateTime.Now;
            //var Address = "http://10.5.106.25:1111/";
            //cef.Address = Environment.CurrentDirectory + @"\view\html\site\index.html?Rand=" + DateTime.Now;

            cef.Address = Address;

            cef.RegisterJsObject("callbackObj", new CallbackObjectForJs());
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

            //cwb.Address = address;

            //cwb.RegisterJsObject("SiteInfoData", new SiteService());
            //cwb.
        }

        public class CallbackObjectForJs
        {
            public void showMessage(string msg)
            {
                MessageBox.Show(msg);
            }
        }
    }
}

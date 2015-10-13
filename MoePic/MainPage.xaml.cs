using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using MoePic.Toolkit.Net;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace MoePic
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = await (await HttpRequest
                .Create("http://higan-wordpress.stor.sinaapp.com/uploads/2015/10/image001.png")
                .WhenTransferProgressChanged(async (o, args) =>
                {
                    await ProgressBar.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ProgressBar.Value = args.Value.Progress*100;
                    });
                })
                .Get()
                .Wait())
                .GetDateAsImage();
        }
    }
}

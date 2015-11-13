using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MoePic.Toolkit.Helper;

namespace MoePic.Toolkit.Data
{
    /// <summary>
    /// 增量加载提供者的基类，这是一个抽象类，需要进行派生，并提供方法才能使用。
    /// </summary>
    public abstract class IncrementalLoadingProvider : NotifyPropertyObject
    {
        private ItemsControl _hostControl;

        private EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        internal async void AttacheHostControl(ItemsControl hostControl)
        {
            if (hostControl != null)
            {
                var scrollViewer = VisualTreeHelper.FindVisualElement<ScrollViewer>(hostControl);
                if (scrollViewer == null)
                {
                    hostControl.Loaded += HostControlOnLoaded;
                    await Task.Run(() =>
                    {
                        _waitHandle.WaitOne(1000);
                    });
                    hostControl.Loaded -= HostControlOnLoaded;

                    await hostControl.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        scrollViewer = VisualTreeHelper.FindVisualElement<ScrollViewer>(hostControl);
                    });
                }

                await hostControl.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    AttacheHostControlPrivate(hostControl, scrollViewer);
                });
            }
        }

        private async void AttacheHostControlPrivate(ItemsControl hostControl, ScrollViewer scrollViewer)
        {
            if (scrollViewer != null)
            {
                IsLoading = true;
                _hostControl = hostControl;
                scrollViewer.ViewChanged += OnScrollViewChanged;

                var data = await FirstRequestData();
                await _hostControl.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    AddDataToHostControl(data);
                });
                IsLoading = false;
            }
            else
            {
                throw new InvalidOperationException("无法在 ItemControl 中找到 ScrollViewer。");
            }
        }

        private void HostControlOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _waitHandle.Set();
        }

        internal void UnattacheHostControl(ItemsControl hostControl)
        {
            if (hostControl != null)
            {
                var scrollViewer = VisualTreeHelper.FindVisualElement<ScrollViewer>(hostControl);
                if (scrollViewer != null)
                {
                    _hostControl = null;
                    scrollViewer.ViewChanged -= OnScrollViewChanged;
                }
            }
        }

        private void AddDataToHostControl(IList data)
        {
            var listSource = (_hostControl.ItemsSource as IList);
            if (listSource != null && data != null)
            {
                foreach (var d in data)
                {
                    listSource.Add(d);
                }
            }
        }
        
        private DateTime _lastRefreshTime;
        
        private bool _isLoading;

        private async void OnScrollViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!IsLoading)
            {
                if (RefreshInterval == TimeSpan.Zero || (DateTime.Now - _lastRefreshTime) > RefreshInterval)
                {
                    IsLoading = true;
                    _lastRefreshTime = DateTime.Now;
                    var scrollViewer = (ScrollViewer)sender;

                    if (IsNeedRequestData(scrollViewer))
                    {
                        var data = await RequestData();
                        AddDataToHostControl(data);
                    }
                    IsLoading = false;
                }
            }
        }

        /// <summary>
        /// 提供请求数据的方法，这是一个异步的过程。
        /// </summary>
        /// <returns></returns>
        protected virtual Task<IList> RequestData()
        {
            throw new NotImplementedException("未提供请求数据的方法。");
        }

        /// <summary>
        /// 提供一个方法，该方法返回值决定了是否需要请求数据。
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsNeedRequestData(ScrollViewer scrollViewer)
        {
            throw new NotImplementedException("未提供是否需要请求数据的方法。");
        }

        /// <summary>
        /// 提供第一次请求数据的方法，该方法会在增量加载提供者首次附加到 ItemControl 时被调用。
        /// </summary>
        /// <returns></returns>
        protected virtual Task<IList> FirstRequestData()
        {
            throw new NotImplementedException("未提供首次请求数据的方法。");
        }

        /// <summary>
        /// 获取一个值，该值表示是否正在载入中，提供属性变更通知。
        /// </summary>
        public bool IsLoading
        {
            get { return _isLoading; }
            private set
            {
                if (value == _isLoading) return;
                OnPropertyChanging();
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 表示判断是否需要请求数据的刷新间隔，该值表示在滚动的过程中，每多长事件进行一次判断是否需要载入数据，避免频繁请求数据。
        /// </summary>
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMilliseconds(500);
    }
}

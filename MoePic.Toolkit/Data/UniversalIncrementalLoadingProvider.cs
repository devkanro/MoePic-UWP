using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace MoePic.Toolkit.Data
{
    /// <summary>
    /// 表示由通用的增量加载提供者。
    /// </summary>
    public class UniversalIncrementalLoadingProvider : IncrementalLoadingProvider
    {
        private double _residualItemsHeight;
        private bool _canStopRequestingData;
        private bool _isStopRequestingData;

        protected override bool IsNeedRequestData(ScrollViewer scrollViewer)
        {
            if (CanStopRequestingData && IsStopRequestingData)
            {
                return false;
            }
            return scrollViewer.ScrollableHeight - scrollViewer.VerticalOffset < scrollViewer.ViewportHeight * ResidualItemsHeight;
        }

        protected async override Task<IList> RequestData()
        {
            RequestDataEventArgs eventArgs = new RequestDataEventArgs();
            if (RequestingData != null)
            {
                RequestingData(this, eventArgs);
                await eventArgs.WaitRequest();
                if (CanStopRequestingData && (eventArgs.Result == null || eventArgs.Result.Count == 0))
                {
                    IsStopRequestingData = true;
                }
                return eventArgs.Result;
            }
            else
            {
                return null;
            }
        }

        protected async override Task<IList> FirstRequestData()
        {
            RequestDataEventArgs eventArgs = new RequestDataEventArgs();
            if (RequestingData != null)
            {
                FirstRequestingData(this, eventArgs);
                await eventArgs.WaitRequest();
                return eventArgs.Result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该指表示列表剩余项目高度为多少时进行数据请求，该值根据列表的可视区域决定，当值为 0.5 时，表示当列表剩余项目高度小于列表可视区域的 1/2 时进行增量加载。
        /// </summary>
        public double ResidualItemsHeight
        {
            get { return _residualItemsHeight; }
            set
            {
                if (value.Equals(_residualItemsHeight)) return;
                OnPropertyChanging();
                _residualItemsHeight = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值表示当请求数据时返回空列表或者空值时，停止继续对数据的增量请求。
        /// </summary>
        public bool CanStopRequestingData
        {
            get { return _canStopRequestingData; }
            set
            {
                if (value == _canStopRequestingData) return;
                OnPropertyChanging();
                _canStopRequestingData = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 获取一个值，该值表示是否已经停止了继续对数据的增量请求。
        /// </summary>
        public bool IsStopRequestingData
        {
            get { return _isStopRequestingData; }
            private set
            {
                if (value == _isStopRequestingData) return;
                OnPropertyChanging();
                _isStopRequestingData = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler<RequestDataEventArgs> RequestingData;

        public event EventHandler<RequestDataEventArgs> FirstRequestingData;
    }
    
    /// <summary>
    /// 为增量加载事件提供参数。
    /// </summary>
    public class RequestDataEventArgs : EventArgs ,IDisposable
    {
        /// <summary>
        /// 增量加载结果。
        /// </summary>
        public IList Result { get; set; }

        /// <summary>
        /// 获取一个值，该值表示增量加载是否已经结束。
        /// </summary>
        public bool IsLoadOver { get; private set; }

        /// <summary>
        /// 增量加载可能是一个耗时过程，当增量加载完成时，调用此方法，将会把结果加入列表中。
        /// </summary>
        public void EndRequest()
        {
            if (!IsLoadOver)
            {
                IsLoadOver = true;
                _requestWaitHandle.Set();
            }
            else
            {
                throw new InvalidOperationException("已经结束增量加载请求。");
            }
        }

        private EventWaitHandle _requestWaitHandle = new ManualResetEvent(false);

        internal Task WaitRequest()
        {
            return Task.Run(() =>
            {
                _requestWaitHandle.WaitOne();
            });
        }

        public void Dispose()
        {
            ((IDisposable) _requestWaitHandle).Dispose();
        }
    }
}

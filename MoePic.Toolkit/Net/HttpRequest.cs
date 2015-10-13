using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using MoePic.Toolkit.Annotations;
using Buffer = Windows.Storage.Streams.Buffer;

namespace MoePic.Toolkit.Net
{
    /// <summary>
    /// 表示正在进行的 HTTP 请求。
    /// </summary>
    public class HttpRequest : NotifyPropertyObject, IDisposable
    {
        /// <summary>
        /// 表示传输时缓冲区大小，该大小表示，每获取到指定字节大小的数据时就将其写入结果，并提供进度报告，初始值为 32KB。
        /// </summary>
        public static int TransferBufferSize { get; set; } = 32 * 1024;

        /// <summary>
        /// 默认次数的请求重试，初始值为 3。
        /// </summary>
        public static int DefaultRetryCount { get; set; } = 3;

        public static HttpRequest Create(String url)
        {
            return new HttpRequest(WebRequest.CreateHttp(url));
        }

        public static HttpRequest Create(Uri uri)
        {
            return new HttpRequest(WebRequest.CreateHttp(uri));
        }

        private EventWaitHandle _progressWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private bool _progressing = false;

        private HttpRequestState _state;
        private HttpWebRequest _request;
        private HttpWebResponse _response;
        private Exception _exception;
        private MemoryStream _result;

        private HttpRequest(HttpWebRequest request)
        {
            Request = request;
        }

        /// <summary>
        /// HTTP 请求的状态。
        /// </summary>
        public HttpRequestState State
        {
            get { return _state; }
            internal set
            {
                if (value == _state) return;
                _state = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// HTTP 请求的内容。
        /// </summary>
        public HttpWebRequest Request
        {
            get { return _request; }
            internal set
            {
                if (Equals(value, _request)) return;
                _request = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// HTTP 响应的内容。
        /// </summary>
        public HttpWebResponse Response
        {
            get { return _response; }
            internal set
            {
                if (Equals(value, _response)) return;
                _response = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 当出现异常时，所捕获的异常。
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
            private set
            {
                if (Equals(value, _exception)) return;
                _exception = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// HTTP 响应结果流。
        /// </summary>
        public MemoryStream ResultStream
        {
            get { return _result; }
            private set
            {
                if (Equals(value, _result)) return;
                _result = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 设置或获取一个委托，当 HTTP 传输进度改变时触发该委托。不仅仅是下载进度改变会触发这个事件，同样的 HTTP 请求状态改变也会触发改事件。
        /// </summary>
        public EventHandler<EventArgs<HttpRequestStats>> TransferProgressChanged { get; set; }

        /// <summary>
        /// 向 HTTP 请求加入 Header。
        /// </summary>
        /// <param name="name">Header 名。</param>
        /// <param name="value">Header 值。</param>
        public HttpRequest AddHeader([NotNull]String name, [NotNull]String value)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改，如果请求已经结束，请使用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.Headers[name] = value;
            return this;
        }

        /// <summary>
        /// 向 HTTP 请求加入 Header。
        /// </summary>
        /// <param name="header">Header 类型。</param>
        /// <param name="value">Header 值。</param>
        public HttpRequest AddHeader([NotNull]HttpRequestHeader header, [NotNull]String value)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改，如果请求已经结束，请使用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.Headers[header] = value;
            return this;
        }

        /// <summary>
        /// 向 HTTP 请求加入 Header。
        /// </summary>
        /// <param name="header">Header 类型。</param>
        /// <param name="value">Header 值。</param>
        public HttpRequest AddHeader([NotNull]HttpResponseHeader header, [NotNull]String value)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改，如果请求已经结束，请使用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.Headers[header] = value;
            return this;
        }

        /// <summary>
        /// 向 HTTP 请求加入 Cookie。
        /// </summary>
        /// <param name="uri">要加入 Cookie 的 Uri。</param>
        /// <param name="cookie">要加入的 Cookie。</param>
        /// <returns></returns>
        public HttpRequest AddCookie([NotNull]Uri uri, [NotNull]Cookie cookie)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改，如果请求已经结束，请使用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.CookieContainer.Add(uri, cookie);
            return this;
        }

        /// <summary>
        /// 向 HTTP 请求加入 Cookies。
        /// </summary>
        /// <param name="uri">要加入 Cookie 的 Uri。</param>
        /// <param name="cookies">要加入的 Cookies。</param>
        /// <returns></returns>
        public HttpRequest AddCookies([NotNull]Uri uri, [NotNull]CookieCollection cookies)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改，如果请求已经结束，请使用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.CookieContainer.Add(uri, cookies);
            return this;
        }

        /// <summary>
        /// 提供相应的处理器来处理该 HTTP 请求。
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public HttpRequest HandleRequest([NotNull]HttpWebRequestHandler handler)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改，如果请求已经结束，请使用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            handler(Request);
            return this;
        }

        /// <summary>
        /// 重新构造相同的 HTTP 请求，用于重复当前请求。
        /// </summary>
        public HttpRequest RebuildRequest()
        {
            if (State != HttpRequestState.Connecting || State != HttpRequestState.Progressing)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改。");
            }
            var oldRequest = Request;
            Request = WebRequest.CreateHttp(oldRequest.RequestUri);

            Request?.Abort();
            ResultStream?.Dispose();

            Request.Accept = oldRequest.Accept;
            Request.AllowReadStreamBuffering = oldRequest.AllowReadStreamBuffering;
            Request.ContentType = oldRequest.ContentType;
            Request.ContinueTimeout = oldRequest.ContinueTimeout;
            Request.CookieContainer = oldRequest.CookieContainer;
            Request.Credentials = oldRequest.Credentials;
            Request.Headers = oldRequest.Headers;
            Request.Method = oldRequest.Method;
            Request.Proxy = oldRequest.Proxy;
            Request.UseDefaultCredentials = oldRequest.UseDefaultCredentials;

            State = HttpRequestState.NothingSpecial;

            return this;
        }

        /// <summary>
        /// 使用默认的重试次数，Post 该请求。
        /// </summary>
        public HttpRequest Post([CanBeNull]Stream content = null)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对已进行过的请求再次发起请求，请考虑用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Post(DefaultRetryCount, content);
            return this;
        }

        /// <summary>
        /// 使用指定的重试次数，Post 该请求。
        /// </summary>
        public HttpRequest Post(int retryCount, [CanBeNull]Stream content = null)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对已进行过的请求再次发起请求，请考虑用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.Method = "POST";
            Start(retryCount, content);
            return this;
        }

        /// <summary>
        /// 使用默认的重试次数，Get 该请求。
        /// </summary>
        public HttpRequest Get()
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对已进行过的请求再次发起请求，请考虑用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Get(DefaultRetryCount);
            return this;
        }

        /// <summary>
        /// 使用指定的重试次数，Post 该请求。
        /// </summary>
        public HttpRequest Get(int retryCount)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对已进行过的请求再次发起请求，请考虑用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.Method = "GET";
            Start(retryCount);
            return this;
        }

        /// <summary>
        /// 使用默认的重试次数，Get 该请求。
        /// </summary>
        public HttpRequest Put([CanBeNull]Stream content = null)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对已进行过的请求再次发起请求，请考虑用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Put(DefaultRetryCount);
            return this;
        }

        /// <summary>
        /// 使用指定的重试次数，Post 该请求。
        /// </summary>
        public HttpRequest Put(int retryCount, [CanBeNull]Stream content = null)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对已进行过的请求再次发起请求，请考虑用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.Method = "GET";
            Start(retryCount);
            return this;
        }

        /// <summary>
        /// 设置传输进度改变时的委托。
        /// </summary>
        public HttpRequest WhenTransferProgressChanged([NotNull]EventHandler<EventArgs<HttpRequestStats>> handler)
        {
            TransferProgressChanged = handler;
            return this;
        }

        private void Start(int retryCount, [CanBeNull]Stream content = null)
        {
            _progressing = true;
            Task<bool> task = Task.Run(async () =>
            {
                _progressWaitHandle.Reset();
                int retry = 0;
                RETRY:

                if (State == HttpRequestState.NothingSpecial)
                {
                    EventArgs<HttpRequestStats> progressInfo = null;
                    DateTime startTime = DateTime.Now;

                    if (TransferProgressChanged != null)
                    {
                        progressInfo = new EventArgs<HttpRequestStats>(new HttpRequestStats());
                    }

                    try
                    {
                        State = HttpRequestState.Connecting;

                        if (TransferProgressChanged != null)
                        {
                            progressInfo.Value.Update();
                            TransferProgressChanged(this, progressInfo);
                        }

                        if (content != null && (Request.Method == "PUT" || Request.Method == "POST"))
                        {
                            using (var requestStream = await Request.GetRequestStreamAsync())
                            {
                                State = HttpRequestState.Progressing;

                                if (TransferProgressChanged != null)
                                {
                                    progressInfo.Value.ProgressType = HttpRequestProgressType.Upload;
                                    progressInfo.Value.Update();
                                    progressInfo.Value.TotalBytes = content.Length;
                                    TransferProgressChanged(this, progressInfo);
                                }

                                byte[] buffer = new byte[TransferBufferSize];
                                int writeLength = 0;

                                while ((writeLength = content.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    requestStream.Write(buffer, 0, writeLength);
                                    if (TransferProgressChanged != null)
                                    {
                                        progressInfo.Value.Update(writeLength);
                                        TransferProgressChanged(this, progressInfo);
                                    }
                                }

                                if (TransferProgressChanged != null)
                                {
                                    progressInfo = new EventArgs<HttpRequestStats>(new HttpRequestStats());
                                }
                            }
                        }

                        if (Request.Method == "GET" || Request.Method == "POST")
                        {
                            using (Response = (HttpWebResponse)await Request.GetResponseAsync())
                            {
                                using (var stream = Response.GetResponseStream())
                                {
                                    if (TransferProgressChanged != null)
                                    {
                                        progressInfo.Value.ProgressType = HttpRequestProgressType.Download;
                                        State = HttpRequestState.Progressing;
                                        progressInfo.Value.TotalBytes = Response.ContentLength > 0 ? Response.ContentLength : 10 * TransferBufferSize;
                                    }

                                    byte[] buffer = new byte[TransferBufferSize];
                                    ResultStream =
                                        new MemoryStream((int)(Response.ContentLength > 0
                                            ? Response.ContentLength
                                            : 10 * TransferBufferSize));

                                    int readLength = 0;
                                    while ((readLength = stream.Read(buffer, 0, buffer.Length)) != 0)
                                    {
                                        ResultStream.Write(buffer, 0, readLength);
                                        if (TransferProgressChanged != null)
                                        {
                                            progressInfo.Value.Update(readLength);
                                            TransferProgressChanged(this, progressInfo);
                                        }
                                    }

                                    ResultStream.Seek(0, SeekOrigin.Begin);
                                    //ResultStream.SetLength(progressInfo.Value.TotalBytes);
                                }
                            }
                        }

                        State = HttpRequestState.Completed;
                        if (TransferProgressChanged != null)
                        {
                            progressInfo.Value.Update();
                            TransferProgressChanged(this, progressInfo);
                        }
                        _progressWaitHandle.Set();
                        _progressing = false;
                        return true;
                    }
                    catch (WebException webException)
                    {
                        Exception = webException;
                        State = HttpRequestState.ErrorOccurred;
                        if (TransferProgressChanged != null)
                        {
                            progressInfo.Value.Update();
                            TransferProgressChanged(this, progressInfo);
                        }
                    }
                }

                if (retry < retryCount)
                {
                    retry++;
                    RebuildRequest();

                    goto RETRY;
                }

                _progressWaitHandle.Set();
                _progressing = false;
                return false;
            });
        }

        /// <summary>
        /// 异步等待一个传输过程。
        /// </summary>
        public async Task<HttpRequest> Wait()
        {
            if (_progressing)
            {
                await Task.Run(() =>
                {
                    _progressWaitHandle.WaitOne();
                });
            }
            return this;
        }

        /// <summary>
        /// 取消 HTTP 请求。
        /// </summary>
        public HttpRequest Canel()
        {
            Request.Abort();
            State = HttpRequestState.Cancelled;
            TransferProgressChanged?.Invoke(this, null);
            return this;
        }

        /// <summary>
        /// 将 HTTP 请求结果以 UTF-8 编码转换为字符串。
        /// </summary>
        public String GetDataAsString()
        {
            return GetDataAsString(Encoding.UTF8);
        }

        /// <summary>
        /// 将 HTTP 请求结果以指定的代码页转换为字符串。
        /// </summary>
        public String GetDataAsString([NotNull]Encoding encoding)
        {
            if (State == HttpRequestState.Completed)
            {
                if (ResultStream != null && ResultStream.Length > 0)
                {
                    byte[] data = new byte[ResultStream.Length];
                    ResultStream?.Read(data, 0, data.Length);
                    this.Dispose();
                    return encoding.GetString(data);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new InvalidOperationException("请在请求完成后调用 HttpRequest.GetDataAsString 方法。");
            }
        }

        public async Task<ImageSource> GetDateAsImage()
        {
            if (State == HttpRequestState.Completed)
            {
                if (ResultStream != null && ResultStream.Length > 0)
                {

                    var image = new BitmapImage();
                    await image.SetSourceAsync(ResultStream.AsRandomAccessStream());
                    this.Dispose();
                    return image;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new InvalidOperationException("请在请求完成后调用 HttpRequest.GetDataAsString 方法。");
            }
        }

        public void Dispose()
        {
            Request?.Abort();
            ((IDisposable)ResultStream)?.Dispose();
        }
    }
}
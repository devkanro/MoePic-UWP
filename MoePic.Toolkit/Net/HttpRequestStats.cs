using System;

namespace MoePic.Toolkit.Net
{
    /// <summary>
    /// HTTP 请求传输统计。
    /// </summary>
    public class HttpRequestStats
    {
        private DateTime _createTime = DateTime.Now;

        /// <summary>
        /// 已经传输的字节数。
        /// </summary>
        public long TransferBytes { get; internal set; }
        /// <summary>
        /// 需要传输的字节数。
        /// </summary>
        public long TotalBytes { get; internal set; }
        /// <summary>
        /// 传输进度，取值为 0 - 1，为 -1 时表示该传输不支持查看进度。
        /// </summary>
        public double Progress => TotalBytes == 0 ? -1 : 1.0*TransferBytes/TotalBytes;
        /// <summary>
        /// 传输速度，单位为 byte/s。
        /// </summary>
        public int Speed { get; private set; }
        /// <summary>
        /// 消耗时间，单位为 ms。    
        /// </summary>
        public int Time { get; private set; }
        /// <summary>
        /// 总消耗时间，单位为 ms。
        /// </summary>
        public int TotalTime { get; private set; }
        /// <summary>
        /// 表示当前传属状态。
        /// </summary>
        public HttpRequestProgressType ProgressType { get; internal set; }

        internal void Update()
        {
            Time = (int)(DateTime.Now -_createTime).TotalMilliseconds - TotalTime;
            TotalTime = (int)(DateTime.Now - _createTime).TotalMilliseconds;
        }
        internal void Update(int readBytes)
        {
            Update();
            TransferBytes += readBytes;
            if (Time != 0)
            {
                Speed = (int)(1000.0 * (readBytes / Time));
            }
            else
            {
                Speed = -1;
            }
        }
    }
}

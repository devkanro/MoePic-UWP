using System;

namespace MoePic.Toolkit
{
    /// <summary>
    /// 提供建议的通用的事件参数。
    /// </summary>
    /// <typeparam name="T">提供的事件信息的类型。</typeparam>
    public class EventArgs<T> : EventArgs
    {
        /// <summary>
        /// 以默认值构造<see cref="EventArgs{T}"/>
        /// </summary>
        public EventArgs()
        {
            
        }

        /// <summary>
        /// 以初始值构造<see cref="EventArgs{T}"/>
        /// </summary>
        /// <param name="value">初始值</param>
        public EventArgs(T value)
        {
            Value = value;
        }

        /// <summary>
        /// 事件参数信息
        /// </summary>
        public T Value { get; set; }
    }
}

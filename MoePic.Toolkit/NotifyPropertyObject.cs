using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MoePic.Toolkit.Annotations;

namespace MoePic.Toolkit
{
    /// <summary>
    /// 表示一个支持属性变更通知的对象。
    /// </summary>
    public class NotifyPropertyObject : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// 当属性变更将要变更时发生。
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;
        /// <summary>
        /// 当属性变更后发生。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 引发<see cref="PropertyChanged"/>事件。
        /// </summary>
        /// <param name="propertyName">发生变更的属性名，为<c>null</c>将自动采用调用方名。</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 引发<see cref="PropertyChanged"/>事件。
        /// </summary>
        /// <param name="propertyName">将要发生变更的属性名，为<c>null</c>将自动采用调用方名。</param>
        protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        /// <summary>
        /// 设置属性值，并提供相应的属性通知。
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="value">属性值</param>
        /// <param name="propertyName">将要发生设置的属性名，为<c>null</c>将自动采用调用方名。</param>
        protected virtual void SetProperty<T>(T value, [CallerMemberName] string propertyName = null)
        {
            PropertyInfo propertyInfo = this.GetType().GetProperty(propertyName);
            T oldValue = (T) propertyInfo.GetValue(this);
            if (Object.Equals(oldValue,value))
            {
                OnPropertyChanging(propertyName);
                propertyInfo.SetValue(this,value);
                OnPropertyChanged(propertyName);
            }
        }
    }
}

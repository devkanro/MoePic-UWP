using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;
using MoePic.Toolkit.Annotations;

namespace MoePic.Toolkit.Media.Animation
{
    /// <summary>
    /// 表示动画的缓动函数。
    /// </summary>
    public abstract class EasingFunction
    {
        protected EasingFunction()
        {
            
        }

        /// <summary>
        /// 创建用于 Composition API 的 CompositionEasingFunction。
        /// </summary>
        /// <param name="compositor"></param>
        /// <returns></returns>
        public virtual CompositionEasingFunction CreateCompositionEasingFunction([NotNull]Compositor compositor)
        {
            throw new NotImplementedException("未提供创建 CompositionEasingFunction 的方法。");
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;

namespace MoePic.Toolkit.Media.Animation
{
    /// <summary>
    /// 表示线性无过度的缓动函数。
    /// </summary>
    public class LinearEasingFunction : EasingFunction
    {
        public LinearEasingFunction()
        {
            
        }
        /// <summary>
        /// 创建用于 Composition API 的 CompositionEasingFunction。
        /// </summary>
        /// <param name="compositor"></param>
        /// <returns></returns>
        public override CompositionEasingFunction CreateCompositionEasingFunction(Compositor compositor)
        {
            return compositor.CreateLinearEasingFunction();
        }
    }
}

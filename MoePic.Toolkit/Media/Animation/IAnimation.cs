using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.NetworkOperators;
using Windows.UI.Composition;
using Windows.UI.Xaml;

namespace MoePic.Toolkit.Media.Animation
{
    internal interface IAnimation : IDisposable
    {   
        UIElement Target { get; set; }

        String Property { get; set; }

        ParameterCollection Parameters { get; }

        CompositionPropertyAnimator BuildCompositionAnimation();
    }
}
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MoePic.Toolkit.Data
{
    /// <summary>
    /// 为<see cref="ItemsControl"/>提供增量加载的服务。
    /// </summary>
    public class IncrementalLoadingService
    {
        public static readonly DependencyProperty ProviderProperty = DependencyProperty.RegisterAttached(
            "Provider", typeof(IncrementalLoadingProvider), typeof(ItemsControl), new PropertyMetadata(default(IncrementalLoadingProvider)));

        public static void SetProvider(DependencyObject element, IncrementalLoadingProvider value)
        {
            if (element is ItemsControl)
            {
                var itemControl = (ItemsControl)element;
                if (GetProvider(itemControl) != null)
                {
                    var oldProvider = GetProvider(itemControl);
                    oldProvider.UnattacheHostControl(itemControl);
                }
                element.SetValue(ProviderProperty, value);
                value.AttacheHostControl(itemControl);
            }
            else
            {
                throw new InvalidOperationException("无法对非 ItemControl 控件提供增量加载服务。");
            }
        }

        public static IncrementalLoadingProvider GetProvider(DependencyObject element)
        {
            return (IncrementalLoadingProvider)element.GetValue(ProviderProperty);
        }
    }
}

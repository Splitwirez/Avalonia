using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using Splat;

namespace Avalonia.ReactiveUI
{
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Initializes ReactiveUI framework to use with Avalonia. Registers Avalonia 
        /// scheduler and Avalonia activation for view fetcher. Always remember to
        /// call this method if you are using ReactiveUI in your application.
        /// </summary>
        public static TAppBuilder UseReactiveUI<TAppBuilder>(this TAppBuilder builder)
            where TAppBuilder : AppBuilderBase<TAppBuilder>, new()
        {
            return builder.AfterPlatformServicesSetup(_ =>
            {
                RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
                Locator.CurrentMutable.RegisterConstant(new AvaloniaActivationForViewFetcher(), typeof(IActivationForViewFetcher));
                Locator.CurrentMutable.RegisterConstant(new AutoDataTemplateBindingHook(), typeof(IPropertyBindingHook));
            });
        }
    }
}

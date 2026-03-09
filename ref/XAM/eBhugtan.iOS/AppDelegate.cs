using Foundation;
using UIKit;

namespace eBhugtan.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());
            UINavigationBar.Appearance.SetBackgroundImage(UIImage.FromBundle("Heade.png").CreateResizableImage(UIEdgeInsets.Zero, UIImageResizingMode.Stretch), UIBarMetrics.Default);
            return base.FinishedLaunching(app, options);
        }
    }
}

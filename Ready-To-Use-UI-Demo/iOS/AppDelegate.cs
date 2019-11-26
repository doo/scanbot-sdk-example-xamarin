using Foundation;
using ReadyToUseUIDemo.iOS.Controller;
using ScanbotSDK.Xamarin.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        const string LICENSE = null;

        public UINavigationController Controller { get; set; }

        public override UIWindow Window { get; set; }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            var configuration = new SBSDKConfiguration { EnableLogging = true };
            SBSDK.Initialize(application, LICENSE, configuration);

            UIViewController initial = new MainViewController();
            Controller = new UINavigationController(initial);

            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            Window.RootViewController = Controller;

            Window.MakeKeyAndVisible();

            return true;
        }
    }
}


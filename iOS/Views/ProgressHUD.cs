using Foundation;
using System;
using UIKit;
using ObjCRuntime;

namespace scanbotsdkexamplexamarin.iOS
{
    public partial class ProgressHUD : UIView
    {
        public ProgressHUD(IntPtr handle) : base(handle)
        {
        }

        public static ProgressHUD Load()
        {
            var array = NSBundle.MainBundle.LoadNib("ProgressHUD", null, null);
            return Runtime.GetNSObject<ProgressHUD>(array.ValueAt(0));
        }

        public void Show()
        {
            UIWindow mainWindow = UIApplication.SharedApplication.KeyWindow;
            mainWindow.AddSubview(this);

            Frame = mainWindow.Frame;
            LayoutIfNeeded();
        }

        public void Hide()
        {
            UIWindow mainWindow = UIApplication.SharedApplication.KeyWindow;
            foreach (UIView subView in mainWindow.Subviews)
            {
                if (subView.IsKindOfClass(Class))
                {
                    subView.RemoveFromSuperview();
                }
            }
        }
    }
}
using System;
using CoreGraphics;
using UIKit;

namespace ClassicalComponentsDemo.iOS
{
    public sealed class CommonUtils
    {
        public static UIImage MaxResizeImage(UIImage sourceImage, float maxWidth, float maxHeight)
        {
            var sourceSize = sourceImage.Size;
            var maxResizeFactor = Math.Max(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);
            if (maxResizeFactor > 1) return sourceImage;
            var width = maxResizeFactor * sourceSize.Width;
            var height = maxResizeFactor * sourceSize.Height;
            UIGraphics.BeginImageContext(new CGSize(width, height));
            sourceImage.Draw(new CGRect(0, 0, width, height));
            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return resultImage;
        }

        internal static void ShowAlert(string title, string message, UIViewController viewController = null, Action<UIAlertAction> OkClicked = null)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
                var okAction = UIAlertAction.Create("Ok", UIAlertActionStyle.Default, OkClicked);
                alert.AddAction(okAction);

                viewController = viewController ?? (UIApplication.SharedApplication.Delegate as AppDelegate).Window.RootViewController;
                viewController.PresentViewController(alert, true, null);
            });
        }
    }
}
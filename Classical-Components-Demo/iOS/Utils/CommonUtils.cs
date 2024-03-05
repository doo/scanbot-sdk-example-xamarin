using System;
using System.Threading.Tasks;
using AVFoundation;
using CoreAudioKit;
using CoreGraphics;
using Foundation;
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

        internal static async System.Threading.Tasks.Task<bool> CheckCameraPermissions()
        {
            var permissionGranted = false;
            var status = AVCaptureDevice.GetAuthorizationStatus(AVAuthorizationMediaType.Video);
            switch (status)
            {
                case AVAuthorizationStatus.Denied:
                    System.Diagnostics.Debug.WriteLine("Denied, request permission from settings. Asking for permissions explicitly");
                    UIApplication.SharedApplication.InvokeOnMainThread(PromtCameraPermissions);
                    break;
                case AVAuthorizationStatus.Restricted:
                    System.Diagnostics.Debug.WriteLine("Restricted, device owner must approve");
                    break;
                case AVAuthorizationStatus.Authorized:
                    System.Diagnostics.Debug.WriteLine("Authorized, proceed");
                    permissionGranted = true;
                    break;
                case AVAuthorizationStatus.NotDetermined:
                    // success or failure
                    permissionGranted = await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVAuthorizationMediaType.Video);
                    break;
            }
            return permissionGranted;
        }

        private static void PromtCameraPermissions()
        {
            var alert = UIAlertController.Create("Permission needed", "The application will need the Camera permissions for this action.", UIAlertControllerStyle.Alert);
            var okAction = UIAlertAction.Create("Go to settings", UIAlertActionStyle.Default, (action) =>
            {
                var url = new NSUrl($"app-settings:");
                UIApplication.SharedApplication.OpenUrl(url);
                UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString));
            });
            var cancelAction = UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null);
            alert.AddAction(okAction);
            alert.AddAction(cancelAction);

            var viewController = (UIApplication.SharedApplication.Delegate as AppDelegate).Window.RootViewController;
            viewController.PresentViewController(alert, true, null);
        }
    }
}
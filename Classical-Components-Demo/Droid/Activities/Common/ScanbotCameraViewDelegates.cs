
using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Android.Views;
using Android.Util;

// Wrapper namespace
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Android;
using IO.Scanbot.Sdk.UI.Camera;
using IO.Scanbot.Sdk.Camera;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using IO.Scanbot.Sdk.Contourdetector;
using IO.Scanbot.Sdk.UI;
using IO.Scanbot.Sdk.Core.Contourdetector;

public class ContourDetectorEventArgs : EventArgs
{
    public ContourDetectorFrameHandler.DetectedFrame Frame { get; set; }
}

public class ContourDetectorDelegate : ContourDetectorFrameHandler.ContourDetectorResultHandler
{
    public EventHandler<ContourDetectorEventArgs> ContourDetected;

    public override bool Handle(FrameHandlerResult result)
    {
        if (result.GetType() == typeof(FrameHandlerResult.Success))
        {
            var success = ((FrameHandlerResult.Success)result);
            if (success.Value.GetType() == typeof(ContourDetectorFrameHandler.DetectedFrame))
            {
                var frame = (ContourDetectorFrameHandler.DetectedFrame)success.Value;
                ContourDetected?.Invoke(this, new ContourDetectorEventArgs { Frame = frame });
            }
        }
        return false;
    }
}

class PictureCallbackEventArgs : EventArgs
{
    public byte[] image { get; set; }
    public int imageOrientation { get; set; }
}

class PictureCallbackDelegate : PictureCallback
{
    public EventHandler<PictureCallbackEventArgs> OnPictureTakenHandler;

    public override void OnPictureTaken(byte[] image, CaptureInfo captureInfo)
    {
        OnPictureTakenHandler?.Invoke(this, new PictureCallbackEventArgs { image = image, imageOrientation = captureInfo.ImageOrientation });
    }
}
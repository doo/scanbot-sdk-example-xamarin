using System;
using Android.Views;
using IO.Scanbot.Sdk;
using IO.Scanbot.Sdk.Camera;
using IO.Scanbot.Sdk.Check;
using IO.Scanbot.Sdk.Check.Entity;
using IO.Scanbot.Sdk.Contourdetector;
using IO.Scanbot.Sdk.Core.Contourdetector;
using IO.Scanbot.Sdk.UI.Camera;

namespace ClassicalComponentsDemo.Droid.Delegates
{
    class ContourDetectorEventArgs : EventArgs
    {
        public ContourDetectorFrameHandler.DetectedFrame Frame { get; set; }
    }

    class ContourDetectorResultDelegate : ContourDetectorResultHandlerWrapper
    {
        public EventHandler<ContourDetectorEventArgs> ContourDetected;
        public override bool HandleResult(ContourDetectorFrameHandler.DetectedFrame result, SdkLicenseError error)
        {
            if (result != null)
            {
                ContourDetected?.Invoke(this, new ContourDetectorEventArgs { Frame = result });
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

    class CameraOpenCallback : Java.Lang.Object, ICameraOpenCallback
    {
        ScanbotCameraXView cameraView;
        bool isFlashEnabled;

        public CameraOpenCallback(ScanbotCameraXView cameraView, bool isFlashEnabled)
        {
            this.cameraView = cameraView;
            this.isFlashEnabled = isFlashEnabled;
        }

        public void OnCameraOpened()
        {
            this.cameraView.PostDelayed(() =>
            {
                this.cameraView.UseFlash(this.isFlashEnabled);
                this.cameraView.ContinuousFocus();
            }, 700);
        }
    }

    class OnClickListener : Java.Lang.Object, View.IOnClickListener
    {
        Action action;
        public OnClickListener(Action action) { this.action = action; }
        public void OnClick(View v) { action.Invoke(); }
    }

    class CheckRecognizerResultDelegate : CheckRecognizerResultHandlerWrapper
    {
        public EventHandler<CheckRecognizerResult> OnCheckRecognized;
        public override bool HandleResult(CheckRecognizerResult result, SdkLicenseError error)
        {
            if (result != null)
            {
                OnCheckRecognized?.Invoke(this, result);
            }
            return false;
        }
    }
}


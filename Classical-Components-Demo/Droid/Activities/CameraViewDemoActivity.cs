﻿using Android.App;
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
using System;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using IO.Scanbot.Sdk.Contourdetector;
using IO.Scanbot.Sdk.UI;
using IO.Scanbot.Sdk.Core.Contourdetector;

namespace ClassicalComponentsDemo.Droid
{
    [Activity(Theme = "@style/Theme.AppCompat")]
    public class CameraViewDemoActivity : AppCompatActivity, ICameraOpenCallback
    {
        static string LOG_TAG = typeof(CameraViewDemoActivity).Name;

        public static string EXTRAS_ARG_DOC_IMAGE_FILE_URI = "documentImageFileUri";
        public static string EXTRAS_ARG_ORIGINAL_IMAGE_FILE_URI = "originalImageFileUri";

        protected ScanbotCameraView cameraView;
        protected DocumentAutoSnappingController autoSnappingController;
        protected ContourDetectorFrameHandler contourDetectorFrameHandler;
        protected PolygonView polygonView;
        protected bool flashEnabled = false;
        protected bool autoSnappingEnabled = true;
        protected readonly bool ignoreBadAspectRatio = true;
        protected TextView userGuidanceTextView;
        protected long lastUserGuidanceHintTs = 0L;
        protected ProgressBar imageProcessingProgress;
        protected ShutterButton shutterButton;
        protected Button autoSnappingToggleButton;

        ContourDetectorDelegate contourDetectorDelegate;
        PictureCallbackDelegate pictureCallbackDelegate;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SupportRequestWindowFeature(WindowCompat.FeatureActionBarOverlay);
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CameraViewDemo);

            SupportActionBar.Hide();

            cameraView = FindViewById<ScanbotCameraView>(Resource.Id.scanbotCameraView);

            // In this example we demonstrate how to lock the orientation of the UI (Activity)
            // as well as the orientation of the taken picture to portrait.
            cameraView.LockToPortrait(true);

            // Uncomment to disable AutoFocus by manually touching the camera view:
            //cameraView.SetAutoFocusOnTouch(false);

            // Preview Mode: See https://github.com/doo/Scanbot-SDK-Examples/wiki/Using-ScanbotCameraView#preview-mode
            //cameraView.SetPreviewMode(CameraPreviewMode.FitIn);

            userGuidanceTextView = FindViewById<TextView>(Resource.Id.userGuidanceTextView);

            imageProcessingProgress = FindViewById<ProgressBar>(Resource.Id.imageProcessingProgress);

            var detector = new IO.Scanbot.Sdk.ScanbotSDK(this).CreateContourDetector();
            contourDetectorFrameHandler = ContourDetectorFrameHandler.Attach(cameraView, detector);
            
            polygonView = FindViewById<PolygonView>(Resource.Id.scanbotPolygonView);
            polygonView.SetStrokeColor(Color.Red);
            polygonView.SetStrokeColorOK(Color.Green);

            // Attach the default polygon result handler, to draw the default polygon
            contourDetectorFrameHandler.AddResultHandler(polygonView.ContourDetectorResultHandler);
            // Add an additional custom contour detector to add user guidance text
            contourDetectorDelegate = new ContourDetectorDelegate();
            contourDetectorFrameHandler.AddResultHandler(contourDetectorDelegate);
            contourDetectorDelegate.ContourDetected += ShowUserGuidance;

            // See https://github.com/doo/Scanbot-SDK-Examples/wiki/Detecting-and-drawing-contours#contour-detection-parameters
            contourDetectorFrameHandler.SetAcceptedAngleScore(60);
            contourDetectorFrameHandler.SetAcceptedSizeScore(70);

            autoSnappingController = DocumentAutoSnappingController.Attach(cameraView, contourDetectorFrameHandler);
            autoSnappingController.SetIgnoreBadAspectRatio(ignoreBadAspectRatio);

            pictureCallbackDelegate = new PictureCallbackDelegate();
            pictureCallbackDelegate.OnPictureTakenHandler += ProcessTakenPicture;
            cameraView.AddPictureCallback(pictureCallbackDelegate);
            cameraView.SetCameraOpenCallback(this);

            shutterButton = FindViewById<ShutterButton>(Resource.Id.shutterButton);
            shutterButton.Click += delegate
            {
                cameraView.TakePicture(false);
            };
            shutterButton.Visibility = ViewStates.Visible;

            FindViewById(Resource.Id.scanbotFlashButton).Click += delegate
            {
                cameraView.UseFlash(!flashEnabled);
                flashEnabled = !flashEnabled;
            };

            autoSnappingToggleButton = FindViewById<Button>(Resource.Id.autoSnappingToggleButton);
            autoSnappingToggleButton.Click += delegate
            {
                autoSnappingEnabled = !autoSnappingEnabled;
                SetAutoSnapEnabled(autoSnappingEnabled);
            };

            shutterButton.Post(() =>
            {
                SetAutoSnapEnabled(autoSnappingEnabled);
            });
        }

        public void OnCameraOpened()
        {
            cameraView.PostDelayed(() =>
            {
                // Disable auto-focus sound:
                cameraView.SetAutoFocusSound(false);
                // Uncomment to disable shutter sound (supported since Android 4.2+):
                // Please note that some devices may not allow disabling the camera shutter sound. 
                // If the shutter sound state cannot be set to the desired value, this method will be ignored.
                cameraView.SetShutterSound(false);
                // Enable ContinuousFocus mode:
                cameraView.ContinuousFocus();
            }, 500);
        }

        protected override void OnResume()
        {
            base.OnResume();
            cameraView.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();
            cameraView.OnPause();
        }

        void ShowUserGuidance(object sender, ContourDetectorEventArgs e)
        {
            if (!autoSnappingEnabled) { return; }

            if (Java.Lang.JavaSystem.CurrentTimeMillis() - lastUserGuidanceHintTs < 400)
            {
                return;
            }

            var color = Color.Red;
            var guideText = "";

            var result = e.Frame.DetectionResult;
            if (result == DetectionResult.Ok)
            {
                guideText = "Don't move.\nCapturing...";
                color = Color.Green;
            }
            else if (result == DetectionResult.OkButTooSmall)
            {
                guideText = "Move closer";
            }
            else if (result == DetectionResult.OkButBadAngles)
            {
                guideText = "Perspective";
            }
            else if (result == DetectionResult.OkButBadAspectRatio)
            {
                guideText = "Wrong aspect ratio.\n Rotate your device";
                if (ignoreBadAspectRatio)
                {
                    guideText = "Don't move.\nCapturing...";
                    color = Color.Green;
                }
            }
            else if (result == DetectionResult.ErrorNothingDetected)
            {
                guideText = "No Document";
            }
            else if (result == DetectionResult.ErrorTooNoisy)
            {
                guideText = "Background too noisy";
            }
            else if (result == DetectionResult.ErrorTooDark)
            {
                guideText = "Poor light";
            }

            // The HandleResult callback is coming from a worker thread. Use main UI thread to update UI:
            userGuidanceTextView.Post(() =>
            {
                userGuidanceTextView.Text = guideText;
                userGuidanceTextView.SetTextColor(Color.White);
                userGuidanceTextView.SetBackgroundColor(color);
            });

            lastUserGuidanceHintTs = Java.Lang.JavaSystem.CurrentTimeMillis();
        }

        void ProcessTakenPicture(object sender, PictureCallbackEventArgs args)
        {
            // Here we get the full image from the camera and apply document detection on it.
            // Implement a suitable async(!) detection and image handling here.
            // This is just a demo showing detected image as downscaled preview image.

            Log.Debug(LOG_TAG, "OnPictureTaken: imageOrientation = " + args.imageOrientation);

            // Show progress spinner:
            RunOnUiThread(() =>
            {
                imageProcessingProgress.Visibility = ViewStates.Visible;
                userGuidanceTextView.Visibility = ViewStates.Gone;
            });

            // decode bytes as Bitmap
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InSampleSize = 1;
            var originalBitmap = BitmapFactory.DecodeByteArray(args.image, 0, args.image.Length, options);

            // rotate original image if required:
            if (args.imageOrientation > 0)
            {
                Matrix matrix = new Matrix();
                matrix.SetRotate(args.imageOrientation, originalBitmap.Width / 2f, originalBitmap.Height / 2f);
                originalBitmap = Bitmap.CreateBitmap(originalBitmap, 0, 0, originalBitmap.Width, originalBitmap.Height, matrix, false);
            }

            // Store the original image as file:
            var originalImgUri = MainApplication.TempImageStorage.AddImage(originalBitmap);

            Android.Net.Uri documentImgUri = null;
            // Run document detection on original image:
            var detectionResult = SBSDK.DetectDocument(originalBitmap);
            if (detectionResult.Status.IsOk())
            {
                var documentImage = detectionResult.Image as Bitmap;
                // Store the document image as file:
                documentImgUri = MainApplication.TempImageStorage.AddImage(documentImage);
            }
            else
            {
                // No document detected! Use original image as document image, so user can try to apply manual cropping.
                documentImgUri = originalImgUri;
            }

            //this.detectedPolygon = detectionResult.Polygon;

            Bundle extras = new Bundle();
            extras.PutString(EXTRAS_ARG_DOC_IMAGE_FILE_URI, documentImgUri.ToString());
            extras.PutString(EXTRAS_ARG_ORIGINAL_IMAGE_FILE_URI, originalImgUri.ToString());
            Intent intent = new Intent();
            intent.PutExtras(extras);
            SetResult(Result.Ok, intent);

            Finish();
            return;

            /* If you want to continue scanning:
            RunOnUiThread(() => {
                // continue camera preview
                cameraView.StartPreview();
                cameraView.ContinuousFocus();
            });
            */
        }

        protected void SetAutoSnapEnabled(bool enabled)
        {
            autoSnappingController.Enabled = enabled;
            contourDetectorFrameHandler.Enabled = enabled;
            polygonView.Visibility = (enabled ? ViewStates.Visible : ViewStates.Gone);
            autoSnappingToggleButton.Text = ("Automatic " + (enabled ? "ON" : "OFF"));
            if (enabled)
            {
                shutterButton.ShowAutoButton();
                userGuidanceTextView.Visibility = ViewStates.Visible;
            }
            else
            {
                shutterButton.ShowManualButton();
                userGuidanceTextView.Visibility = ViewStates.Gone;
            }
        }

    }

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
}

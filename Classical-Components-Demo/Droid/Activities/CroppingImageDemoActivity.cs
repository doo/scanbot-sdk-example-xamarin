using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Util;

using AndroidNetUri = Android.Net.Uri;

using ScanbotSDK.Xamarin.Android;
using AndroidX.AppCompat.App;
using IO.Scanbot.Sdk.UI;
using IO.Scanbot.Sdk.Core.Contourdetector;
using IO.Scanbot.Sdk.Process;

namespace ClassicalComponentsDemo.Droid
{
    [Activity(Theme = "@style/Theme.AppCompat")]
    public class CroppingImageDemoActivity : AppCompatActivity
    {
        static string LOG_TAG = typeof(CroppingImageDemoActivity).Name;

        public static String EXTRAS_ARG_IMAGE_FILE_URI = "EXTRAS_ARG_IMAGE_FILE_URI";

        static IList<PointF> DEFAULT_POLYGON = new List<PointF>();

        IO.Scanbot.Sdk.ScanbotSDK SDK;

        static CroppingImageDemoActivity()
        {
            DEFAULT_POLYGON.Add(new PointF(0, 0));
            DEFAULT_POLYGON.Add(new PointF(1, 0));
            DEFAULT_POLYGON.Add(new PointF(1f, 1f));
            DEFAULT_POLYGON.Add(new PointF(0, 1));
        }

        AndroidNetUri imageUri;
        Bitmap originalBitmap;
        EditPolygonImageView editPolygonImageView;
        MagnifierView scanbotMagnifierView;
        ProgressBar processImageProgressBar;
        View cancelBtn, doneBtn, rotateCWButton;

        int rotationDegrees = 0;
        long lastRotationEventTs = 0L;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CroppingImageDemo);

            SupportActionBar.SetDisplayShowHomeEnabled(false);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            SupportActionBar.SetDisplayShowCustomEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            SupportActionBar.SetCustomView(Resource.Layout.CroppingActionBarView);

            editPolygonImageView = FindViewById<EditPolygonImageView>(Resource.Id.scanbotEditImageView);
            scanbotMagnifierView = FindViewById<MagnifierView>(Resource.Id.scanbotMagnifierView);
            processImageProgressBar = FindViewById<ProgressBar>(Resource.Id.processImageProgressBar);

            cancelBtn = FindViewById<View>(Resource.Id.cancelButton);
            cancelBtn.Click += delegate
            {
                Finish();
            };

            doneBtn = FindViewById<View>(Resource.Id.doneButton);
            doneBtn.Click += delegate
            {
                cancelBtn.Enabled = false;
                doneBtn.Enabled = false;
                rotateCWButton.Enabled = false;
                CropAndSaveImage();
            };

            rotateCWButton = FindViewById<View>(Resource.Id.rotateCWButton);
            rotateCWButton.Click += delegate
            {
                if ((Java.Lang.JavaSystem.CurrentTimeMillis() - lastRotationEventTs) < 350)
                {
                    return;
                }
                rotationDegrees += 90;
                editPolygonImageView.RotateClockwise();
                lastRotationEventTs = Java.Lang.JavaSystem.CurrentTimeMillis();
            };

            string imageFileUri = Intent.Extras.GetString(EXTRAS_ARG_IMAGE_FILE_URI);
            imageUri = AndroidNetUri.Parse(imageFileUri);
            InitImageView();

             SDK = new IO.Scanbot.Sdk.ScanbotSDK(this);
        }

        void InitImageView()
        {
            Task.Run(() =>
            {
                try
                {
                    var polygon = DEFAULT_POLYGON;

                    originalBitmap = ImageLoader.Instance.Load(imageUri);
                    Bitmap resizedBitmap = ImageUtils.ResizeImage(originalBitmap, 1000, 1000);

                    RunOnUiThread(() =>
                    {
                        // important! first set the image and then the detected polygon and lines!
                        editPolygonImageView.SetImageBitmap(resizedBitmap);
                        // set up the MagnifierView every time when editPolygonView is set with a new image.
                        scanbotMagnifierView.SetupMagnifier(editPolygonImageView);
                    });

                    var detector = SDK.CreateContourDetector();
                    // Since we just need detected polygon and lines here, we use ContourDetector class from the native SDK namespace.
                    var detectionResult = detector.Detect(resizedBitmap);

                    if (detectionResult == DetectionResult.Ok || detectionResult == DetectionResult.OkButBadAngles ||
                        detectionResult == DetectionResult.OkButTooSmall || detectionResult == DetectionResult.OkButBadAspectRatio)
                    {
                        polygon = detector.PolygonF;
                        DebugLog("Detected polygon: " + polygon);
                    }

                    RunOnUiThread(() =>
                    {
                        editPolygonImageView.Polygon = polygon;
                        editPolygonImageView.SetLines(detector.HorizontalLines, detector.VerticalLines);
                    });

                }
                catch (Exception e)
                {
                    ErrorLog("Could not initialize image view", e);
                }
            });
        }

        void CropAndSaveImage()
        {
            processImageProgressBar.Visibility = ViewStates.Visible;
            cancelBtn.Visibility = ViewStates.Gone;
            doneBtn.Visibility = ViewStates.Gone;
            rotateCWButton.Visibility = ViewStates.Gone;

            Task.Run(() =>
            {
                try
                {
                    var detector = SDK.CreateContourDetector();
                    var documentImage = SDK.ImageProcessor().ProcessBitmap(originalBitmap, new CropOperation(editPolygonImageView.Polygon), false);
                    documentImage = SBSDK.RotateImage(documentImage, -rotationDegrees);
                    var documentImgUri = MainApplication.TempImageStorage.AddImage(documentImage);

                    RunOnUiThread(() =>
                    {
                        var extras = new Bundle();
                        extras.PutString(EXTRAS_ARG_IMAGE_FILE_URI, documentImgUri.ToString());
                        var intent = new Intent();
                        intent.PutExtras(extras);
                        SetResult(Result.Ok, intent);
                        Finish();
                    });
                }
                catch (Exception e)
                {
                    ErrorLog("Could not apply image changes", e);
                }
            });
        }


        void DebugLog(string msg)
        {
            Log.Debug(LOG_TAG, msg);
        }

        void ErrorLog(string msg)
        {
            Log.Error(LOG_TAG, msg);
        }

        void ErrorLog(string msg, Exception ex)
        {
            Log.Error(LOG_TAG, Java.Lang.Throwable.FromException(ex), msg);
        }

    }
}

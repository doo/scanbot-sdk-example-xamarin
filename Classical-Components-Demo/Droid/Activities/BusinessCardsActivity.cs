using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using IO.Scanbot.Sdk.Businesscard;
using IO.Scanbot.Sdk.Multipleobjects;
using IO.Scanbot.Sdk.Persistence;
using IO.Scanbot.Sdk.Process;
using IO.Scanbot.Sdk.UI.Camera;
using IO.Scanbot.Multipleobjectsscanner;
using Net.Doo.Snap.Camera;
using Net.Doo.Snap.Lib.Detector;
using Net.Doo.Snap.UI;

namespace ClassicalComponentsDemo.Droid.Activities
{
    [Activity(Label = "Business Card Scanner")]
    public class BusinessCardsActivity : BaseActivity, IPictureCallback, ICameraOpenCallback
    {
        public static List<BusinessCardsImageProcessorBusinessCardProcessingResult> ProcessedResults;

        ScanbotCameraView cameraView;
        ProgressBar progress;
        ShutterButton shutterButton;

        bool flashEnabled = false;

        MultipleObjectsDetectorParams modParams = null;
        // qualify only square-like objects:
        //MultipleObjectsDetectorParams modParams = new MultipleObjectsDetectorParams(0.9f, 1.1f);

        IO.Scanbot.Sdk.ScanbotSDK sdk;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.BusinessCardsLayout);

            cameraView = FindViewById<ScanbotCameraView>(Resource.Id.camera);
            cameraView.AddPictureCallback(this);
            cameraView.SetCameraOpenCallback(this);
            cameraView.SetAutoFocusSound(false);

            progress = FindViewById<ProgressBar>(Resource.Id.progressView);

            shutterButton = FindViewById<ShutterButton>(Resource.Id.snap);
            shutterButton.Click += delegate
            {
                cameraView.TakePicture(false);
            };
            shutterButton.Post(delegate
            {
                shutterButton.ShowAutoButton();
            });

            sdk = new IO.Scanbot.Sdk.ScanbotSDK(this);
            var detector = sdk.MultipleObjectsDetector();
            if (modParams != null)
            {
                detector.SetParams(modParams);
            }

            var handler = MultipleObjectsFrameHandler.Attach(cameraView, detector);

            var polygon = FindViewById<MultiplePolygonsView>(Resource.Id.polygonView);
            handler.AddResultHandler(polygon);

            FindViewById(Resource.Id.flash).Click += delegate
            {
                flashEnabled = !flashEnabled;
                cameraView.UseFlash(flashEnabled);
            };
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

        public void OnCameraOpened()
        {
            cameraView.PostDelayed(delegate
            {
                cameraView.SetShutterSound(true);
                cameraView.UseFlash(flashEnabled);
                cameraView.ContinuousFocus();

            }, 300);
        }

        public void OnPictureTaken(byte[] image, int imageOrientation)
        {
            cameraView.Post(delegate
            {
                progress.Visibility = ViewStates.Visible;
            });

            var bitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);

            var matrix = new Matrix();
            matrix.SetRotate(imageOrientation, bitmap.Width / 2, bitmap.Height / 2);
            var result = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, false);

            var detector = sdk.MultipleObjectsDetector();
            if (modParams != null)
            {
                detector.SetParams(modParams);
            }
            var polygons = detector.DetectOnBitmap(result, 0);
            var pages = new List<Page>();

            foreach (var polygon in polygons)
            {
                var id = sdk.PageFileStorage.Add(result);
                var page = new Page(id, new List<PointF>(), DetectionResult.Ok, ImageFilterType.Binarized);
                var cropped = sdk.PageProcessor().CropAndRotate(page, 0, polygon.PolygonF);
                pages.Add(cropped);
            }

            var processor = sdk.BusinessCardsImageProcessor();
            var languages = sdk.OcrRecognizer().InstalledLanguages;

            ProcessedResults = processor.ProcessPages(pages, languages, true, true).ToList();

            var i = 1;
            foreach (var item in ProcessedResults)
            {
                var text = item.OcrResult.RecognizedText;
                Console.WriteLine($"Recognized text on card {i}:\n {text}\n\n");
                i++;
            }

            RunOnUiThread(delegate
            {
                Toast.MakeText(this, $"Found {ProcessedResults.Count} business cards", ToastLength.Short).Show();
                progress.Visibility = ViewStates.Gone;
                var intent = new Intent(this, typeof(BusinessCardsPreviewActivity));
                StartActivity(intent);
            });
        }

    }
}

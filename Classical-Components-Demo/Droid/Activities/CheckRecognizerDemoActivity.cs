using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using ClassicalComponentsDemo.Droid.Delegates;
using IO.Scanbot.Check;
using IO.Scanbot.Sdk.Camera;
using IO.Scanbot.Sdk.Check;
using IO.Scanbot.Sdk.Check.Entity;
using IO.Scanbot.Sdk.Contourdetector;
using IO.Scanbot.Sdk.Core.Contourdetector;
using IO.Scanbot.Sdk.UI;
using IO.Scanbot.Sdk.UI.Camera;
using Javax.Xml.Transform.Dom;

namespace ClassicalComponentsDemo.Droid.Activities
{
    [Activity(Theme = "@style/Theme.AppCompat")]
    public class CheckRecognizerDemoActivity : AppCompatActivity
    {
        private ScanbotCameraXView cameraView;
        private TextView resultView;
        private CheckRecognizerFrameHandlerWrapper checkFrameHandlerWrapper;
        private IO.Scanbot.Sdk.ScanbotSDK scanbotSDK;
        private bool isFlashEnabled = false;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ActivityCheckRecognizer);

            cameraView = FindViewById<ScanbotCameraXView>(Resource.Id.camera);
            if (cameraView != null)
            {
                cameraView.SetPreviewMode(CameraPreviewMode.FitIn);
                cameraView.SetCameraOpenCallback(new Delegates.CameraOpenCallback(cameraView, isFlashEnabled));
            }

            resultView = FindViewById<TextView>(Resource.Id.result);
            scanbotSDK = new IO.Scanbot.Sdk.ScanbotSDK(this);
            ICheckRecognizer checkScanner = scanbotSDK.CreateCheckRecognizer();
            checkFrameHandlerWrapper = new CheckRecognizerFrameHandlerWrapper(checkScanner);
            var checkResultHandler = new CheckRecognizerResultDelegate();
            checkResultHandler.OnCheckRecognized += HandleCheckResult;
            checkFrameHandlerWrapper.AddResultHandler(checkResultHandler);
            ScanbotCameraXViewWrapper.Attach(cameraView, checkFrameHandlerWrapper);

            FindViewById<Button>(Resource.Id.flash).SetOnClickListener(new OnClickListener(() =>
            {
                this.isFlashEnabled = !this.isFlashEnabled;
                this.cameraView.UseFlash(this.isFlashEnabled);
            }));

            Toast.MakeText(
                this,
                scanbotSDK.IsLicenseActive ? "License is active" : "License Expired",
                ToastLength.Long
            ).Show();
        }

        private void CounterDetected(object sender, ContourDetectorEventArgs e)
        {
            
        }

        private void HandleCheckResult(object sender, CheckRecognizerResult result)
        {
            if (result.Status == IO.Scanbot.Check.Model.CheckRecognizerStatus.Success)
            {
                this.checkFrameHandlerWrapper.FrameHandler.Enabled = false;
                StartActivity(CheckRecognizerResultActivity.NewIntent(this, result));
            }
            else if (!this.scanbotSDK.IsLicenseActive)
            {
                this.checkFrameHandlerWrapper.FrameHandler.Enabled = false;
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "License is expired", ToastLength.Long).Show();
                    Finish();
                });
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (checkFrameHandlerWrapper?.FrameHandler != null)
            {
                this.checkFrameHandlerWrapper.FrameHandler.Enabled = true;
            }
        }

        public static Intent NewIntent(Context context)
        {
            return new Intent(context, typeof(CheckRecognizerDemoActivity));
        }
    }
}
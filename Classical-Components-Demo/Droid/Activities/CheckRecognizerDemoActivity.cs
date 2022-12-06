using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using IO.Scanbot.Check.Model;
using IO.Scanbot.Sdk.Camera;
using IO.Scanbot.Sdk.Check;
using IO.Scanbot.Sdk.Check.Entity;
using IO.Scanbot.Sdk.UI.Camera;

namespace ClassicalComponentsDemo.Droid.Activities
{
    [Activity(Theme = "@style/Theme.AppCompat")]
    public class CheckRecognizerDemoActivity: AppCompatActivity
    {
        private ScanbotCameraXView cameraView;
        private TextView resultView;
        private CheckRecognizerFrameHandler frameHandler;
        private IO.Scanbot.Sdk.ScanbotSDK scanbotSDK;

        private bool isFlashEnabled = false;

        private class CameraOpenCallback : Java.Lang.Object, ICameraOpenCallback
        {
            private ScanbotCameraXView cameraView;
            private bool isFlashEnabled;

            public CameraOpenCallback(ScanbotCameraXView cameraView, bool isFlashEnabled) {
                this.cameraView = cameraView;
                this.isFlashEnabled = isFlashEnabled;
            }

            public void OnCameraOpened()
            {
                this.cameraView.PostDelayed(() => {
                    this.cameraView.UseFlash(this.isFlashEnabled);
                    this.cameraView.ContinuousFocus();
                }, 700);
            }
        }

        private class OnClickListener : Java.Lang.Object, View.IOnClickListener
        {
            private Action action;
            public OnClickListener(Action action) { this.action = action; }
            public void OnClick(View v) { action.Invoke(); }
        }

        private class ResultHandler : CheckRecognizerFrameHandler.CheckRecognizerResultHandler
        {
            private Action<FrameHandlerResult> action;
            public ResultHandler(Action<FrameHandlerResult> action) { this.action = action; }

            public override bool Handle(FrameHandlerResult result)
            {
                this.action.Invoke(result);
                return false;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ActivityCheckRecognizer);

            cameraView = FindViewById<ScanbotCameraXView>(Resource.Id.camera);
            if (cameraView != null) {
                cameraView.SetPreviewMode(CameraPreviewMode.FitIn);
                cameraView.SetCameraOpenCallback(new CameraOpenCallback(cameraView, isFlashEnabled));
            }

            resultView = FindViewById<TextView>(Resource.Id.result);

            scanbotSDK = new IO.Scanbot.Sdk.ScanbotSDK(this);
            var checkScanner = scanbotSDK.CreateCheckRecognizer();
            frameHandler = CheckRecognizerFrameHandler.Attach(cameraView, checkScanner);
            frameHandler.AddResultHandler(new ResultHandler((result) => {
                if (result is FrameHandlerResult.Success success)
                {
                    var recognitionResult = success.Value as CheckRecognizerResult;
                    if (recognitionResult.Status == CheckRecognizerStatus.Success)
                    {
                        this.frameHandler.IsEnabled = false;
                        StartActivity(CheckRecognizerResultActivity.NewIntent(this, recognitionResult));
                    }
                    else if (!this.scanbotSDK.IsLicenseActive)
                    {
                        this.frameHandler.IsEnabled = false;
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "License is expired", ToastLength.Long).Show();
                            Finish();
                        });
                    }
                }
            }));

            FindViewById<Button>(Resource.Id.flash).SetOnClickListener(new OnClickListener(() => {
                this.isFlashEnabled = !this.isFlashEnabled;
                this.cameraView.UseFlash(this.isFlashEnabled);
            }));

            Toast.MakeText(
                this,
                scanbotSDK.IsLicenseActive ? "License is active" : "License Expired",
                ToastLength.Long
            ).Show();
        }

        protected override void OnResume()
        {
            base.OnResume();
            this.frameHandler.IsEnabled = true;
        }

        public static Intent NewIntent(Context context)
        {
            return new Intent(context, typeof(CheckRecognizerDemoActivity));
        }
    }
}


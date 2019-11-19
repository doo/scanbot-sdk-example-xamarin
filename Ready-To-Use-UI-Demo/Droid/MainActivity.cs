using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.Constraints;
using ReadyToUseUIDemo.model;
using Android.Views;
using System;
using ReadyToUseUIDemo.Droid.Views;
using ReadyToUseUIDemo.Droid.Fragments;
using IO.Scanbot.Sdk.UI.View.Workflow.Configuration;
using Net.Doo.Snap.Camera;
using Android.Support.V4.Content;
using Android.Graphics;
using IO.Scanbot.Sdk.UI.View.Workflow;
using Android.Content;
using Android.Runtime;
using System.Collections.Generic;

namespace ReadyToUseUIDemo.Droid
{
    [Activity(Label = "Ready-to-use UI Demo", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private const int DC_SCAN_WORKFLOW_REQUEST_CODE = 914;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var container = (ConstraintLayout)FindViewById(Resource.Id.constraintLayout);
            
            var child = new FragmentButton(this);
            child.SetAllCaps(false);

            var item = DataDetectors.Instance.Items[0];
            child.Data = item;

            child.Text = item.Title;

            child.LayoutParameters = GetParameters();
            container.AddView(child);
            
            child.Click += OnButtonClick;
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            Console.WriteLine("activity result something-something");
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            var button = (FragmentButton)sender;
            if (button.Data.Code == ListItemCode.ScanDC)
            {
                var configuration = new WorkflowScannerConfiguration();
                configuration.SetIgnoreBadAspectRatio(true);
                configuration.SetTopBarBackgroundColor(Color.White);
                //configuration.SetTopBarButtonsActiveColor(ContextCompat.GetColor(this, global::Android.Resource.Color.White);
                //configuration.SetTopBarButtonsInactiveColor(ContextCompat.GetColor(this, android.R.color.white));
                //configuration.SetTopBarBackgroundColor(ContextCompat.GetColor(this, R.color.colorPrimaryDark));
                //configuration.SetBottomBarBackgroundColor(ContextCompat.GetColor(this, R.color.colorPrimaryDark));
                configuration.SetCameraPreviewMode(CameraPreviewMode.FitIn);

                var filler = new Dictionary<Java.Lang.Class, Java.Lang.Class>();
                var intent = WorkflowScannerActivity.NewIntent(this, configuration,
                    WorkflowFactory.DisabilityCertificate, filler
                );
                StartActivityForResult(intent, DC_SCAN_WORKFLOW_REQUEST_CODE);
            }
        }

        ViewGroup.LayoutParams GetParameters()
        {
            var parameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent
            );

            var margin = (int)(3 * Resources.DisplayMetrics.Density);
            parameters.SetMargins(margin, margin, margin, margin);

            return parameters;
        }
        
    }
}


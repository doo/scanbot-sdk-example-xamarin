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
using Android.Support.V7.App;
using IO.Scanbot.Sdk.UI.Entity.Workflow;
using IO.Scanbot.Sdk.UI.View.Mrz.Configuration;
using IO.Scanbot.Sdk.UI.View.Mrz;
using IO.Scanbot.Mrzscanner.Model;
using IO.Scanbot.Sdk.UI.View.Camera.Configuration;
using IO.Scanbot.Sdk.UI.View.Camera;
using System.Linq;
using IO.Scanbot.Sdk.Persistence;
using ReadyToUseUIDemo.Droid.Repository;
using ScanbotSDK.Xamarin.Android;
using ReadyToUseUIDemo.Droid.Activities;

namespace ReadyToUseUIDemo.Droid
{
    [Activity(Label = "Ready-to-use UI Demo", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : AppCompatActivity
    {
        private const int CAMERA_DEFAULT_UI_REQUEST_CODE = 1111;

        private const int MRZ_DEFAULT_UI_REQUEST_CODE = 909;
        private const int DC_SCAN_WORKFLOW_REQUEST_CODE = 914;

        readonly List<FragmentButton> buttons = new List<FragmentButton>();

        View LicenseIndicator
        {
            get
            {
                var container = FindViewById(Resource.Id.container);
                return container.FindViewById(Resource.Id.licenseIndicator);
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var container = (LinearLayout)FindViewById(Resource.Id.container);
            
            var scanner = (LinearLayout)container.FindViewById(Resource.Id.document_scanner);
            var scannerTitle = (TextView)scanner.FindViewById(Resource.Id.textView);
            scannerTitle.Text = DocumentScanner.Instance.Title;

            foreach (ListItem item in DocumentScanner.Instance.Items)
            {
                var child = new FragmentButton(this)
                {
                    Data = item,
                    Text = item.Title,
                    LayoutParameters = GetParameters()
                };
                scanner.AddView(child);
                buttons.Add(child);
            }

            var collectors = (LinearLayout)container.FindViewById(Resource.Id.data_collectors);
            var collectorsTitle = (TextView)collectors.FindViewById(Resource.Id.textView);
            collectorsTitle.Text = DataDetectors.Instance.Title;

            foreach (ListItem item in DataDetectors.Instance.Items)
            {
                var child = new FragmentButton(this)
                {
                    Data = item,
                    Text = item.Title,
                    LayoutParameters = GetParameters()
                };
                collectors.AddView(child);
                buttons.Add(child);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
    
            if (SBSDK.IsLicenseValid())
            {
                LicenseIndicator.Visibility = ViewStates.Gone;
            }
            else
            {
                LicenseIndicator.Visibility = ViewStates.Visible;
            }

            foreach (var button in buttons)
            {
                button.Click += OnButtonClick;
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            foreach (var button in buttons)
            {
                button.Click -= OnButtonClick;
            }
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            var button = (FragmentButton)sender;

            if (button.Data.Code == ListItemCode.ScanDocument)
            {
                var configuration = new DocumentScannerConfiguration();
                configuration.SetCameraPreviewMode(CameraPreviewMode.FitIn);
                configuration.SetIgnoreBadAspectRatio(true);
                configuration.SetMultiPageEnabled(true);
                configuration.SetPageCounterButtonTitle("%d Page(s)");
                configuration.SetTextHintOK("Don't move.\nCapturing document...");

                var intent = DocumentScannerActivity.NewIntent(this, configuration);
                StartActivityForResult(intent, CAMERA_DEFAULT_UI_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.ScanDC)
            {
                var configuration = new WorkflowScannerConfiguration();
                configuration.SetIgnoreBadAspectRatio(true);
                configuration.SetTopBarBackgroundColor(Color.White);
                configuration.SetCameraPreviewMode(CameraPreviewMode.FitIn);

                var filler = new Dictionary<Java.Lang.Class, Java.Lang.Class>();
                var intent = WorkflowScannerActivity.NewIntent(this, configuration,
                    WorkflowFactory.DisabilityCertificate, filler
                );
                StartActivityForResult(intent, DC_SCAN_WORKFLOW_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.ScanMRZ)
            {
                var configuration = new MRZScannerConfiguration();
                configuration.SetSuccessBeepEnabled(false);

                var intent = MRZScannerActivity.NewIntent(this, configuration);
                StartActivityForResult(intent, MRZ_DEFAULT_UI_REQUEST_CODE);
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok)
            {
                return;
            }

            if (requestCode == CAMERA_DEFAULT_UI_REQUEST_CODE)
            {
                var parcelable = data.GetParcelableArrayExtra(DocumentScannerActivity.SnappedPageExtra);
                var pages = parcelable.Cast<Page>().ToList();

                PageRepository.Add(pages);
                var intent = new Intent(this, typeof(PagePreviewActivity));
                StartActivity(intent);
            }
            else if (requestCode == DC_SCAN_WORKFLOW_REQUEST_CODE)
            {
                var workflow = (Workflow)data.GetParcelableExtra(WorkflowScannerActivity.WorkflowExtra);
                var results = (List<WorkflowStepResult>)data.GetParcelableArrayListExtra(WorkflowScannerActivity.WorkflowResultExtra);
                var fragment = DCResultDialogFragment.CreateInstance(workflow, results);
                fragment.Show(SupportFragmentManager, DCResultDialogFragment.NAME);
            }
            else if (requestCode == MRZ_DEFAULT_UI_REQUEST_CODE)
            {
                var result = (MRZRecognitionResult)data.GetParcelableExtra(MRZScannerActivity.ExtractedFieldsExtra);
                var fragment = MRZDialogFragment.CreateInstance(result);
                fragment.Show(SupportFragmentManager, MRZDialogFragment.NAME);
            }
        }

        ViewGroup.LayoutParams GetParameters()
        {
            var parameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent
            );

            var margin = (int)(3 * Resources.DisplayMetrics.Density);
            parameters.SetMargins(0, margin, 0, margin);

            return parameters;
        }
        
    }
}


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
using ReadyToUseUIDemo.Droid.Utils;
using System.Threading.Tasks;
using Android.Provider;
using System.IO;
using IO.Scanbot.Sdk.UI.View.Edit.Configuration;
using IO.Scanbot.Sdk.UI.View.Edit;
using Net.Doo.Snap.Lib.Detector;
using IO.Scanbot.Sdk.Process;
using IO.Scanbot.Sdk.UI.View.Barcode.Configuration;
using IO.Scanbot.Sdk.UI.View.Barcode;
using IO.Scanbot.Sdk.Barcode.Entity;
using IO.Scanbot.Sdk.UI.View.Hic.Configuration;
using IO.Scanbot.Sdk.UI.View.Hic;
using IO.Scanbot.Hicscanner.Model;

namespace ReadyToUseUIDemo.Droid
{
    [Activity(Label = "Ready-to-use UI Demo", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : AppCompatActivity
    {
        private const int CAMERA_DEFAULT_UI_REQUEST_CODE = 1111;

        const int MRZ_DEFAULT_UI_REQUEST_CODE = 909;
        const int DC_SCAN_WORKFLOW_REQUEST_CODE = 914;
        const int PAYFORM_SCAN_WORKFLOW_REQUEST_CODE = 916;
        const int MRZ_SNAP_WORKFLOW_REQUEST_CODE = 912;
        const int MRZ_FRONBACK_SNAP_WORKFLOW_REQUEST_CODE = 913;
        const int QR_BARCODE_DEFAULT_UI_REQUEST_CODE = 910;
        const int REQUEST_EHIC_SCAN = 4715;

        private const int IMPORT_IMAGE_REQUEST = 7777;
        private const int CROP_DEFAULT_UI_REQUEST = 9999;

        readonly List<FragmentButton> buttons = new List<FragmentButton>();

        ProgressBar progress;

        TextView LicenseIndicator
        {
            get
            {
                var container = FindViewById(Resource.Id.container);
                return container.FindViewById<TextView>(Resource.Id.licenseIndicator);
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var container = (LinearLayout)FindViewById(Resource.Id.container);

            var title = container.FindViewById<TextView>(Resource.Id.textView);
            title.Text = Texts.scanbot_sdk_demo;

            progress = FindViewById<ProgressBar>(Resource.Id.progressBar);

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

            LicenseIndicator.Text = Texts.no_license_found_the_app_will_terminate_after_one_minute;
        }

        protected override void OnResume()
        {
            base.OnResume();

            CheckLicense();

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

        bool CheckLicense()
        {
            if (SBSDK.IsLicenseValid())
            {
                LicenseIndicator.Visibility = ViewStates.Gone;
            }
            else
            {
                LicenseIndicator.Visibility = ViewStates.Visible;
                Alert.Toast(this, "Invalid or missing license");
            }

            return SBSDK.IsLicenseValid();
        }


        // WorkflowScannerActivity workflowScanners parameter is an optional parameter,
        // however, the generated Kotlin -> C# bindings do not that take that into account.
        // Just create an empty Dictionary and enter it as a parameter
        Dictionary<Java.Lang.Class, Java.Lang.Class> WorkflowScanners
        {
            get => new Dictionary<Java.Lang.Class, Java.Lang.Class>();
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            if (!CheckLicense())
            {
                return;
            }

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
            else if (button.Data.Code == ListItemCode.ImportImage)
            {
                var intent = new Intent();
                intent.SetType("image/*");
                intent.SetAction(Intent.ActionGetContent);
                intent.PutExtra(Intent.ExtraLocalOnly, false);
                intent.PutExtra(Intent.ExtraAllowMultiple, false);

                var chooser = Intent.CreateChooser(intent, Texts.share_title);
                StartActivityForResult(chooser, IMPORT_IMAGE_REQUEST);
            }
            else if (button.Data.Code == ListItemCode.ViewImages)
            {
                var intent = new Intent(this, typeof(PagePreviewActivity));
                StartActivity(intent);
            }
            else if (button.Data.Code == ListItemCode.WorkflowDC)
            {
                var configuration = new WorkflowScannerConfiguration();
                configuration.SetIgnoreBadAspectRatio(true);
                configuration.SetTopBarBackgroundColor(Color.White);
                configuration.SetCameraPreviewMode(CameraPreviewMode.FitIn);

                var intent = WorkflowScannerActivity.NewIntent(this, configuration,
                    WorkflowFactory.DisabilityCertificate, WorkflowScanners
                );
                StartActivityForResult(intent, DC_SCAN_WORKFLOW_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.ScannerMRZ)
            {
                var configuration = new MRZScannerConfiguration();
                configuration.SetSuccessBeepEnabled(false);

                var intent = MRZScannerActivity.NewIntent(this, configuration);
                StartActivityForResult(intent, MRZ_DEFAULT_UI_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.WorkflowMRZImage)
            {
                var configuration = new WorkflowScannerConfiguration();
                configuration.SetIgnoreBadAspectRatio(true);

                var flow = WorkflowFactory.ScanMRZAndSnap;
                var intent = WorkflowScannerActivity.NewIntent(this, configuration, flow, WorkflowScanners);
                StartActivityForResult(intent, MRZ_SNAP_WORKFLOW_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.WorkflowMRZFrontBack)
            {
                var configuration = new WorkflowScannerConfiguration();
                configuration.SetIgnoreBadAspectRatio(true);

                var flow = WorkflowFactory.ScanMRZAndFrontBackSnap;
                var intent = WorkflowScannerActivity.NewIntent(this, configuration, flow, WorkflowScanners);
                StartActivityForResult(intent, MRZ_FRONBACK_SNAP_WORKFLOW_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.WorkflowSEPA)
            {
                var configuration = new WorkflowScannerConfiguration();
                configuration.SetIgnoreBadAspectRatio(true);
                configuration.SetCameraPreviewMode(CameraPreviewMode.FitIn);

                var flow = WorkflowFactory.PayFormWithClassicalDocPolygonDetection;
                var intent = WorkflowScannerActivity.NewIntent(this, configuration, flow, WorkflowScanners);

                StartActivityForResult(intent, PAYFORM_SCAN_WORKFLOW_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.ScannerBarcode)
            {
                var configuration = new BarcodeScannerConfiguration();
                configuration.SetFinderTextHint("Please align the QR-/Barcode in the frame above to scan it");
                var intent = BarcodeScannerActivity.NewIntent(this, configuration);
                StartActivityForResult(intent, QR_BARCODE_DEFAULT_UI_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.ScannerEHIC)
            {
                var config = new HealthInsuranceCardScannerConfiguration();
                config.SetTopBarButtonsColor(Color.White);

                var intent = HealthInsuranceCardScannerActivity.NewIntent(this, config);
                StartActivityForResult(intent, REQUEST_EHIC_SCAN);
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
            else if (requestCode == IMPORT_IMAGE_REQUEST)
            {
                if (!SBSDK.IsLicenseValid())
                {
                    Alert.ShowLicenseDialog(this);
                    return;
                }

                progress.Visibility = ViewStates.Visible;
                Alert.Toast(this, Texts.importing_and_processing);
                Task.Run(delegate
                {
                    var result = ProcessGalleryResult(data);

                    var pageId = SBSDK.PageStorage.Add(result);
                    var page = new Page(pageId, new List<PointF>(), DetectionResult.Ok, ImageFilterType.None);
                    page = SBSDK.PageProcessor.DetectDocument(page);
                    PageRepository.Add(page);

                    var intent = new Intent(this, typeof(PagePreviewActivity));
                    RunOnUiThread(delegate
                    {
                        progress.Visibility = ViewStates.Gone;
                        StartActivity(intent);
                    });
                });
            }
            else if (requestCode == CROP_DEFAULT_UI_REQUEST)
            {
                var page = data.GetParcelableExtra(CroppingActivity.EditedPageExtra) as Page;
                PageRepository.Add(page);
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
            else if (requestCode == MRZ_SNAP_WORKFLOW_REQUEST_CODE)
            {
                var workflow = (Workflow)data.GetParcelableExtra(WorkflowScannerActivity.WorkflowExtra);
                var results = (List<WorkflowStepResult>)data.GetParcelableArrayListExtra(WorkflowScannerActivity.WorkflowResultExtra);
                var fragment = MRZImageResultDialogFragment.CreateInstance(workflow, results);
                fragment.Show(SupportFragmentManager, MRZImageResultDialogFragment.NAME);
            }
            else if (requestCode == MRZ_FRONBACK_SNAP_WORKFLOW_REQUEST_CODE)
            {
                var workflow = (Workflow)data.GetParcelableExtra(WorkflowScannerActivity.WorkflowExtra);
                var results = (List<WorkflowStepResult>)data.GetParcelableArrayListExtra(WorkflowScannerActivity.WorkflowResultExtra);
                var fragment = MRZFrontBackImageResultDialogFragment.CreateInstance(workflow, results);
                fragment.Show(SupportFragmentManager, MRZFrontBackImageResultDialogFragment.NAME);
            }
            else if (requestCode == QR_BARCODE_DEFAULT_UI_REQUEST_CODE)
            {
                var code = (BarcodeScanningResult)data.GetParcelableExtra(BarcodeScannerActivity.ScannedBarcodeExtra);
                var fragment = BarcodeDialogFragment.CreateInstance(code);
                fragment.Show(SupportFragmentManager, BarcodeDialogFragment.NAME);
            }
            else if (requestCode == REQUEST_EHIC_SCAN)
            {
                var result = (HealthInsuranceCardRecognitionResult)data.GetParcelableExtra(
                    HealthInsuranceCardScannerActivity.ExtractedFieldsExtra);
                
                var fragment = HealthInsuranceCardFragment.CreateInstance(result);
                fragment.Show(SupportFragmentManager, HealthInsuranceCardFragment.NAME);
            }
            else if (requestCode == PAYFORM_SCAN_WORKFLOW_REQUEST_CODE)
            {
                var workflow = (Workflow)data.GetParcelableExtra(WorkflowScannerActivity.WorkflowExtra);
                var results = (List<WorkflowStepResult>)data.GetParcelableArrayListExtra(WorkflowScannerActivity.WorkflowResultExtra);
                var fragment = PayFormResultDialogFragment.CreateInstance(workflow, results);
                fragment.Show(SupportFragmentManager, PayFormResultDialogFragment.NAME);
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

        Bitmap ProcessGalleryResult(Intent data)
        {
            var imageUri = data.Data;
            Bitmap bitmap = null;
            if (imageUri != null)
            {
                try
                {
                    var source = ImageDecoder.CreateSource(ContentResolver, imageUri);
                    bitmap = ImageDecoder.DecodeBitmap(source);
                }
                catch (IOException)
                {
                }
            }

            return bitmap;
        }
    }
}


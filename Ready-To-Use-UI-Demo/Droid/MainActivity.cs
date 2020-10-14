using Android.App;
using Android.Widget;
using Android.OS;
using ReadyToUseUIDemo.model;
using Android.Views;
using System;
using ReadyToUseUIDemo.Droid.Views;
using ReadyToUseUIDemo.Droid.Fragments;
using IO.Scanbot.Sdk.UI.View.Workflow.Configuration;
using Android.Graphics;
using IO.Scanbot.Sdk.UI.View.Workflow;
using Android.Content;
using Android.Runtime;
using System.Collections.Generic;
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
using System.IO;
using IO.Scanbot.Sdk.UI.View.Edit;
using IO.Scanbot.Sdk.Process;
using IO.Scanbot.Sdk.UI.View.Barcode.Configuration;
using IO.Scanbot.Sdk.UI.View.Barcode;
using IO.Scanbot.Sdk.Barcode.Entity;
using IO.Scanbot.Sdk.UI.View.Hic.Configuration;
using IO.Scanbot.Sdk.UI.View.Hic;
using IO.Scanbot.Hicscanner.Model;
using IO.Scanbot.Sdk.Camera;
using IO.Scanbot.Sdk.Core.Contourdetector;
using AndroidX.AppCompat.App;
using IO.Scanbot.Sdk.UI.View.Barcode.Batch.Configuration;
using IO.Scanbot.Sdk.UI.View.Barcode.Batch;
using IO.Scanbot.Sdk.UI.View.Nfc.Configuration;
using IO.Scanbot.Sdk.UI.View.Nfc;
using IO.Scanbot.Sdk.UI.View.Nfc.Entity;
using IO.Scanbot.Sdk.UI.View.Generictext.Configuration;
using IO.Scanbot.Sdk.UI.View.Generictext;
using IO.Scanbot.Sdk.UI.Camera;
using IO.Scanbot.Sdk.UI.View.Generictext.Entity;

namespace ReadyToUseUIDemo.Droid
{
    [Activity(Label = "Ready-to-use UI Demo", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : AppCompatActivity
    {
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
            AddItemsTo(scanner, DocumentScanner.Instance.Items);

            var barcodes = (LinearLayout)container.FindViewById(Resource.Id.barcode_detectors);
            var barcodeTitle = (TextView)barcodes.FindViewById(Resource.Id.textView);
            barcodeTitle.Text = BarcodeDetectors.Instance.Title;
            AddItemsTo(barcodes, BarcodeDetectors.Instance.Items);

            var detectors = (LinearLayout)container.FindViewById(Resource.Id.data_detectors);
            var detectorsTitle = (TextView)detectors.FindViewById(Resource.Id.textView);
            detectorsTitle.Text = DataDetectors.Instance.Title;
            AddItemsTo(detectors, DataDetectors.Instance.Items);

            LicenseIndicator.Text = Texts.no_license_found_the_app_will_terminate_after_one_minute;
        }

        void AddItemsTo(LinearLayout container, List<ListItem> items)
        {
            foreach (ListItem item in items)
            {
                var child = new FragmentButton(this)
                {
                    Data = item,
                    Text = item.Title,
                    LayoutParameters = ViewUtils.GetParameters(this)
                };
                container.AddView(child);
                buttons.Add(child);
            }
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

        void StartImportActivity(int resultConstant)
        {
            var intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);
            intent.PutExtra(Intent.ExtraLocalOnly, false);
            intent.PutExtra(Intent.ExtraAllowMultiple, false);

            var chooser = Intent.CreateChooser(intent, Texts.share_title);
            StartActivityForResult(chooser, resultConstant);
        }
        /**
         * Start Scanner or Import Activity
         */
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
                //configuration.SetBottomBarBackgroundColor(Color.Blue);
                //configuration.SetBottomBarButtonsColor(Color.White);
                // see further customization configs...

                var intent = DocumentScannerActivity.NewIntent(this, configuration);
                StartActivityForResult(intent, Constants.CAMERA_DEFAULT_UI_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.ImportImage)
            {
                StartImportActivity(Constants.IMPORT_IMAGE_REQUEST);
            }
            else if (button.Data.Code == ListItemCode.ViewImages)
            {
                var intent = new Intent(this, typeof(PagePreviewActivity));
                StartActivity(intent);
            }

            // Barcode Detectors
            else if (button.Data.Code == ListItemCode.ScannerBarcode)
            {
                var configuration = new BarcodeScannerConfiguration();
                configuration.SetFinderTextHint("Please align the QR-/Barcode in the frame above to scan it");
                var intent = BarcodeScannerActivity.NewIntent(this, configuration);
                StartActivityForResult(intent, Constants.QR_BARCODE_DEFAULT_UI_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.ScannerBatchBarcode)
            {
                var configuration = new BatchBarcodeScannerConfiguration();
                configuration.SetFinderTextHint("Please align the QR-/Barcode in the frame above to scan it");
                var intent = BatchBarcodeScannerActivity.NewIntent(this, configuration, null);
                StartActivityForResult(intent, Constants.QR_BARCODE_DEFAULT_UI_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.ScannerImportBarcode)
            {
                StartImportActivity(Constants.IMPORT_BARCODE_REQUEST);
            }

            // Other Data Detectors
            else if (button.Data.Code == ListItemCode.ScannerText)
            {
                var config = new TextDataScannerConfiguration();

                var tag = "tag";
                var title = "";
                var guidance = "Move the viewfinder over the text you wish to recognize";
                var pattern = "#### ######";
                var shouldMatch = true;
                var validation = new ValidationCallback();
                var recognition = new RecognitionCallback();
                var preferredZoom = 1.4f;
                var ratio = new FinderAspectRatio(4.0, 1.0);
                var unzoomedHeight = 40f;
                var allowedSymbols = new List<Java.Lang.Character>();

                var step = new TextDataScannerStep(tag, title, guidance,
                    pattern, shouldMatch, validation, recognition,
                    preferredZoom, ratio, unzoomedHeight, allowedSymbols);

                var intent = TextDataScannerActivity.NewIntent(this, config, step);

                StartActivityForResult(intent, (int)ListItemCode.ScannerText);
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
                StartActivityForResult(intent, Constants.DC_SCAN_WORKFLOW_REQUEST_CODE);
            }

            else if (button.Data.Code == ListItemCode.ScannerMRZ)
            {
                var configuration = new MRZScannerConfiguration();
                configuration.SetSuccessBeepEnabled(false);

                var intent = MRZScannerActivity.NewIntent(this, configuration);
                StartActivityForResult(intent, Constants.MRZ_DEFAULT_UI_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.ScannerNFC)
            {
                var configuration = new NfcPassportConfiguration();
                configuration.SetShouldSavePhotoImageInStorage(true);
                
                configuration.SetPassportPhotoSaveCallback(new NfcDialogFragment.PassportCallback().Class);
                var intent = NfcPassportScannerActivity.NewIntent(this, configuration);
                StartActivityForResult(intent, Constants.NFC_DEFAULT_UI_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.WorkflowMRZImage)
            {
                var configuration = new WorkflowScannerConfiguration();
                configuration.SetIgnoreBadAspectRatio(true);

                var flow = WorkflowFactory.ScanMRZAndSnap;
                var intent = WorkflowScannerActivity.NewIntent(this, configuration, flow, WorkflowScanners);
                StartActivityForResult(intent, Constants.MRZ_SNAP_WORKFLOW_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.WorkflowMRZFrontBack)
            {
                var configuration = new WorkflowScannerConfiguration();
                configuration.SetIgnoreBadAspectRatio(true);

                var flow = WorkflowFactory.ScanMRZAndFrontBackSnap;
                var intent = WorkflowScannerActivity.NewIntent(this, configuration, flow, WorkflowScanners);
                StartActivityForResult(intent, Constants.MRZ_FRONBACK_SNAP_WORKFLOW_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.WorkflowSEPA)
            {
                var configuration = new WorkflowScannerConfiguration();
                configuration.SetIgnoreBadAspectRatio(true);
                configuration.SetCameraPreviewMode(CameraPreviewMode.FitIn);

                var flow = WorkflowFactory.PayFormWithClassicalDocPolygonDetection;
                var intent = WorkflowScannerActivity.NewIntent(this, configuration, flow, WorkflowScanners);

                StartActivityForResult(intent, Constants.PAYFORM_SCAN_WORKFLOW_REQUEST_CODE);
            }

            else if (button.Data.Code == ListItemCode.ScannerEHIC)
            {
                var config = new HealthInsuranceCardScannerConfiguration();
                config.SetTopBarButtonsColor(Color.White);

                var intent = HealthInsuranceCardScannerActivity.NewIntent(this, config);
                StartActivityForResult(intent, Constants.REQUEST_EHIC_SCAN);
            }
        }

        /**
         * Scanner returned, parse results
         */
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok)
            {
                return;
            }

            if (requestCode == (int)ListItemCode.ScannerText)
            {
                // No need to process result, see TextDataScanner.cs
                // for the Validation and Recognition callbacks
            }
            else if (requestCode == Constants.CAMERA_DEFAULT_UI_REQUEST_CODE)
            {
                var parcelable = data.GetParcelableArrayExtra(DocumentScannerActivity.SnappedPageExtra);
                var pages = parcelable.Cast<Page>().ToList();

                PageRepository.Add(pages);
                var intent = new Intent(this, typeof(PagePreviewActivity));
                StartActivity(intent);
            }
            else if (requestCode == Constants.IMPORT_IMAGE_REQUEST)
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
                    var result = Utils.ImageUtils.ProcessGalleryResult(this, data);

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
            else if (requestCode == Constants.IMPORT_BARCODE_REQUEST)
            {
                Task.Run(delegate
                {
                    var bitmap = Utils.ImageUtils.ProcessGalleryResult(this, data);
                    var detector = new IO.Scanbot.Sdk.ScanbotSDK(this).BarcodeDetector();
                    var result = detector.DetectFromBitmap(bitmap, 0);
                    var fragment = BarcodeDialogFragment.CreateInstance(result);

                    // Estimate blur of imported barcode
                    // Estimating blur on already cropped barcodes should
                    // normally yield the best results, as there is little empty space
                    var estimator = new IO.Scanbot.Sdk.ScanbotSDK(this).BlurEstimator();
                    fragment.Blur = estimator.EstimateInBitmap(bitmap, 0);
                    fragment.Show(SupportFragmentManager, BarcodeDialogFragment.NAME);
                });
            }
            else if (requestCode == Constants.QR_BARCODE_DEFAULT_UI_REQUEST_CODE)
            {
                var result = (BarcodeScanningResult)data.GetParcelableExtra(BarcodeScannerActivity.ScannedBarcodeExtra);
                var fragment = BarcodeDialogFragment.CreateInstance(result);
                fragment.Show(SupportFragmentManager, BarcodeDialogFragment.NAME);
            }
            else if (requestCode == Constants.CROP_DEFAULT_UI_REQUEST)
            {
                var page = data.GetParcelableExtra(CroppingActivity.EditedPageExtra) as Page;
                PageRepository.Add(page);
            }
            else if (requestCode == Constants.DC_SCAN_WORKFLOW_REQUEST_CODE)
            {
                var workflow = (Workflow)data.GetParcelableExtra(WorkflowScannerActivity.WorkflowExtra);
                var results = (List<WorkflowStepResult>)data.GetParcelableArrayListExtra(WorkflowScannerActivity.WorkflowResultExtra);
                var fragment = DCResultDialogFragment.CreateInstance(workflow, results);
                fragment.Show(SupportFragmentManager, DCResultDialogFragment.NAME);
            }
            else if (requestCode == Constants.MRZ_DEFAULT_UI_REQUEST_CODE)
            {
                var result = (MRZRecognitionResult)data.GetParcelableExtra(MRZScannerActivity.ExtractedFieldsExtra);
                var fragment = MRZDialogFragment.CreateInstance(result);
                fragment.Show(SupportFragmentManager, MRZDialogFragment.NAME);
            }
            else if (requestCode == Constants.NFC_DEFAULT_UI_REQUEST_CODE)
            {
                var result = (NfcPassportScanningResult)data.GetParcelableExtra(NfcPassportScannerActivity.ExtractedFieldsExtra);
                var fragment = NfcDialogFragment.CreateInstance(result);
                fragment.Show(SupportFragmentManager, NfcDialogFragment.NAME);
            }
            else if (requestCode == Constants.MRZ_SNAP_WORKFLOW_REQUEST_CODE)
            {
                var workflow = (Workflow)data.GetParcelableExtra(WorkflowScannerActivity.WorkflowExtra);
                var results = (List<WorkflowStepResult>)data.GetParcelableArrayListExtra(WorkflowScannerActivity.WorkflowResultExtra);
                var fragment = MRZImageResultDialogFragment.CreateInstance(workflow, results);
                fragment.Show(SupportFragmentManager, MRZImageResultDialogFragment.NAME);
            }
            else if (requestCode == Constants.MRZ_FRONBACK_SNAP_WORKFLOW_REQUEST_CODE)
            {
                var workflow = (Workflow)data.GetParcelableExtra(WorkflowScannerActivity.WorkflowExtra);
                var results = (List<WorkflowStepResult>)data.GetParcelableArrayListExtra(WorkflowScannerActivity.WorkflowResultExtra);
                var fragment = MRZFrontBackImageResultDialogFragment.CreateInstance(workflow, results);
                fragment.Show(SupportFragmentManager, MRZFrontBackImageResultDialogFragment.NAME);
            }
            else if (requestCode == Constants.REQUEST_EHIC_SCAN)
            {
                var result = (HealthInsuranceCardRecognitionResult)data.GetParcelableExtra(
                    HealthInsuranceCardScannerActivity.ExtractedFieldsExtra);
                
                var fragment = HealthInsuranceCardFragment.CreateInstance(result);
                fragment.Show(SupportFragmentManager, HealthInsuranceCardFragment.NAME);
            }
            else if (requestCode == Constants.PAYFORM_SCAN_WORKFLOW_REQUEST_CODE)
            {
                var workflow = (Workflow)data.GetParcelableExtra(WorkflowScannerActivity.WorkflowExtra);
                var results = (List<WorkflowStepResult>)data.GetParcelableArrayListExtra(WorkflowScannerActivity.WorkflowResultExtra);
                var fragment = PayFormResultDialogFragment.CreateInstance(workflow, results);
                fragment.Show(SupportFragmentManager, PayFormResultDialogFragment.NAME);
            }
        }
    }
}


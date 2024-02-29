using Android.App;
using AndroidX.AppCompat.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using ReadyToUseUIDemo.Droid.Activities;
using ReadyToUseUIDemo.Droid.Fragments;
using ReadyToUseUIDemo.Droid.Repository;
using ReadyToUseUIDemo.Droid.Utils;
using ReadyToUseUIDemo.Droid.Views;
using ReadyToUseUIDemo.model;

using IO.Scanbot.Genericdocument.Entity;
using IO.Scanbot.Hicscanner.Model;
using IO.Scanbot.Mrzscanner.Model;

using IO.Scanbot.Sdk.Barcode.Entity;
using IO.Scanbot.Sdk.Camera;
using IO.Scanbot.Sdk.Check.Entity;
using IO.Scanbot.Sdk.Core.Contourdetector;
using IO.Scanbot.Sdk.Persistence;
using IO.Scanbot.Sdk.Process;

using IO.Scanbot.Sdk.UI.Result;
using IO.Scanbot.Sdk.UI.View.Barcode.Configuration;
using IO.Scanbot.Sdk.UI.View.Barcode;
using IO.Scanbot.Sdk.UI.View.Barcode.Batch.Configuration;
using IO.Scanbot.Sdk.UI.View.Barcode.Batch;
using IO.Scanbot.Sdk.UI.View.Base;
using IO.Scanbot.Sdk.UI.View.Camera.Configuration;
using IO.Scanbot.Sdk.UI.View.Camera;
using IO.Scanbot.Sdk.UI.View.Check.Configuration;
using IO.Scanbot.Sdk.UI.View.Check;
using IO.Scanbot.Sdk.UI.View.Genericdocument.Configuration;
using IO.Scanbot.Sdk.UI.View.Genericdocument;
using IO.Scanbot.Sdk.UI.View.Generictext;
using IO.Scanbot.Sdk.UI.View.Generictext.Configuration;
using IO.Scanbot.Sdk.UI.View.Generictext.Entity;
using IO.Scanbot.Sdk.UI.View.Hic.Configuration;
using IO.Scanbot.Sdk.UI.View.Hic;
using IO.Scanbot.Sdk.UI.View.Mrz.Configuration;
using IO.Scanbot.Sdk.UI.View.Mrz;
using IO.Scanbot.Sdk.UI.View.Vin.Configuration;

using ScanbotSDK.Xamarin.Android;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReadyToUseUIDemo.Droid
{
    [Activity(Label = "Ready-to-use UI Demo", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : AppCompatActivity
    {
        readonly List<FragmentButton> buttons = new List<FragmentButton>();

        ProgressBar progress;
        IO.Scanbot.Sdk.ScanbotSDK sdkInstance;

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

            sdkInstance = new IO.Scanbot.Sdk.ScanbotSDK(this);

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
                var intent = BatchBarcodeScannerActivity.NewIntent(this, configuration);
                StartActivityForResult(intent, Constants.QR_BARCODE_DEFAULT_UI_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.ScannerImportBarcode)
            {
                StartImportActivity(Constants.IMPORT_BARCODE_REQUEST);
            }
            else if (button.Data.Code == ListItemCode.ScannerMRZ)
            {
                var configuration = new MRZScannerConfiguration();
                configuration.SetSuccessBeepEnabled(false);

                var intent = MRZScannerActivity.NewIntent(this, configuration);
                StartActivityForResult(intent, Constants.MRZ_DEFAULT_UI_REQUEST_CODE);
            }
            else if (button.Data.Code == ListItemCode.ScannerEHIC)
            {
                var config = new HealthInsuranceCardScannerConfiguration();
                config.SetTopBarButtonsColor(Color.White);

                var intent = HealthInsuranceCardScannerActivity.NewIntent(this, config);
                StartActivityForResult(intent, Constants.REQUEST_EHIC_SCAN);
            }
            else if (button.Data.Code == ListItemCode.GenericDocumentRecognizer)
            {
                var config = new GenericDocumentRecognizerConfiguration();
                config.SetAcceptedDocumentTypes(new List<RootDocumentType>
                {
                    RootDocumentType.DeIdCardFront,
                    RootDocumentType.DeIdCardBack,
                });
                var intent = GenericDocumentRecognizerActivity.NewIntent(this, config);
                StartActivityForResult(intent, Constants.GENERIC_DOCUMENT_RECOGNIZER_REQUEST);
            }
            else if (button.Data.Code == ListItemCode.CheckRecognizer)
            {
                var config = new CheckRecognizerConfiguration();
                config.SetAcceptedCheckStandards(new List<IO.Scanbot.Check.Entity.RootDocumentType>
                {
                    IO.Scanbot.Check.Entity.RootDocumentType.AUSCheck,
                    IO.Scanbot.Check.Entity.RootDocumentType.FRACheck,
                    IO.Scanbot.Check.Entity.RootDocumentType.INDCheck,
                    IO.Scanbot.Check.Entity.RootDocumentType.KWTCheck,
                    IO.Scanbot.Check.Entity.RootDocumentType.USACheck,
                    IO.Scanbot.Check.Entity.RootDocumentType.ISRCheck,
                });
                var intent = CheckRecognizerActivity.NewIntent(this, config);
                StartActivityForResult(intent, Constants.CHECK_RECOGNIZER_REQUEST);
            }
            else if (button.Data.Code == ListItemCode.TextDataRecognizer)
            {
                // Launch the TextDataScanner UI
                var step = new TextDataScannerStep(
                     stepTag: "tag",
                     title: string.Empty,
                     guidanceText: string.Empty,
                     pattern: string.Empty,
                     shouldMatchSubstring: true,
                     validationCallback: new ValidationCallback(),
                     cleanRecognitionResultCallback: new RecognitionCallback(),
                     preferredZoom: 1.6f,
                     aspectRatio: new IO.Scanbot.Sdk.AspectRatio(4.0, 1.0),
                     unzoomedFinderHeight: 40f,
                     allowedSymbols: new List<Java.Lang.Character>(),
                     significantShakeDelay: 0);

                var config = new TextDataScannerConfiguration(step);
                var intent = TextDataScannerActivity.NewIntent(this, config);
                StartActivityForResult(intent, Constants.TEXT_DATA_RECOGNIZER_REQUEST);
            }
            else if (button.Data.Code == ListItemCode.VinRecognizer)
            {
                // Launch the VinScannerConfigurations
                var configuration = new VinScannerConfiguration();
                configuration.SetGuidanceText("Please place the finder over the VIN.");
                configuration.SetFinderAspectRatio(new IO.Scanbot.Sdk.AspectRatio(7, 1));
                configuration.SetFlashEnabled(true);
                var intent = IO.Scanbot.Sdk.UI.View.Vin.VinScannerActivity.NewIntent(this, configuration);
                StartActivityForResult(intent, Constants.VIN_RECOGNIZER_REQUEST);
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

            if (requestCode == Constants.CAMERA_DEFAULT_UI_REQUEST_CODE)
            {
                var parcelable = data.GetParcelableArrayExtra(RtuConstants.ExtraKeyRtuResult);
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
                    var page = new Page(pageId, new List<PointF>(), DetectionStatus.Ok, ImageFilterType.None);
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
                    var detector = new IO.Scanbot.Sdk.ScanbotSDK(this).CreateBarcodeDetector();
                    var result = detector.DetectFromBitmap(bitmap, 0);
                    var fragment = BarcodeDialogFragment.CreateInstance(result);

                    // Estimate quality of the imported barcode image
                    // Estimating quality on already cropped barcodes should
                    // normally yield the best results, as there is little empty space
                    var qualityAnalyzer = new IO.Scanbot.Sdk.ScanbotSDK(this).CreateDocumentQualityAnalyzer();
                    fragment.Quality = qualityAnalyzer.AnalyzeInBitmap(bitmap, 0);
                    fragment.Show(SupportFragmentManager, BarcodeDialogFragment.NAME);
                });
            }
            else if (requestCode == Constants.QR_BARCODE_DEFAULT_UI_REQUEST_CODE)
            {
                var result = (BarcodeScanningResult)data.GetParcelableExtra(RtuConstants.ExtraKeyRtuResult);
                var fragment = BarcodeDialogFragment.CreateInstance(result);
                fragment.Show(SupportFragmentManager, BarcodeDialogFragment.NAME);
            }
            else if (requestCode == Constants.CROP_DEFAULT_UI_REQUEST)
            {
                var page = data.GetParcelableExtra(RtuConstants.ExtraKeyRtuResult) as Page;
                PageRepository.Add(page);
            }

            else if (requestCode == Constants.REQUEST_EHIC_SCAN)
            {
                var result = (HealthInsuranceCardRecognitionResult)data.GetParcelableExtra(
                    RtuConstants.ExtraKeyRtuResult);

                var fragment = HealthInsuranceCardFragment.CreateInstance(result);
                fragment.Show(SupportFragmentManager, HealthInsuranceCardFragment.NAME);
            }
            else if (requestCode == Constants.MRZ_DEFAULT_UI_REQUEST_CODE)
            {
                var result = (MRZGenericDocument)data.GetParcelableExtra(RtuConstants.ExtraKeyRtuResult);
                var fragment = MRZDialogFragment.CreateInstance(result);
                fragment.Show(SupportFragmentManager, MRZDialogFragment.NAME);
            }
            else if (requestCode == Constants.GENERIC_DOCUMENT_RECOGNIZER_REQUEST)
            {
                var resultsArray = data.GetParcelableArrayListExtra(RtuConstants.ExtraKeyRtuResult);
                if (resultsArray.Count == 0)
                {
                    return;
                }

                var resultWrapper = (ResultWrapper)resultsArray[0];
                var resultRepository = sdkInstance.ResultRepositoryForClass(resultWrapper.Clazz);
                var genericDocument = (GenericDocument)resultRepository.GetResultAndErase(resultWrapper.ResultId);
                var fields = genericDocument.Fields.Cast<Field>().ToList();
                var description = string.Join(";\n", fields
                    .Where(field => field != null)
                    .Select((field) =>
                    {
                        string outStr = "";
                        if (field.GetType() != null && field.GetType().Name != null)
                        {
                            outStr += field.GetType().Name + " = ";
                        }
                        if (field.Value != null && field.Value.Text != null)
                        {
                            outStr += field.Value.Text;
                        }
                        return outStr;
                    })
                    .ToList()
                );
                Console.WriteLine("GDR Result: ", description);
                ShowAlert("Result", description);
            }
            else if (requestCode == Constants.CHECK_RECOGNIZER_REQUEST)
            {
                var resultWrapper = (ResultWrapper)data.GetParcelableExtra(RtuConstants.ExtraKeyRtuResult);
                var resultRepository = sdkInstance.ResultRepositoryForClass(resultWrapper.Clazz);
                var checkResult = (CheckRecognizerResult)resultRepository.GetResultAndErase(resultWrapper.ResultId);
                var fields = checkResult.Check.Fields;
                var description = string.Join(";\n", fields
                    .Where(field => field != null)
                    .Select((field) =>
                    {
                        string outStr = "";
                        if (field.GetType() != null && field.GetType().Name != null)
                        {
                            outStr += field.GetType().Name + " = ";
                        }
                        if (field.Value != null && field.Value.Text != null)
                        {
                            outStr += field.Value.Text;
                        }
                        return outStr;
                    })
                    .ToList());
                Console.WriteLine("Check Recognizer Result: ", description);
                ShowAlert("Result", description);
            }
            else if (requestCode == Constants.TEXT_DATA_RECOGNIZER_REQUEST)
            {
                var result = data.GetParcelableArrayExtra(RtuConstants.ExtraKeyRtuResult);
                if (result == null || result.Count() == 0)
                {
                    return;
                }
                var textDataScannerStepResult = result.First() as TextDataScannerStepResult;
                Console.WriteLine("Text Recognizer Result: " + textDataScannerStepResult.Text);
                RunOnUiThread(delegate
                {
                    ShowAlert("Result", textDataScannerStepResult.Text);
                });
            }
            else if (requestCode == Constants.VIN_RECOGNIZER_REQUEST)
            {
                var result = (IO.Scanbot.Sdk.Vin.VinScanResult)data.GetParcelableExtra(RtuConstants.ExtraKeyRtuResult);
                if (result == null || string.IsNullOrEmpty(result.RawText))
                {
                    return;
                }
                Console.WriteLine("VIN Recognizer Result: " + result.RawText);
                RunOnUiThread(delegate
                {
                    ShowAlert("Result", result.RawText);
                });
            }
        }

        private void ShowAlert(string title, string message)
        {
            var dialog = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            AndroidX.AppCompat.App.AlertDialog alert = dialog.Create();
            alert.SetTitle(title);
            alert.SetMessage(message);
            alert.SetButton((int)DialogButtonType.Neutral, "OK", (c, ev) =>
            {
                alert.Dismiss();
            });
            alert.Show();
        }
    }
}


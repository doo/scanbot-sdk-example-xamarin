using System;
using System.Threading.Tasks;

using UIKit;
using MobileCoreServices;
using Foundation;

using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.iOS;

using ScanbotSDK.iOS;
using System.Linq;
using ClassicalComponentsDemo.iOS.ViewControllers;
using System.Collections.Generic;

namespace ClassicalComponentsDemo.iOS
{
    public partial class MainSelectionTableViewController : UITableViewController
    {
        class CameraDemoDelegateHandler : CameraDemoDelegate
        {
            public MainSelectionTableViewController parentController;

            public override void DidCaptureDocumentImage(UIImage documentImage)
            {
                if (parentController != null)
                {
                    parentController.documentImage = documentImage;
                    AppDelegate.TempImageStorage.AddImage(documentImage);
                    parentController.ShowImageView(documentImage);
                }
            }

            public override void DidCaptureOriginalImage(UIImage originalImage)
            {
                if (parentController != null)
                {
                    AppDelegate.TempImageStorage.AddImage(originalImage);
                    parentController.originalImage = originalImage;
                }
            }
        }

        class CroppingDemoDelegateHandler : CroppingDemoDelegate
        {
            public MainSelectionTableViewController parentController;

            public override void CropViewControllerDidFinish(UIImage croppedImage)
            {
                // Obtain cropped image from cropping view controller
                if (parentController != null)
                {
                    AppDelegate.TempImageStorage.AddImage(croppedImage);
                    parentController.documentImage = croppedImage;
                    parentController.ShowImageView(croppedImage);
                }
            }
        }

        class GenericDocumentRecognizerDelegate : SBSDKUIGenericDocumentRecognizerViewControllerDelegate
        {
            public WeakReference<MainSelectionTableViewController> rootVc;
            public GenericDocumentRecognizerDelegate(MainSelectionTableViewController rootVc)
            {
                this.rootVc = new WeakReference<MainSelectionTableViewController>(rootVc);
            }

            public override void DidFinishWithDocuments(SBSDKUIGenericDocumentRecognizerViewController viewController, SBSDKGenericDocument[] documents)
            {
                if (documents == null || documents.Length == 0)
                {
                    return;
                }

                // We only take the first document for simplicity
                var firstDocument = documents.First();
                var fields = firstDocument.Fields
                    .Where((f) => f != null && f.Type != null && f.Type.Name != null && f.Value != null && f.Value.Text != null)
                    .Select((f) => string.Format("{0}: {1}", f.Type.Name, f.Value.Text))
                    .ToList();
                var description = string.Join("\n", fields);

                rootVc.TryGetTarget(out MainSelectionTableViewController vc);
                if (vc != null)
                {
                    vc.ShowResultMessage(description);
                }
            }
        }

        CameraDemoDelegateHandler cameraHandler = new CameraDemoDelegateHandler();
        CroppingDemoDelegateHandler croppingHandler = new CroppingDemoDelegateHandler();

        GenericDocumentRecognizerDelegate gdrDelegate;

        UIImagePickerController imagePicker;

        UIImage documentImage, originalImage;

        public MainSelectionTableViewController(IntPtr handle) : base(handle)
        {
            gdrDelegate = new GenericDocumentRecognizerDelegate(this);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            copyrightLabel.Text = "Copyright (c) " + DateTime.Now.Year.ToString() + " doo GmbH. All rights reserved.";
        }

        partial void ApplyImageFilterTouchUpInside(UIButton sender)
        {
            if (!CheckScanbotSDKLicense()) { return; }
            if (!CheckDocumentImageUrl()) { return; }

            UIAlertController actionSheetAlert = UIAlertController.Create("Select filter type", "", UIAlertControllerStyle.ActionSheet);

            foreach (ImageFilter filter in Enum.GetValues(typeof(ImageFilter)))
            {
                if (filter.ToString().ToLower() == "none") { continue; }
                actionSheetAlert.AddAction(UIAlertAction.Create(filter.ToString(), UIAlertActionStyle.Default, (action) =>
                {
                    ApplyFilterOnDocumentImage(filter);
                }));
            }

            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            PresentViewController(actionSheetAlert, true, null);
        }

        protected void ApplyFilterOnDocumentImage(ImageFilter filter)
        {
            Task.Run(() =>
            {
                // The SDK call is sync!
                var resultImage = SBSDK.ApplyImageFilter(documentImage, filter);
                DebugLog("Image filter result: " + resultImage);
                ShowImageView(resultImage);
            });
        }

        partial void PerformOCRUpInside(UIButton sender)
        {
            if (!CheckScanbotSDKLicense()) { return; }
            if (!CheckDocumentImageUrl()) { return; }

            Task.Run(async () =>
            {

                DebugLog("Performing OCR ...");

                var images = AppDelegate.TempImageStorage.ImageURLs;

                // Uncomment below code to use the old OCR approach. Use [OCRMode.Legacy] and set the required [InstalledLanguages] property.
                //var languages = new List<string> { "en", "de" };
                //var ocrConfig = new OcrConfigs
                //{
                //    InstalledLanguages = languages,
                //    OcrMode = OCRMode.Legacy,
                //    LanguageDataPath = SBSDK.GetOcrConfigs().LanguageDataPath
                //};

                var result = await SBSDK.PerformOCR(images, SBSDK.GetOcrConfigs());
                DebugLog("OCR result: " + result.RecognizedText);
                ShowMessage("OCR Text", result.RecognizedText);
            });
        }

        partial void DocumentDetectionTouchUpInside(UIButton sender)
        {
            if (!CheckScanbotSDKLicense()) { return; }

            // Select image from PhotoLibrary and run document detection

            imagePicker = new UIImagePickerController
            {
                SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
                MediaTypes = new string[] { UTType.Image }
            };
            imagePicker.FinishedPickingMedia += Handle_FinishedPickingMedia;
            imagePicker.Canceled += delegate
            {
                imagePicker.DismissModalViewController(true);
            };

            //Display the imagePicker controller:
            this.PresentModalViewController(imagePicker, true);
        }

        partial void CroppingUITouchUpInside(UIButton sender)
        {
            if (!CheckScanbotSDKLicense()) { return; }
            if (!CheckOriginalImageUrl()) { return; }

            var image = originalImage;
            var cropViewController = new CroppingDemoNavigationController(image);
            cropViewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            cropViewController.NavigationBar.BarStyle = UIBarStyle.Black;
            cropViewController.NavigationBar.TintColor = UIColor.White;
            croppingHandler.parentController = this;
            cropViewController.croppingDelegate = this.croppingHandler;
            PresentViewController(cropViewController, true, null);
        }

        partial void CameraUITouchUpInside(UIButton sender)
        {
            if (!CheckScanbotSDKLicense()) { return; }

            var cameraViewController = new CameraDemoViewController();
            cameraHandler.parentController = this;
            cameraViewController.cameraDelegate = this.cameraHandler;
            NavigationController.PushViewController(cameraViewController, true);
        }

        partial void GenericDocumentRecognizerTouchUpInside(UIButton sender)
        {
            if (!CheckScanbotSDKLicense()) { return; }

            var configuration = SBSDKUIGenericDocumentRecognizerConfiguration.DefaultConfiguration;
            configuration.TextConfiguration.CancelButtonTitle = "Done";
            configuration.BehaviorConfiguration.DocumentType = SBSDKUIDocumentType.IdCardFrontBackDE;

            gdrDelegate.rootVc.SetTarget(this);
            var scanner = SBSDKUIGenericDocumentRecognizerViewController.CreateNewWithConfiguration(configuration, gdrDelegate);
            NavigationController.PushViewController(scanner, true);
        }

        partial void CheckRecognizerTouchUpInside(UIButton sender)
        {
            if (!CheckScanbotSDKLicense()) { return; }
            var vc = new CheckRecognizerDemoViewController();
            NavigationController.PushViewController(vc, true);
        }

        bool CheckDocumentImageUrl()
        {
            if (documentImage == null)
            {
                ShowErrorMessage("Please snap a document image via Scanning UI or run Document Detection on an image file from the PhotoLibrary");
                return false;
            }
            return true;
        }

        bool CheckOriginalImageUrl()
        {
            if (originalImage == null)
            {
                ShowErrorMessage("Please snap a document image via Scanning UI or run Document Detection on an image file from the PhotoLibrary");
                return false;
            }
            return true;
        }

        bool CheckSelectedImages()
        {
            if (AppDelegate.TempImageStorage.ImageCount == 0)
            {
                ShowErrorMessage("Please select at least one image from Gallery or via Camera UI");
                return false;
            }
            return true;
        }

        bool CheckScanbotSDKLicense()
        {
            if (SBSDK.IsLicenseValid())
            {
                // Trial period, valid trial license or valid production license.
                return true;
            }

            ShowErrorMessage("Scanbot SDK (trial) license has expired!");
            return false;
        }

        public void ShowMessage(string title, string message, UIViewController controller = null)
        {
            UIViewController presenter = controller != null ? controller : this;

            InvokeOnMainThread(() =>
            {
                var alertController = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                presenter.PresentViewController(alertController, true, null);
            });
        }

        public void ShowResultMessage(string message)
        {
            ShowMessage("Operation result", message);
        }

        public void ShowErrorMessage(string message)
        {
            ShowMessage("Error", message);
        }

        void ShowImageView(UIImage hiresImage)
        {
            var previewImage = CommonUtils.MaxResizeImage(hiresImage, 900, 900);
            InvokeOnMainThread(() =>
            {
                selectedImageView.Image = previewImage;
                selectImageLabel.Hidden = true;
            });
        }

        protected void Handle_FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs e)
        {
            NSUrl referenceURL = e.Info[UIImagePickerController.ReferenceUrl] as NSUrl;
            if (referenceURL != null)
            {
                //NSUrl imgUrl = e.Info[UIImagePickerController.ReferenceUrl] as NSUrl;
                // Get the original image
                UIImage originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;
                if (originalImage != null)
                {
                    DebugLog("Got the original image from gallery");
                    AppDelegate.TempImageStorage.AddImage(originalImage);
                    this.originalImage = originalImage;
                    ShowImageView(originalImage);
                    RunDocumentDetection(originalImage);
                }
            }

            // Dismiss the picker
            imagePicker.DismissModalViewController(true);
        }

        void RunDocumentDetection(UIImage image)
        {
            Task.Run(() =>
            {
                // The SDK call is sync!
                var detectionResult = SBSDK.DetectDocument(image);
                if (detectionResult.Status.IsOk())
                {
                    var imageResult = detectionResult.Image as UIImage;
                    DebugLog("Detection result image: " + imageResult);
                    AppDelegate.TempImageStorage.AddImage(imageResult);
                    documentImage = imageResult;

                    ShowImageView(imageResult);

                    DebugLog("Detection result polygon: ");
                    string resultString = "";
                    foreach (var p in detectionResult.Polygon)
                    {
                        resultString += p + "\n";
                    }
                    DebugLog(resultString);
                }
                else
                {
                    DebugLog("No Document detected! DetectionStatus = " + detectionResult.Status);
                    ShowErrorMessage("No Document detected! DetectionStatus = " + detectionResult.Status);
                }
            });
        }

        partial void CreateTiffFileTouchUpInside(UIButton sender)
        {
            if (!CheckScanbotSDKLicense()) { return; }
            if (!CheckDocumentImageUrl()) { return; }

            Task.Run(() =>
            {
                DebugLog("Creating TIFF file ...");
                var images = AppDelegate.TempImageStorage.ImageURLs;
                var tiffOutputFileUrl = GenerateRandomFileUrlInDemoTempStorage(".tiff");
                SBSDK.WriteTiff(images, tiffOutputFileUrl, new TiffOptions { OneBitEncoded = true });
                DebugLog("TIFF file created: " + tiffOutputFileUrl);
                ShowMessage("TIFF file created", "" + tiffOutputFileUrl);
            });
        }

        partial void CreatePdfTouchUpInside(UIButton sender)
        {
            if (!CheckScanbotSDKLicense()) { return; }
            if (!CheckDocumentImageUrl()) { return; }

            Task.Run(() =>
            {
                DebugLog("Creating PDF file ...");
                var images = AppDelegate.TempImageStorage.ImageURLs;
                var pdfOutputFileUrl = GenerateRandomFileUrlInDemoTempStorage(".pdf");
                SBSDK.CreatePDF(images, pdfOutputFileUrl, PDFPageSize.A4);
                DebugLog("PDF file created: " + pdfOutputFileUrl);
                ShowMessage("PDF file created", "" + pdfOutputFileUrl);
            });
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);
        }

        void DebugLog(string msg)
        {
            Console.WriteLine("Scanbot SDK Example: " + msg);
        }


        NSUrl GenerateRandomFileUrlInDemoTempStorage(string fileExtension)
        {
            var targetFile = System.IO.Path.Combine(
                AppDelegate.Directory, new NSUuid().AsString().ToLower() + fileExtension);
            return NSUrl.FromFilename(targetFile);
        }
    }
}
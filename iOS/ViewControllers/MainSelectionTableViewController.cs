using System;
using System.Threading.Tasks;

using UIKit;
using MobileCoreServices;
using Foundation;

using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.iOS;

namespace scanbotsdkexamplexamarin.iOS
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
                    parentController.documentImageUrl = AppDelegate.TempImageStorage.AddImage(documentImage);
                    parentController.ShowImageView(documentImage, true);
                }
            }

            public override void DidCaptureOriginalImage(UIImage originalImage)
            {
                if (parentController != null)
                {
                    parentController.originalImageUrl = AppDelegate.TempImageStorage.AddImage(originalImage);
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
                    parentController.documentImageUrl = AppDelegate.TempImageStorage.AddImage(croppedImage);
                    parentController.ShowImageView(croppedImage, true);
                }
            }
        }

        CameraDemoDelegateHandler cameraHandler = new CameraDemoDelegateHandler();
        CroppingDemoDelegateHandler croppingHandler = new CroppingDemoDelegateHandler();

        UIImagePickerController imagePicker;

        NSUrl documentImageUrl, originalImageUrl;


        public MainSelectionTableViewController(IntPtr handle) : base(handle) { }

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
            DebugLog("Applying image filter on " + documentImageUrl);
            Task.Run(() =>
            {
                // The SDK call is sync!
                var resultImage = SBSDK.ApplyImageFilter(documentImageUrl, filter);
                DebugLog("Image filter result: " + resultImage);
                ShowImageView(resultImage, true);
            });
        }

        partial void PerformOCRUpInside(UIButton sender)
        {
            if (!CheckScanbotSDKLicense()) { return; }
            if (!CheckDocumentImageUrl()) { return; }

            Task.Run(() =>
            {
                DebugLog("Performing OCR ...");
                var images = new NSUrl[] { documentImageUrl };
                var result = SBSDK.PerformOCR(images, new[] { "en", "de" });
                DebugLog("OCR result: " + result.RecognizedText);
                ShowMessage("OCR Text", result.RecognizedText);
            });
        }

        partial void DocumentDetectionTouchUpInside(UIButton sender)
        {
            if (!CheckScanbotSDKLicense()) { return; }

            // Select image from PhotoLibrary and run document detection

            imagePicker = new UIImagePickerController();
            imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            imagePicker.MediaTypes = new string[] { UTType.Image };
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

            var image = ImageUtils.LoadImage(originalImageUrl);
            var cropViewController = new CroppingDemoNavigationController(image);
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

        bool CheckDocumentImageUrl()
        {
            if (documentImageUrl == null)
            {
                ShowErrorMessage("Please snap a document image via Scanning UI or run Document Detection on an image file from the PhotoLibrary");
                return false;
            }
            return true;
        }

        bool CheckOriginalImageUrl()
        {
            if (originalImageUrl == null)
            {
                ShowErrorMessage("Please snap a document image via Scanning UI or run Document Detection on an image file from the PhotoLibrary");
                return false;
            }
            return true;
        }

        bool CheckSelectedImages()
        {
            if (AppDelegate.TempImageStorage.Count() == 0)
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

        void ShowMessage(string title, string message)
        {
            InvokeOnMainThread(() =>
            {
                var alertController = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            });
        }

        void ShowResultMessage(string message)
        {
            ShowMessage("Operation result", message);
        }

        void ShowErrorMessage(string message)
        {
            ShowMessage("Error", message);
        }

        void ShowImageView(UIImage originalImage, bool resizeThumbnail)
        {
            UIImage imageToShow;
            if (resizeThumbnail)
            {
                imageToShow = ExampleImageUtils.MaxResizeImage(originalImage, 900, 900);
            }
            else
            {
                imageToShow = originalImage;
            }

            InvokeOnMainThread(() =>
            {
                selectedImageView.Image = imageToShow;
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
                    originalImageUrl = AppDelegate.TempImageStorage.AddImage(originalImage);
                    RunDocumentDetection(originalImageUrl);
                }
            }

            // Dismiss the picker
            imagePicker.DismissModalViewController(true);
        }

        void RunDocumentDetection(NSUrl imgurl)
        {
            DebugLog("Performing document detection on image " + imgurl);
            Task.Run(() =>
            {
                // The SDK call is sync!
                var detectionResult = SBSDK.DetectDocument(imgurl);
                if (detectionResult.Status.IsOk())
                {
                    var imageResult = detectionResult.Image as UIImage;
                    DebugLog("Detection result image: " + imageResult);
                    documentImageUrl = AppDelegate.TempImageStorage.AddImage(imageResult);

                    ShowImageView(imageResult, true);

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
                var images = new NSUrl[] { documentImageUrl }; // add more images for multipage TIFF
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
                var images = new NSUrl[] { documentImageUrl }; // add more images for multipage PDF
                var pdfOutputFileUrl = GenerateRandomFileUrlInDemoTempStorage(".pdf");
                SBSDK.CreatePDF(images, pdfOutputFileUrl, PDFPageSize.FixedA4);
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
                AppDelegate.TempImageStorage.TempDir, new NSUuid().AsString().ToLower() + fileExtension);
            return NSUrl.FromFilename(targetFile);
        }
    }
}
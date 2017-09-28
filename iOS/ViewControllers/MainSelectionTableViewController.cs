using System;
using System.Threading.Tasks;

using UIKit;
using MobileCoreServices;
using Foundation;

using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.iOS.Wrapper;

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
                    parentController.documentImageUrl = parentController.tempStorage.AddImage(documentImage);
                    parentController.ShowImageView(documentImage, true);
                }
            }

            public override void DidCaptureOriginalImage(UIImage originalImage)
            {
                if (parentController != null)
                {
                    parentController.originalImageUrl = parentController.tempStorage.AddImage(originalImage);
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
                    parentController.documentImageUrl = parentController.tempStorage.AddImage(croppedImage);
                    parentController.ShowImageView(croppedImage, true);
                }
            }
        }

        CameraDemoDelegateHandler cameraHandler = new CameraDemoDelegateHandler();
        CroppingDemoDelegateHandler croppingHandler = new CroppingDemoDelegateHandler();

        UIImagePickerController imagePicker;

        TempImageStorage tempStorage = new TempImageStorage();
        NSUrl documentImageUrl, originalImageUrl;


        public MainSelectionTableViewController(IntPtr handle) : base(handle) { }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            copyrightLabel.Text = "Copyright (c) " + DateTime.Now.Year.ToString() + " doo GmbH. All rights reserved.";
        }

        partial void ApplyImageFilterTouchUpInside(UIButton sender)
        {
            if (!CheckDocumentImageUrl()) { return; }

            UIAlertController actionSheetAlert = UIAlertController.Create("Select filter type", "", UIAlertControllerStyle.ActionSheet);
            actionSheetAlert.AddAction(UIAlertAction.Create("Binarized", UIAlertActionStyle.Default, (action) =>
            {
                ApplyFilterOnDocumentImage(ImageFilter.Binarized);
            }));

            actionSheetAlert.AddAction(UIAlertAction.Create("Grayscale", UIAlertActionStyle.Default, (action) =>
            {
                ApplyFilterOnDocumentImage(ImageFilter.Grayscale);
            }));

            actionSheetAlert.AddAction(UIAlertAction.Create("Color enhanced", UIAlertActionStyle.Default, (action) =>
            {
                ApplyFilterOnDocumentImage(ImageFilter.ColorEnhanced);
            }));

            actionSheetAlert.AddAction(UIAlertAction.Create("Color document", UIAlertActionStyle.Default, (action) =>
            {
                ApplyFilterOnDocumentImage(ImageFilter.ColorDocument);
            }));

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

        partial void DocumentDetectionTouchUpInside(UIButton sender)
        {
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
            if (tempStorage.Count() == 0)
            {
                ShowErrorMessage("Please select at least one image from Gallery or via Camera UI");
                return false;
            }
            return true;
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
                    originalImageUrl = tempStorage.AddImage(originalImage);
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
                var detectionResult = SBSDK.DocumentDetection(imgurl);
                if (detectionResult.Status.IsOk())
                {
                    var imageResult = detectionResult.Image as UIImage;
                    DebugLog("Detection result image: " + imageResult);
                    documentImageUrl = tempStorage.AddImage(imageResult);

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

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);
        }

        void DebugLog(string msg)
        {
            Console.WriteLine("Scanbot SDK Example: " + msg);
        }

    }
}
using System;
using System.Threading.Tasks;

using UIKit;
using MobileCoreServices;
using Foundation;

using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.iOS;

using ScanbotSDK.iOS;

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
                    parentController.documentImageUrl = AppDelegate.TempImageStorage.AddImage(documentImage);
                    parentController.ShowImageView(documentImage);
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
                    parentController.ShowImageView(croppedImage);
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
                ShowImageView(resultImage);
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

        void ShowMessage(string title, string message, UIViewController controller = null)
        {
            UIViewController presenter = controller != null ? controller : this;

            InvokeOnMainThread(() =>
            {
                var alertController = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                presenter.PresentViewController(alertController, true, null);
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

        void ShowImageView(UIImage hiresImage)
        {
            var previewImage = ExampleImageUtils.MaxResizeImage(hiresImage, 900, 900);
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
                    originalImageUrl = AppDelegate.TempImageStorage.AddImage(originalImage);
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
                    documentImageUrl = AppDelegate.TempImageStorage.AddImage(imageResult);

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

        partial void WorkflowScannerTouchUpInside(UIButton sender)
        {
            this.ShowWorkflowSelector();
        }

        void ShowWorkflowSelector()
        {
            UIAlertController actionSheetAlert = UIAlertController.Create("Select a workflow", "", UIAlertControllerStyle.ActionSheet);

            foreach (var workflow in WorkflowFactory.AllWorkflows())
            {
                UIAlertAction action = UIAlertAction.Create(workflow.Name, UIAlertActionStyle.Default, (actn) =>
                {
                    this.ShowWorkflow(workflow);
                });

                actionSheetAlert.AddAction(action);
            }

            this.PresentViewController(actionSheetAlert, true, null);
        }

        void ShowWorkflow(SBSDKUIWorkflow workflow)
        {
            SBSDKUIWorkflowScannerConfiguration config = SBSDKUIWorkflowScannerConfiguration.DefaultConfiguration;
            SBSDKUIWorkflowScannerViewController controller = SBSDKUIWorkflowScannerViewController.CreateNewWithWorkflow(workflow, config, null);
            controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            controller.WeakDelegate = this;
            this.PresentViewController(controller, false, null);
        }

        [Export("workflowScanViewController:didFinishWorkflow:withResults:")]
        void WorkflowScanViewController(SBSDKUIWorkflowScannerViewController viewController, SBSDKUIWorkflow workflow, SBSDKUIWorkflowStepResult[] results)
        {
            this.ShowWorkflowResults(results, viewController);
        }

        void ShowWorkflowResults(SBSDKUIWorkflowStepResult[] results, UIViewController viewController)
        {
            WorkflowResultsViewController controller = WorkflowResultsViewController.InstantiateWith(results);
            viewController.PresentViewController(controller, true, null);
        }

        // @optional -(void)workflowScanViewController:(SBSDKUIWorkflowScannerViewController * _Nonnull)viewController didFailStepValidation:(SBSDKUIWorkflowStep * _Nonnull)step withResult:(SBSDKUIWorkflowStepResult * _Nonnull)result;
        [Export("workflowScanViewController:didFailStepValidation:withResult:")]
        void WorkflowScanViewControllerDidFail(SBSDKUIWorkflowScannerViewController viewController, SBSDKUIWorkflowStep step, SBSDKUIWorkflowStepResult result)
        {
            this.ShowMessage("Step validation failed", result.ValidationError.LocalizedDescription, viewController);
        }

        // @optional -(void)workflowScanViewController:(SBSDKUIWorkflowScannerViewController * _Nonnull)viewController didFailWorkflowValidation:(SBSDKUIWorkflow * _Nonnull)workflow withResults:(NSArray<SBSDKUIWorkflowStepResult *> * _Nonnull)results validationError:(NSError * _Nonnull)error;
        [Export("workflowScanViewController:didFailWorkflowValidation:withResults:validationError:")]
        void WorkflowScanViewController(SBSDKUIWorkflowScannerViewController viewController, SBSDKUIWorkflow workflow, SBSDKUIWorkflowStepResult[] results, NSError error)
        {
            this.ShowMessage("Workflow validation failed", error.LocalizedDescription, viewController);
        }

        // @required -(SBSDKUIWorkflowStep * _Nullable)nextStepAfterFinishingStep:(SBSDKUIWorkflowStep * _Nonnull)step withResults:(NSArray<SBSDKUIWorkflowStepResult *> * _Nonnull)results;
        [Export("nextStepAfterFinishingStep:withResults:")]
        SBSDKUIWorkflowStep WithResults(SBSDKUIWorkflowStep step, SBSDKUIWorkflowStepResult[] results)
        {
            if (results.Length >= 2)
            {
                return null;
            }

            if (results.Length >= 1 && results[0].CapturedPage != null)
            {
                return WorkflowFactory.QrCodeStep();
            }
            else
            {
                return WorkflowFactory.DocumentStep();
            }
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
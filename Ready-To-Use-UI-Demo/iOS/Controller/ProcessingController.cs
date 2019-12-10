using System;
using Foundation;
using ReadyToUseUIDemo.iOS.Repository;
using ReadyToUseUIDemo.iOS.Utils;
using ReadyToUseUIDemo.iOS.View;
using ReadyToUseUIDemo.model;
using ScanbotSDK.iOS;
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.iOS;
using UIKit;
using PdfKit;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class ProcessingController : UIViewController
    {
        public ProcessingView ContentView { get; private set; }

        CroppingFinishedHandler handler;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ContentView = new ProcessingView();
            View = ContentView;

            Title = "Process Image";
            
            handler = new CroppingFinishedHandler();

            var saveButton = new UIBarButtonItem(Texts.save, UIBarButtonItemStyle.Done, OnSaveButtonClick);
            NavigationItem.SetRightBarButtonItem(saveButton, false);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ContentView.ProcessingBar.CropAndRotateButton.Click += CropAndRotate;

            ContentView.ProcessingBar.FilterButton.Click += OpenFilterScreen;

            ContentView.ProcessingBar.DeleteButton.Click += DeleteImage;

            handler.Finished += CroppingFinished;

            ContentView.ImageView.Image = PageRepository.Current.DocumentImage;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ContentView.ProcessingBar.CropAndRotateButton.Click -= CropAndRotate;

            ContentView.ProcessingBar.FilterButton.Click -= OpenFilterScreen;

            ContentView.ProcessingBar.DeleteButton.Click -= DeleteImage;

            handler.Finished -= CroppingFinished;
        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            var input = new NSUrl[] { PageRepository.Current.DocumentImageURL };

            var docs = NSSearchPathDirectory.DocumentDirectory;
            var nsurl = NSFileManager.DefaultManager.GetUrls(docs, NSSearchPathDomain.User)[0];
            
            var controller = UIAlertController.Create(Texts.save, Texts.SaveHow, UIAlertControllerStyle.ActionSheet);

            var title = "Oops!";
            var body = "Something went wrong with saving your file. Please try again";

            if (!SBSDK.IsLicenseValid())
            {
                title = "Oops";
                body = "Your license has expired";
                Alert.Show(this, title, body);
                return;
            }

            var pdf = CreateButton(Texts.Pdf, delegate
            {
                var output = new NSUrl(nsurl.AbsoluteString + Guid.NewGuid() + ".pdf");
                SBSDK.CreatePDF(input, output, PDFPageSize.Auto);
                OpenDocument(output);
            });

            var ocr = CreateButton(Texts.PdfWithOCR, delegate
            {
                var output = new NSUrl(nsurl.AbsoluteString + Guid.NewGuid() + ".pdf");
                var languages = SBSDK.GetOcrConfigs().InstalledLanguages;
                try
                {
                    SBSDK.PerformOCR(input, languages.ToArray(), output);
                    OpenDocument(output);
                }
                catch (Exception ex)
                {
                    body = ex.Message;
                    Alert.Show(this, title, body);
                }
            });

            var tiff = CreateButton(Texts.Tiff, delegate
            {
                var output = new NSUrl(nsurl.AbsoluteString + Guid.NewGuid() + ".tiff");
                var options = new TiffOptions { OneBitEncoded = true };

                bool success = SBSDK.WriteTiff(input, output,  options);

                if (success)
                {
                    title = "Great Success!";
                    body = "Saved your tiff to: " + output.Path;
                }

                Alert.Show(this, title, body);
            });

            var cancel = CreateButton("Cancel", delegate { }, UIAlertActionStyle.Cancel);
            
            controller.AddAction(pdf);
            controller.AddAction(ocr);
            controller.AddAction(tiff);

            controller.AddAction(cancel);
            
            UIPopoverPresentationController presentationPopover = controller.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            PresentViewController(controller, true, null);
        }

        void OpenDocument(NSUrl uri)
        {
            var controller = new PdfViewController(uri);
            NavigationController.PushViewController(controller, true);
        }

        UIAlertAction CreateButton(string text, Action<UIAlertAction> action, UIAlertActionStyle style = UIAlertActionStyle.Default)
        {
            return UIAlertAction.Create(text, style, action);
        }

        private void CropAndRotate(object sender, EventArgs e)
        {
            var config = SBSDKUICroppingScreenConfiguration.DefaultConfiguration;
            var controller = SBSDKUICroppingViewController.CreateNewWithPage(PageRepository.Current, config, handler);
            PresentViewController(controller, false, null);
        }

        private void CroppingFinished(object sender, CroppingFinishedArgs e)
        {
            PageRepository.Current = e.Page;
            ContentView.ImageView.Image = PageRepository.Current.DocumentImage;
        }

        private void OpenFilterScreen(object sender, EventArgs e)
        {
            var controller = new FilterController();
            NavigationController.PushViewController(controller, true);
        }

        private void DeleteImage(object sender, EventArgs e)
        {
            PageRepository.Remove(PageRepository.Current);
            NavigationController.PopViewController(true);
        }

    }

    public class CroppingFinishedArgs : EventArgs
    {
        public SBSDKUIPage Page { get; set; }
    }

    public class CroppingFinishedHandler : SBSDKUICroppingViewControllerDelegate
    {
        public EventHandler<CroppingFinishedArgs> Finished;

        public override void DidFinish(SBSDKUICroppingViewController viewController, SBSDKUIPage changedPage)
        {
            Finished?.Invoke(this, new CroppingFinishedArgs { Page = changedPage });
        }
    }

}

using System;
using Foundation;
using ReadyToUseUIDemo.iOS.Repository;
using ReadyToUseUIDemo.iOS.Utils;
using ReadyToUseUIDemo.iOS.View;
using ReadyToUseUIDemo.iOS.View.Collection;
using ReadyToUseUIDemo.model;
using ScanbotSDK.iOS;
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class ImageListController : UIViewController
    {
        public ImageCollectionView ContentView { get; private set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ContentView = new ImageCollectionView();
            View = ContentView;

            Title = "Scanned documents";

            LoadPages();

            var saveButton = new UIBarButtonItem(Texts.save, UIBarButtonItemStyle.Done, OnSaveButtonClick);
            NavigationItem.SetRightBarButtonItem(saveButton, false);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            LoadPages();

            ContentView.Collection.Selected += OnImageSelected;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ContentView.Collection.Selected -= OnImageSelected;
        }

        void LoadPages()
        {
            ContentView.Collection.Pages.Clear();
            ContentView.Collection.Pages.AddRange(PageRepository.Items);
            ContentView.Collection.ReloadData();
        }

        void OnImageSelected(object sender, CollectionEventArgs e)
        {
            PageRepository.Current = e.Page;

            var controller = new ProcessingController();
            NavigationController.PushViewController(controller, true);
        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            var input = PageRepository.DocumentImageURLs;

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

            var pdf = CreateButton(Texts.save_without_ocr, delegate
            {
                var output = new NSUrl(nsurl.AbsoluteString + Guid.NewGuid() + ".pdf");
                SBSDK.CreatePDF(input, output, PDFPageSize.FixedA4);
                OpenDocument(output, false);
            });

            var ocr = CreateButton(Texts.save_with_ocr, delegate
            {
                var output = new NSUrl(nsurl.AbsoluteString + Guid.NewGuid() + ".pdf");
                var languages = SBSDK.GetOcrConfigs().InstalledLanguages;
                try
                {
                    SBSDK.PerformOCR(input, languages.ToArray(), output);
                    OpenDocument(output, true);
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

                // Please note that some compression types are only compatible for 1-bit encoded images (binarized black & white images)!
                var options = new TiffOptions { OneBitEncoded = true, Compression = TiffCompressionOptions.CompressionCcittfax4, Dpi = 250 };

                bool success = SBSDK.WriteTiff(input, output, options);

                if (success)
                {
                    title = "Info";
                    body = "TIFF file saved to: " + output.Path;
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

        void OpenDocument(NSUrl uri, bool ocr)
        {
            var controller = new PdfViewController(uri, ocr);
            NavigationController.PushViewController(controller, true);
        }

        UIAlertAction CreateButton(string text, Action<UIAlertAction> action,
            UIAlertActionStyle style = UIAlertActionStyle.Default)
        {
            return UIAlertAction.Create(text, style, action);
        }

    }
}

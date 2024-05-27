using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using ImageIO;
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
            var controller = UIAlertController.Create(Texts.save, Texts.SaveHow, UIAlertControllerStyle.ActionSheet);

            if (!SBSDK.IsLicenseValid())
            {
                Alert.Show(this, "Oops", "Your license has expired");
                return;
            }

            controller.AddAction(CreateButton(Texts.save_without_ocr, (action) => GeneratePdfAsync(input)));
            controller.AddAction(CreateButton(Texts.perform_ocr, (action) => PerformOcrAsync(input)));
            controller.AddAction(CreateButton(Texts.save_with_ocr, (action) => GenerateSandwichPdfAsync(input)));
            controller.AddAction(CreateButton(Texts.Tiff, (action) => GenerateTiffAsync(input)));
            controller.AddAction(CreateButton("Cancel", delegate { }, UIAlertActionStyle.Cancel));

            UIPopoverPresentationController presentationPopover = controller.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }
            PresentViewController(controller, true, null);
        }

        private async void GeneratePdfAsync(NSUrl[] inputUrls)
        {
            var outputUrl = await SBSDK.CreatePDF(inputUrls,
                new PDFConfiguration
                {
                    PageOrientation = PDFPageOrientation.Auto,
                    PageSize = PDFPageSize.A4,
                    PdfAttributes = new PDFAttributes
                    {
                        Author = "Scanbot User",
                        Creator = "ScanbotSDK",
                        Title = "ScanbotSDK PDF",
                        Subject = "Generating a sandwiched PDF",
                        Keywords = new[] { "x-platform", "ios", "android" },
                    }
                });
            OpenDocument(outputUrl, false);
        }

        private async void GenerateSandwichPdfAsync(NSUrl[] inputUrls)
        {
            // NOTE:
            // The default OCR engine is 'OcrConfig.ScanbotOCR' which is ML based. This mode doesn't expect the Langauges array.
            // If you wish to use the previous engine please use 'OcrConfig.Tesseract(...)'. The Languages array is mandatory in this mode.
            // Uncomment the below code to use the past legacy 'OcrConfig.Tesseract(...)' engine mode.
            // var ocrConfig = OcrConfig.Tesseract(withLanguageString: new List<string>{ "en", "de" });

            // You may also use the default InstalledLanguages property in the OCR configuration.
            // SBSDK.GetOcrConfigs() returns all the default OCR configurations from the SDK.
            // var languages = SBSDK.GetOcrConfigs().InstalledLanguages;

            // Using the default OCR option
            var ocrConfig = OcrConfig.ScanbotOCR;

            try
            {
                var outputUrl = await SBSDK.CreateSandwichPDF(inputUrls,
                    new PDFConfiguration
                    {
                        PageOrientation = PDFPageOrientation.Auto,
                        PageSize = PDFPageSize.A4,
                        PdfAttributes = new PDFAttributes
                        {
                            Author = "Scanbot User",
                            Creator = "ScanbotSDK",
                            Title = "ScanbotSDK PDF",
                            Subject = "Generating a sandwiched PDF",
                            Keywords = new[] { "x-platform", "ios", "android" },
                        }
                    }, ocrConfig);
                OpenDocument(outputUrl, true);
            }
            catch (Exception ex)
            {
                Alert.Show(this, "Error", ex.Message);
            }
        }

        private async void PerformOcrAsync(NSUrl[] inputUrls)
        {
            // NOTE:
            // The default OCR engine is 'OcrConfig.ScanbotOCR' which is ML based. This mode doesn't expect the Langauges array.
            // If you wish to use the previous engine please use 'OcrConfig.Tesseract(...)'. The Languages array is mandatory in this mode.
            // Uncomment the below code to use the past legacy 'OcrConfig.Tesseract(...)' engine mode.
            // var ocrConfig = OcrConfig.Tesseract(withLanguageString: new List<string>{ "en", "de" });

            // Using the default OCR option
            var ocrConfig = OcrConfig.ScanbotOCR;

            var ocrResult = await SBSDK.PerformOCR(inputUrls, ocrConfig);

            // You can access the results with: result.Pages
            Alert.Show(this, "OCR", ocrResult.RecognizedText);
        }

        private void GenerateTiffAsync(NSUrl[] inputUrls)
        {
            try
            {
                var docs = NSSearchPathDirectory.DocumentDirectory;
                var nsurl = NSFileManager.DefaultManager.GetUrls(docs, NSSearchPathDomain.User)[0];
                var outputUrl = new NSUrl(nsurl.AbsoluteString + Guid.NewGuid() + ".tiff");
                // Please note that some compression types are only compatible for 1-bit encoded images (binarized black & white images)!
                var options = new TiffOptions { OneBitEncoded = true, Compression = TiffCompressionOptions.CompressionCcittfax4, Dpi = 250 };
                var success = SBSDK.WriteTiff(inputUrls, outputUrl, options);
                if (success)
                {
                    Alert.Show(this, "Info", "TIFF file saved to: " + outputUrl);
                }
            }
            catch (Exception ex)
            {
                Alert.Show(this, "Error", ex.Message);
            }
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

        private void ShowUnexpectedError()
        {
            var title = "Oops!";
            var body = "Something went wrong with saving your file. Please try again";
            Alert.Show(this, title, body);
        }
    }
}

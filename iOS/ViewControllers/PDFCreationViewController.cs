using Foundation;
using System;
using System.IO;
using UIKit;
using MobileCoreServices;

using ScanbotSDK.Xamarin.iOS.Wrapper;
using System.Threading.Tasks;

namespace scanbotsdkexamplexamarin.iOS
{
    public partial class PDFCreationViewController : UIViewController
    {
        UIImagePickerController imagePicker;
        NSUrl pdfOutputUrl;

        class CollectionViewSource : UICollectionViewSource
        {
            public TempImageStorage tempStorage;

            public override nint NumberOfSections(UICollectionView collectionView)
            {
                return 1;
            }

            public override nint GetItemsCount(UICollectionView collectionView, nint section)
            {
                return tempStorage.Count();
            }

            public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
            {
                ThumbnailCollectionViewCell cell = (ThumbnailCollectionViewCell)collectionView.DequeueReusableCell("thumbCell", indexPath);
                NSUrl[] images = tempStorage.GetImages();
                NSUrl imageUrl = images[indexPath.Row];
                NSData imageData = NSData.FromUrl(imageUrl);
                cell.ShowThumbnail(UIImage.LoadFromData(imageData));
                return cell;
            }
        }

        CollectionViewSource collectionSource;


        public PDFCreationViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            collectionSource = new CollectionViewSource();
            if (collectionSource.tempStorage == null)
            {
                collectionSource.tempStorage = new TempImageStorage();
            }

            createPDFButton.TouchUpInside += delegate
            {
                if (collectionSource.tempStorage.Count() == 0)
                {
                    var alertController = UIAlertController.Create("Info", "Please add some images from the PhotoLibrary.", UIAlertControllerStyle.Alert);
                    alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                    PresentViewController(alertController, true, null);
                    return;
                }

                ProgressHUD progressHUD = ProgressHUD.Load();

                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var demoPath = Path.Combine(documents, "scanbot-sdk-example-xamarin");
                Directory.CreateDirectory(demoPath);
                var targetFile = Path.Combine(demoPath, new NSUuid().AsString().ToLower() + ".pdf");
                pdfOutputUrl = NSUrl.FromFilename(targetFile);

                progressHUD.Show();
                Task.Run(() =>
                {
                    // The SDK call is sync!
                    SBSDK.CreatePDF(collectionSource.tempStorage.GetImages(), pdfOutputUrl);
                    InvokeOnMainThread(() =>
                    {
                        progressHUD.Hide();
                        PerformSegue("showGeneratedPDF", this);
                    });
                });
            };

            addImageButton.TouchUpInside += delegate
            {
                imagePicker = new UIImagePickerController();
                imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
                imagePicker.MediaTypes = new string[] { UTType.Image };
                imagePicker.FinishedPickingMedia += Handle_FinishedPickingMedia;
                imagePicker.Canceled += Handle_ImagePickerCanceled;
                PresentModalViewController(imagePicker, true);
            };
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            collectionView.Source = collectionSource;
        }

        void Handle_ImagePickerCanceled(object sender, EventArgs e)
        {
            imagePicker.DismissModalViewController(true);
        }

        void Handle_FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs e)
        {
            UIImage originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;
            if (originalImage != null)
            {
                collectionSource.tempStorage.AddImage(originalImage);
            }
            addImageButton.SetTitle("Add Image (" + collectionSource.tempStorage.Count() + ")", UIControlState.Normal);
            collectionView.ReloadData();

            // dismiss the picker
            imagePicker.DismissModalViewController(true);
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            if (segue.Identifier == "showGeneratedPDF")
            {
                var resultVC = (PDFViewController)segue.DestinationViewController;
                if (resultVC != null)
                {
                    resultVC.ShowPDF(pdfOutputUrl);
                }
            }
        }

    }
}
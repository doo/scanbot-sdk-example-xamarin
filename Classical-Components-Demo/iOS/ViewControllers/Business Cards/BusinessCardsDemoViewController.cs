using UIKit; using Foundation; using ScanbotSDK.iOS; using System;  namespace ClassicalComponentsDemo.iOS {     public class BusinessCard : NSObject     {         public UIImage Image;         public string RecognizedText;          public BusinessCard(UIImage image, string recognizedText)         {             this.Image = image;             this.RecognizedText = recognizedText;         }     }       public partial class BusinessCardsDemoViewController : UIViewController, ISBSDKMultipleObjectScannerViewControllerDelegate, IUICollectionViewDelegate, IUICollectionViewDataSource     {          private SBSDKMultipleObjectScannerViewController _scannerController;         private SBSDKBusinessCardsImageProcessor _imageProcessor;         private BusinessCard _selectedCard;         private NSArray<SBSDKOCRResult> _ocrResults;         private ISBSDKImageStoring _storage = new SBSDKIndexedImageStorage();         private bool _isProcessing;          public BusinessCardsDemoViewController(IntPtr handle) : base(handle)         {         }          public override void ViewDidLoad()         {             base.ViewDidLoad();             _imageProcessor = new SBSDKBusinessCardsImageProcessor();              _imageProcessor.PerformOCR = true;             _imageProcessor.PerformAutoRotation = true;

            _scannerController = new SBSDKMultipleObjectScannerViewController(this, null);             _scannerController.WeakDelegate = this;             collectionView.Delegate = this;         }          public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)         {             if (segue.DestinationViewController is BusinessCardDetailDemoViewController)             {                 BusinessCardDetailDemoViewController destinationViewController = (BusinessCardDetailDemoViewController)segue.DestinationViewController;                 destinationViewController.BusinessCard = _selectedCard;             }         }          private void ProcessDetectedDocumentsInStorage(ISBSDKImageStoring storage)         {             ShowLoadingView();             _isProcessing = true;             _imageProcessor.ProcessImageStorage(storage, "en+de", (processedStorage, results) =>
            {
                _storage = processedStorage;                 _ocrResults = results;                 collectionView.ReloadData();                 HideLoadingView();                  if (processedStorage.ImageCount > 0)
                {
                    OpenCollectionContainer();
                }                  _isProcessing = false;
            });         }

        private void ShowLoadingView()         {             loadingView.Alpha = (nfloat)0.0;             loadingView.Hidden = false;             UIView.Animate(0.2, () =>
            {
                loadingView.Alpha = (nfloat)1.0;
            });         }
         private void HideLoadingView()
        {
            UIView.Animate(0.2, () =>             {                 loadingView.Alpha = (nfloat)0.0;             }, () =>             {                 loadingView.Hidden = true;             });
        }

         private void OpenCollectionContainer()
        {
            collectionContainerHeightConstraint.Constant = 450;             UIView.Animate(0.2, () =>             {                 View.LayoutIfNeeded();             });
        }

        private void CloseCollectionContainer()         {             collectionContainerHeightConstraint.Constant = 0;             UIView.Animate(0.2, () =>             {                 View.LayoutIfNeeded();             });         }
         private bool IsPanelOpened()
        {
            return collectionContainerHeightConstraint.Constant > 0;
        }          // IUICollectionViewDataSource

        public UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)         {             BusinessCardDemoCell cell = (BusinessCardDemoCell)collectionView.DequeueReusableCell("BusinessCardCell", indexPath);             cell.Image = _storage.ImageAtIndex((nuint)indexPath.Row);             return cell;         }          public nint GetItemsCount(UICollectionView collectionView, nint section)         {             return (nint)_storage.ImageCount;         }

        // IUICollectionViewDelegate

        [Export("collectionView:shouldSelectItemAtIndexPath:")]
        public bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)         {             UIImage image = _storage.ImageAtIndex((nuint)indexPath.Row);             string text = _ocrResults?.Count > 0 ? _ocrResults[indexPath.Row].RecognizedText : "";             _selectedCard = new BusinessCard(image, text);             return true;         }          [Export("collectionView:didSelectItemAtIndexPath:")]         public void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)         {             collectionView.DeselectItem(indexPath, true);         }

        [Export("scannerController:didCaptureObjectImagesInStorage:")]         public void ScannerController(SBSDKMultipleObjectScannerViewController controller, ISBSDKImageStoring imageStorage)
        {
            ProcessDetectedDocumentsInStorage(imageStorage);
        }

        [Export("scannerControllerWillCaptureImage:controller")]         public void ScannerControllerWillCaptureImage(SBSDKMultipleObjectScannerViewController controller)
        {
            _storage.RemoveAllImages();
        }          [Export("scannerControllerShouldAnalyseVideoFrame:controller")]         public bool ScannerControllerShouldAnalyseVideoFrame(SBSDKMultipleObjectScannerViewController controller)
        {             bool shouldAnalyse = true;             InvokeOnMainThread(() =>
           {
               shouldAnalyse = IsPanelOpened() == false && _isProcessing == false;
           });
            return shouldAnalyse;
        }

        partial void closeCollectionContainerTapped(UIButton sender)
        {
            CloseCollectionContainer();
        }     }  }   
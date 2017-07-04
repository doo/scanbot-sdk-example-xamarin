// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace scanbotsdkexamplexamarin.iOS
{
    [Register ("PDFCreationViewController")]
    partial class PDFCreationViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton addImageButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UICollectionView collectionView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton createPDFButton { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (addImageButton != null) {
                addImageButton.Dispose ();
                addImageButton = null;
            }

            if (collectionView != null) {
                collectionView.Dispose ();
                collectionView = null;
            }

            if (createPDFButton != null) {
                createPDFButton.Dispose ();
                createPDFButton = null;
            }
        }
    }
}
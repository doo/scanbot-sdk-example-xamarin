// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace scanbotsdkexamplexamarin.iOS
{
    [Register ("WorkflowResultsViewController")]
    partial class WorkflowResultsViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton closeButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UICollectionView collectionView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView textView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton toPasteboardButton { get; set; }


        [Action ("closeButtonTapped:")]
        partial void closeButtonTapped (UIKit.UIButton sender);


        [Action ("toPasteboardButtonTapped:")]
        partial void toPasteboardButtonTapped (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (closeButton != null) {
                closeButton.Dispose ();
                closeButton = null;
            }

            if (collectionView != null) {
                collectionView.Dispose ();
                collectionView = null;
            }

            if (textView != null) {
                textView.Dispose ();
                textView = null;
            }

            if (toPasteboardButton != null) {
                toPasteboardButton.Dispose ();
                toPasteboardButton = null;
            }
        }
    }
}
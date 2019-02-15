// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace scanbotsdkexamplexamarin.iOS
{
    [Register ("BusinessCardsDemoViewController")]
    partial class BusinessCardsDemoViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton closeCollectionContainerButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint collectionContainerHeightConstraint { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UICollectionView collectionView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView loadingView { get; set; }

        [Action ("closeCollectionContainerTapped:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void CloseCollectionContainerTapped (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (closeCollectionContainerButton != null) {
                closeCollectionContainerButton.Dispose ();
                closeCollectionContainerButton = null;
            }

            if (collectionContainerHeightConstraint != null) {
                collectionContainerHeightConstraint.Dispose ();
                collectionContainerHeightConstraint = null;
            }

            if (collectionView != null) {
                collectionView.Dispose ();
                collectionView = null;
            }

            if (loadingView != null) {
                loadingView.Dispose ();
                loadingView = null;
            }
        }
    }
}
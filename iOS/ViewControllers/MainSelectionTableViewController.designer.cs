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
    [Register ("MainSelectionTableViewController")]
    partial class MainSelectionTableViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton applyImageFilterButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel copyrightLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton createTiffFileButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel demoLicenseLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton documentDetectionButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton goToPdfViewButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton scanbotCameraButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton scanbotCroppingButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView selectedImageView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel selectImageLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView tableView { get; set; }

        [Action ("ApplyImageFilterTouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ApplyImageFilterTouchUpInside (UIKit.UIButton sender);

        [Action ("CameraUITouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void CameraUITouchUpInside (UIKit.UIButton sender);

        [Action ("CreatePdfTouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void CreatePdfTouchUpInside (UIKit.UIButton sender);

        [Action ("CreateTiffFileTouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void CreateTiffFileTouchUpInside (UIKit.UIButton sender);

        [Action ("CroppingUITouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void CroppingUITouchUpInside (UIKit.UIButton sender);

        [Action ("DocumentDetectionTouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void DocumentDetectionTouchUpInside (UIKit.UIButton sender);

        [Action ("PerformOCRUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void PerformOCRUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (applyImageFilterButton != null) {
                applyImageFilterButton.Dispose ();
                applyImageFilterButton = null;
            }

            if (copyrightLabel != null) {
                copyrightLabel.Dispose ();
                copyrightLabel = null;
            }

            if (createTiffFileButton != null) {
                createTiffFileButton.Dispose ();
                createTiffFileButton = null;
            }

            if (demoLicenseLabel != null) {
                demoLicenseLabel.Dispose ();
                demoLicenseLabel = null;
            }

            if (documentDetectionButton != null) {
                documentDetectionButton.Dispose ();
                documentDetectionButton = null;
            }

            if (goToPdfViewButton != null) {
                goToPdfViewButton.Dispose ();
                goToPdfViewButton = null;
            }

            if (scanbotCameraButton != null) {
                scanbotCameraButton.Dispose ();
                scanbotCameraButton = null;
            }

            if (scanbotCroppingButton != null) {
                scanbotCroppingButton.Dispose ();
                scanbotCroppingButton = null;
            }

            if (selectedImageView != null) {
                selectedImageView.Dispose ();
                selectedImageView = null;
            }

            if (selectImageLabel != null) {
                selectImageLabel.Dispose ();
                selectImageLabel = null;
            }

            if (tableView != null) {
                tableView.Dispose ();
                tableView = null;
            }
        }
    }
}
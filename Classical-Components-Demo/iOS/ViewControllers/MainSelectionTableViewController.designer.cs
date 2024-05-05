// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace ClassicalComponentsDemo.iOS
{
	[Register ("MainSelectionTableViewController")]
	partial class MainSelectionTableViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton applyImageFilterButton { get; set; }

		[Outlet]
		UIKit.UIButton checkRecognizerButton { get; set; }

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
		UIKit.UIButton genericDocumentRecognizerButton { get; set; }

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
		partial void ApplyImageFilterTouchUpInside (UIKit.UIButton sender);

		[Action ("CameraUITouchUpInside:")]
		partial void CameraUITouchUpInside (UIKit.UIButton sender);

		[Action ("CheckRecognizerTouchUpInside:")]
		partial void CheckRecognizerTouchUpInside (UIKit.UIButton sender);

		[Action ("CreatePdfTouchUpInside:")]
		partial void CreatePdfTouchUpInside (UIKit.UIButton sender);

		[Action ("CreateSandwichPdfTouchUpInside:")]
		partial void CreateSandwichPdfTouchUpInside (UIKit.UIButton sender);

		[Action ("CreateTiffFileTouchUpInside:")]
		partial void CreateTiffFileTouchUpInside (UIKit.UIButton sender);

		[Action ("CroppingUITouchUpInside:")]
		partial void CroppingUITouchUpInside (UIKit.UIButton sender);

		[Action ("DocumentDetectionTouchUpInside:")]
		partial void DocumentDetectionTouchUpInside (UIKit.UIButton sender);

		[Action ("GenericDocumentRecognizerTouchUpInside:")]
		partial void GenericDocumentRecognizerTouchUpInside (UIKit.UIButton sender);

		[Action ("PerformOCRUpInside:")]
		partial void PerformOCRUpInside (UIKit.UIButton sender);

		[Action ("WorkflowScannerTouchUpInside:")]
		partial void WorkflowScannerTouchUpInside (UIKit.UIButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (applyImageFilterButton != null) {
				applyImageFilterButton.Dispose ();
				applyImageFilterButton = null;
			}

			if (checkRecognizerButton != null) {
				checkRecognizerButton.Dispose ();
				checkRecognizerButton = null;
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

			if (genericDocumentRecognizerButton != null) {
				genericDocumentRecognizerButton.Dispose ();
				genericDocumentRecognizerButton = null;
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

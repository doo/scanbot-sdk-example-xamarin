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

namespace ClassicalComponentsDemo.iOS
{
    [Register ("ProgressHUD")]
    partial class ProgressHUD
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIActivityIndicatorView loadingIndicator { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (loadingIndicator != null) {
                loadingIndicator.Dispose ();
                loadingIndicator = null;
            }
        }
    }
}
using System;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Service
{
    public class ImagePicker
    {
        public static readonly ImagePicker Instance = new ImagePicker();

        UIImagePickerController controller;
        public UIImagePickerController Controller
        {
            get
            {
                if (controller == null)
                {
                    controller = new UIImagePickerController
                    {
                        SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
                        MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary)
                    };
                }
                
                return controller;
            }
        }

        public void Present(UIViewController controller)
        {
            controller.PresentModalViewController(Controller, true);
        }

        public void Dismiss()
        {
            Controller.DismissModalViewController(false);
        }
    }
}

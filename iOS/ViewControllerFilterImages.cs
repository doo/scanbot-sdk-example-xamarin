using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using System.Threading.Tasks;

using ScanbotSDK.Xamarin.iOS.Wrapper;
using ScanbotSDK.Xamarin;
using Scanbot.Utils.iOS;

namespace scanbot_sdk_xamarin_demo.iOS
{
	public partial class ViewControllerFilterImages : UIViewController
	{
		UIImagePickerController imagePicker;
		UIButton selectImageButton;

		TempImageStorage tempStorage;

		//Dictionary<int, UIImageView> imageViewMap = new Dictionary<int, UIImageView>();
		List<UIImageView> imageViewList = new List<UIImageView>();


		public ViewControllerFilterImages(IntPtr handle) : base(handle)
		{
			// This constructor should not contain any initialization logic!
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			if (this.tempStorage == null)
			{
				this.tempStorage = new TempImageStorage();
			}

			selectImageButton = new UIButton(UIButtonType.System);
			selectImageButton.SetTitle("Add Image (0)", UIControlState.Normal);
			selectImageButton.TouchUpInside += (sender, e) => { 
				imagePicker = new UIImagePickerController();
				imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
				imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);
				imagePicker.FinishedPickingMedia += Handle_FinishedPickingMedia;
				imagePicker.Canceled += delegate { imagePicker.DismissModalViewController(true); };
				//Display the imagePicker controller:
				this.PresentModalViewController(imagePicker, true);
			};
			//selectImageButton.WidthAnchor.ConstraintEqualTo(200).Active = true;
			//selectImageButton.HeightAnchor.ConstraintEqualTo(50).Active = true;


			var applyFilterButton = new UIButton(UIButtonType.System);
			applyFilterButton.SetTitle("Apply Filter to ALL Images", UIControlState.Normal);
			applyFilterButton.TouchUpInside += (sender, e) => {
				//var tasks = new List<Task>();
				var idx = 0;
				foreach (var url in tempStorage.GetImages())
				{
					var resultImg = SBSDK.ApplyImageFilter(url, ImageFilter.Binarized);
					var thumbImg = MyImageUtils.MaxResizeImage(resultImg, 200, 200);
					var imgView = imageViewList[idx];
            		InvokeOnMainThread(() => {
						// Run on UI main thread:
						imgView.Image = thumbImg;
					});
					idx++;

				}

				// alternative via tasks-list and Task.WhenAll(..)
				//await Task.WhenAll(tasks);
			};

			//applyFilterButton.WidthAnchor.ConstraintEqualTo(200).Active = true;
			//applyFilterButton.HeightAnchor.ConstraintEqualTo(50).Active = true;

			StackView.Spacing = 10;
			//StackView.Distribution = UIStackViewDistribution.EqualSpacing;;
			//StackView.TranslatesAutoresizingMaskIntoConstraints = false;

			StackView.AddArrangedSubview(selectImageButton);
			StackView.AddArrangedSubview(applyFilterButton);
		}



		protected void Handle_FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs e)
		{
			// determine what was selected, video or image
			bool isImage = false;
			switch (e.Info[UIImagePickerController.MediaType].ToString())
			{
				case "public.image":
					Console.WriteLine("Image selected");
					isImage = true;
					break;
				case "public.video":
					Console.WriteLine("Video selected");
					break;
			}

			if (isImage)
			{
				// get the original image
				UIImage originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;
				if (originalImage != null)
				{
					Console.WriteLine("got the original image");
					this.tempStorage.AddImage(originalImage);

					var thumbImg = MyImageUtils.MaxResizeImage(originalImage, 200, 200);
					var imgView = new UIImageView(thumbImg);
					imgView.WidthAnchor.ConstraintEqualTo(100).Active = true;
					imgView.HeightAnchor.ConstraintEqualTo(100).Active = true;
					imageViewList.Add(imgView);
					StackView.AddArrangedSubview(imgView);
				}

				selectImageButton.SetTitle("Add Image (" + this.tempStorage.GetImages().Length + ")", UIControlState.Normal);
			}
			else {
				// it's a video
			}

			// dismiss the picker
			imagePicker.DismissModalViewController(true);
		}


	}

}
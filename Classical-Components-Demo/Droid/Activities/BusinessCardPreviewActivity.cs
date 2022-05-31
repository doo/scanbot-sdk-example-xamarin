
using System.IO;

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using IO.Scanbot.Sdk.Businesscard;
using IO.Scanbot.Sdk.Persistence;

namespace ClassicalComponentsDemo.Droid.Activities
{
    [Activity(Label = "Business Card Preview")]
    public class BusinessCardPreviewActivity : BaseActivity
    {
        public static BusinessCardsImageProcessorBusinessCardProcessingResult SelectedItem { get; internal set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.BusinessCardPreview);

            var textView = FindViewById<TextView>(Resource.Id.businessCardText);
            textView.Text = SelectedItem.OcrResult.RecognizedText;

            var imageView = FindViewById<ImageView>(Resource.Id.businessCardImage);
            var type = PageFileStorage.PageFileType.UnfilteredDocument;
            var documentPath = BusinessCardsPreviewActivity.GetPath(this, SelectedItem.Page.PageId, type);

            type = PageFileStorage.PageFileType.Original;
            var originalImagePath = BusinessCardsPreviewActivity.GetPath(this, SelectedItem.Page.PageId, type);

            var sdk = new IO.Scanbot.Sdk.ScanbotSDK(this);
            Android.Net.Uri imageUri;
            if (File.Exists(documentPath.Path))
            {
                imageUri = Android.Net.Uri.Parse(documentPath.Path);
            }
            else
            {
                imageUri = originalImagePath;
            }

            var bitmap = sdk.FileIOProcessor().ReadImage(imageUri, new BitmapFactory.Options());
            imageView.SetImageBitmap(bitmap);
        }
    }
}

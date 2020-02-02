
using System.IO;

using Android.App;
using Android.OS;
using Android.Support.V7.App;
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

            if (File.Exists(documentPath.Path))
            {
                imageView.SetImageURI(documentPath);
            }
            else
            {
                imageView.SetImageURI(originalImagePath);
            }
        }
    }
}

using System;
using Android.App;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using IO.Scanbot.Genericdocument.Entity;
using System.Linq;
using Android.Graphics;
using Android.Content;
using IO.Scanbot.Sdk.Check.Entity;

namespace ClassicalComponentsDemo.Droid.Activities
{
    [Activity(Theme = "@style/Theme.AppCompat")]
    public class CheckRecognizerResultActivity: AppCompatActivity
    {
        private const string EXTRA_CHECK_DOCUMENT = "CheckDocument";
        private ImageView checkResultImageView;

        // TODO: handle image more carefully in production code
        public static Bitmap tempDocumentImage = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ActivityCheckRecognizerResult);

            var fieldsLayout = FindViewById<LinearLayout>(Resource.Id.check_result_fields_layout);
            checkResultImageView = FindViewById<ImageView>(Resource.Id.check_result_image);

            if (Intent.GetParcelableExtra(EXTRA_CHECK_DOCUMENT) is GenericDocument document)
            {
                AddValueView(fieldsLayout, "Type", document.GetType().Name);
                foreach (var field in document.Fields) {
                    if (field.Value is not OcrResult ocrResult) { continue; }
                    AddValueView(fieldsLayout, field.GetType().Name, ocrResult.Text);
                }
            }

            if (tempDocumentImage is Bitmap bitmap) {
                checkResultImageView.Visibility = Android.Views.ViewStates.Visible;
                checkResultImageView.SetImageBitmap(bitmap);
            }

            FindViewById<Button>(Resource.Id.retry).Click += delegate
            {
                tempDocumentImage = null;
                Finish();
            };
        }

        private void AddValueView(LinearLayout layout, string title, string value)
        {
            var v = LayoutInflater.Inflate(Resource.Layout.ViewKeyValue, layout, false);
            v.FindViewById<TextView>(Resource.Id.view_text_key).Text = title;
            v.FindViewById<TextView>(Resource.Id.view_text_value).Text = value;
            layout.AddView(v);
        }

        public static Intent NewIntent(Context context, CheckRecognizerResult result) {
            var intent = new Intent(context, typeof(CheckRecognizerResultActivity));
            intent.PutExtra(EXTRA_CHECK_DOCUMENT, result.Check);
            return intent;
        }
    }
}


using System;
using System.Text;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using IO.Scanbot.Sdk.UI.View.Nfc;
using IO.Scanbot.Sdk.UI.View.Nfc.Entity;
using ReadyToUseUIDemo.Droid.Views;

namespace ReadyToUseUIDemo.Droid.Fragments
{
    public class NfcDialogFragment : BaseDialogFragment
    {
        public const string NFC_DATA = "NFC_DATA_EXTRA";
        public const string NAME = "NFCDialogFragment";

        public static NfcDialogFragment CreateInstance(NfcPassportScanningResult data)
        {
            var fragment = new NfcDialogFragment();
            var args = new Bundle();
            args.PutParcelable(NFC_DATA, data);
            fragment.Arguments = args;

            return fragment;
        }

        NfcPassportScanningResult result;

        public override View AddContentView(LayoutInflater inflater, ViewGroup container)
        {
            result = (NfcPassportScanningResult)Arguments.GetParcelable(NFC_DATA);
            var view = new NfcAlertView(Context, result);
            container.AddView(view);
            return view;
        }


        public class PassportCallback: Java.Lang.Object, IPassportPhotoSaveCallback
        {
            public static Bitmap Photo;
            public void OnImageRetrieved(Bitmap photo)
            {
                Photo = photo;
            }
        }

        public class NfcAlertView : RelativeLayout
        {
            public ImageView Image { get; private set; }

            public TextView Text { get; private set; }

            public NfcAlertView(Context context, NfcPassportScanningResult result) : base(context)
            {
                Image = new ImageView(context);
                Image.SetImageBitmap(PassportCallback.Photo);
                AddView(Image);

                Text = new TextView(context);
                AddView(Text);

                var text = "Document: " + result.Dg1Group.DocumentNumber + "\n";
                text += "Expires: " + result.Dg1Group.DateOfExpiry + "\n";
                /**
                 * TODO: Add whatever you seem necessary to 
                 * 
                 */
                Text.Text = text;
            }

            protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
            {
                base.OnLayout(changed, left, top, right, bottom);

                var parentWidth = right;
                var parentHeight = bottom;
                var padding = 20;

                var width = parentWidth / 2;
                var height = width;
                var x = parentWidth / 2 - width / 2;
                var y = padding;

                Image.LayoutParameters = GetParameters(x, y, width, height);

                y += height + padding;

                x = padding;
                
                width = parentWidth - 2 * padding;
                height = LayoutParams.WrapContent;

                Text.LayoutParameters = GetParameters(x, y, width, height);
            }

            RelativeLayout.LayoutParams GetParameters(int x, int y, int w, int h)
            {
                var parameters = new RelativeLayout.LayoutParams(w, h);
                parameters.LeftMargin = x;
                parameters.TopMargin = y;
                return parameters;
            }
        }
    }
}

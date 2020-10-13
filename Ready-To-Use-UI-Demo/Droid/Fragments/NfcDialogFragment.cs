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

        public class NfcAlertView : ConstraintLayout
        {
            public NfcAlertView(Context context, NfcPassportScanningResult result) : base(context)
            {
                var image = new ImageView(context);
                image.SetImageBitmap(PassportCallback.Photo);
                AddView(image);

                /*
                 * TODO: Proper styling & display additional data for for popup
                 */
                var content = new TextView(context);
                content.LayoutParameters = new ConstraintLayout.LayoutParams(300, 300);
                AddView(content);

                var text = "Expires: " + result.Dg1Group.CheckDigitExpiryDate + "\n";
                content.Text = text;
            }
        }
    }
}

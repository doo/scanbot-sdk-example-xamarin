using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using IO.Scanbot.Sdk.Barcode.Entity;
using ReadyToUseUIDemo.Droid.Views;

namespace ReadyToUseUIDemo.Droid.Fragments
{
    public class BarcodeDialogFragment : BaseDialogFragment
    {
        public const string NAME = "BarcodeDialogFragment";

        public static BarcodeDialogFragment CreateInstance(BarcodeScanningResult data)
        {
            var fragment = new BarcodeDialogFragment();
            var args = new Bundle();
            args.PutParcelable(NAME, data);
            fragment.Arguments = args;
            return fragment;
        }

        public override View AddContentView(LayoutInflater inflater, ViewGroup container)
        {
            var data = (BarcodeScanningResult)Arguments.GetParcelable(NAME);
            var view = inflater.Inflate(Resource.Layout.fragment_barcode_dialog, container);

            var format = view.FindViewById<TextView>(Resource.Id.qr_barcode_format);
            var content = view.FindViewById<TextView>(Resource.Id.qr_barcode_value);

            format.Text = data.BarcodeFormat.Name();
            content.Text = data.DescribeContents().ToString();

            return view;
        }
    }
}

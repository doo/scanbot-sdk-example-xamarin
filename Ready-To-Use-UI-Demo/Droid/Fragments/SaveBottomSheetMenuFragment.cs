using System;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using ReadyToUseUIDemo.Droid.Activities;

namespace ReadyToUseUIDemo.Droid.Fragments
{
    public class SaveBottomSheetMenuFragment : BottomSheetDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.save_bottom_sheet, container, false);

            view.FindViewById<Button>(Resource.Id.save_with_ocr).Click += delegate
            {
                (Activity as PagePreviewActivity).SaveWithOcr();
                DismissAllowingStateLoss();
            };

            view.FindViewById<Button>(Resource.Id.save_without_ocr).Click += delegate
            {
                (Activity as PagePreviewActivity).SaveWithoutOcr();
                DismissAllowingStateLoss();
            };

            return view;
        }
    }
}

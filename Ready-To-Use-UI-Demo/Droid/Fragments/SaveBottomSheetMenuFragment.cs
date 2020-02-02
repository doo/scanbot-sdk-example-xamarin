using System;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using ReadyToUseUIDemo.Droid.Activities;
using ReadyToUseUIDemo.model;

namespace ReadyToUseUIDemo.Droid.Fragments
{
    public class SaveBottomSheetMenuFragment : BottomSheetDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.save_bottom_sheet, container, false);

            var saveWith = view.FindViewById<Button>(Resource.Id.save_with_ocr);
            saveWith.Text = Texts.save_with_ocr;
            saveWith.Click += delegate
            {
                (Activity as PagePreviewActivity).SaveWithOcr();
                DismissAllowingStateLoss();
            };

            var saveWithout = view.FindViewById<Button>(Resource.Id.save_without_ocr);
            saveWithout.Text = Texts.save_without_ocr;
            saveWithout.Click += delegate
            {
                (Activity as PagePreviewActivity).SaveWithoutOcr();
                DismissAllowingStateLoss();
            };

            var saveTiff = view.FindViewById<Button>(Resource.Id.save_tiff);
            saveTiff.Text = Texts.Tiff;
            saveTiff.Click += delegate
            {
                (Activity as PagePreviewActivity).SaveTiff();
                DismissAllowingStateLoss();
            };

            return view;
        }
    }
}

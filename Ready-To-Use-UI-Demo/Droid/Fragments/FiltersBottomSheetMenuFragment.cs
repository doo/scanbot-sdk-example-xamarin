using System;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using IO.Scanbot.Sdk.Process;
using ReadyToUseUIDemo.Droid.Activities;

namespace ReadyToUseUIDemo.Droid.Fragments
{
    public class FilterBottomSheetMenuFragment : BottomSheetDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.filters_bottom_sheet, container, false);

            view.FindViewById<Button>(Resource.Id.lowLightBinarizationFilter).Click += delegate {
                (Activity as PagePreviewActivity).ApplyFilter(ImageFilterType.LowLightBinarization);
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.edgeHighlightFilter).Click += delegate {
                (Activity as PagePreviewActivity).ApplyFilter(ImageFilterType.EdgeHighlight);
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.deepBinarizationFilter).Click += delegate {
                (Activity as PagePreviewActivity).ApplyFilter(ImageFilterType.DeepBinarization);
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.otsuBinarizationFilter).Click += delegate {
                (Activity as PagePreviewActivity).ApplyFilter(ImageFilterType.OtsuBinarization);
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.cleanBackgroundFilter).Click += delegate {
                (Activity as PagePreviewActivity).ApplyFilter(ImageFilterType.BackgroundClean);
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.colorDocumentFilter).Click += delegate {
                (Activity as PagePreviewActivity).ApplyFilter(ImageFilterType.ColorDocument);
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.colorFilter).Click += delegate {
                (Activity as PagePreviewActivity).ApplyFilter(ImageFilterType.ColorEnhanced);
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.grayscaleFilter).Click += delegate {
                (Activity as PagePreviewActivity).ApplyFilter(ImageFilterType.Grayscale);
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.binarizedFilter).Click += delegate {
                (Activity as PagePreviewActivity).ApplyFilter(ImageFilterType.Binarized);
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.pureBinarizedFilter).Click += delegate {
                (Activity as PagePreviewActivity).ApplyFilter(ImageFilterType.PureBinarized);
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.blackAndWhiteFilter).Click += delegate {
                (Activity as PagePreviewActivity).ApplyFilter(ImageFilterType.BlackAndWhite);
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.none).Click += delegate {
                (Activity as PagePreviewActivity).ApplyFilter(ImageFilterType.None);
                DismissAllowingStateLoss();
            };

            return view;
        }
    }
}

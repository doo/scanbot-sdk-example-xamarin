using System;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using IO.Scanbot.Sdk.Process;
using ReadyToUseUIDemo.Droid.Activities;
using ReadyToUseUIDemo.Droid.Listeners;

namespace ReadyToUseUIDemo.Droid.Fragments
{
    public class FilterBottomSheetMenuFragment : BottomSheetDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.filters_bottom_sheet, container, false);

            view.FindViewById<Button>(Resource.Id.lowLightBinarizationFilter).Click += delegate {
                (Activity as IFiltersListener).LowLightBinarizationFilter();
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.edgeHighlightFilter).Click += delegate {
                (Activity as IFiltersListener).EdgeHighlightFilter();
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.deepBinarizationFilter).Click += delegate {
                (Activity as IFiltersListener).DeepBinarizationFilter();
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.otsuBinarizationFilter).Click += delegate {
                (Activity as IFiltersListener).OtsuBinarizationFilter();
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.cleanBackgroundFilter).Click += delegate {
                (Activity as IFiltersListener).CleanBackgroundFilter();
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.colorDocumentFilter).Click += delegate {
                (Activity as IFiltersListener).ColorDocumentFilter();
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.colorFilter).Click += delegate {
                (Activity as IFiltersListener).ColorFilter();
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.grayscaleFilter).Click += delegate {
                (Activity as IFiltersListener).GrayscaleFilter();
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.binarizedFilter).Click += delegate {
                (Activity as IFiltersListener).BinarizedFilter();
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.pureBinarizedFilter).Click += delegate {
                (Activity as IFiltersListener).PureBinarizedFilter();
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.blackAndWhiteFilter).Click += delegate {
                (Activity as IFiltersListener).BlackAndWhiteFilter();
                DismissAllowingStateLoss();
            };
            view.FindViewById<Button>(Resource.Id.none).Click += delegate {
                (Activity as IFiltersListener).NoneFilter();
                DismissAllowingStateLoss();
            };

            return view;
        }
    }
}

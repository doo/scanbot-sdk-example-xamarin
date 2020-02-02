using System;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using IO.Scanbot.Sdk.Process;
using ReadyToUseUIDemo.Droid.Activities;
using ReadyToUseUIDemo.Droid.Listeners;
using ReadyToUseUIDemo.model;

namespace ReadyToUseUIDemo.Droid.Fragments
{
    public class FilterBottomSheetMenuFragment : BottomSheetDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.filters_bottom_sheet, container, false);

            var lowLightBinarizationFilter = view.FindViewById<Button>(Resource.Id.lowLightBinarizationFilter);
            lowLightBinarizationFilter.Text = Texts.low_light_binarization_filter;
            lowLightBinarizationFilter.Click += delegate {
                (Activity as IFiltersListener).LowLightBinarizationFilter();
                DismissAllowingStateLoss();
            };

            var lowLightBinarizationFilter2 = view.FindViewById<Button>(Resource.Id.lowLightBinarizationFilter2);
            lowLightBinarizationFilter2.Text = Texts.low_light_binarization_filter2;
            lowLightBinarizationFilter2.Click += delegate {
                (Activity as IFiltersListener).LowLightBinarizationFilter2();
                DismissAllowingStateLoss();
            };

            var edgeHighlightFilter = view.FindViewById<Button>(Resource.Id.edgeHighlightFilter);
            edgeHighlightFilter.Text = Texts.edge_highlight_filter;
            edgeHighlightFilter.Click += delegate {
                (Activity as IFiltersListener).EdgeHighlightFilter();
                DismissAllowingStateLoss();
            };

            var deepBinarizationFilter = view.FindViewById<Button>(Resource.Id.deepBinarizationFilter);
            deepBinarizationFilter.Text = Texts.deep_binarization_filter;
            deepBinarizationFilter.Click += delegate {
                (Activity as IFiltersListener).DeepBinarizationFilter();
                DismissAllowingStateLoss();
            };


            var otsuBinarizationFilter = view.FindViewById<Button>(Resource.Id.otsuBinarizationFilter);
            otsuBinarizationFilter.Text = Texts.otsu_binarization_filter;
            otsuBinarizationFilter.Click += delegate {
                (Activity as IFiltersListener).OtsuBinarizationFilter();
                DismissAllowingStateLoss();
            };


            var cleanBackgroundFilter = view.FindViewById<Button>(Resource.Id.cleanBackgroundFilter);
            cleanBackgroundFilter.Text = Texts.clean_background_filter;
            cleanBackgroundFilter.Click += delegate {
                (Activity as IFiltersListener).CleanBackgroundFilter();
                DismissAllowingStateLoss();
            };

            var colorDocumentFilter = view.FindViewById<Button>(Resource.Id.colorDocumentFilter);
            colorDocumentFilter.Text = Texts.color_document_filter;
            colorDocumentFilter.Click += delegate {
                (Activity as IFiltersListener).ColorDocumentFilter();
                DismissAllowingStateLoss();
            };

            var colorFilter = view.FindViewById<Button>(Resource.Id.colorFilter);
            colorFilter.Text = Texts.color_filter;
            colorFilter.Click += delegate {
                (Activity as IFiltersListener).ColorFilter();
                DismissAllowingStateLoss();
            };

            var grayscaleFilter = view.FindViewById<Button>(Resource.Id.grayscaleFilter);
            grayscaleFilter.Text = Texts.grayscale_filter;
            grayscaleFilter.Click += delegate {
                (Activity as IFiltersListener).GrayscaleFilter();
                DismissAllowingStateLoss();
            };

            var binarizedFilter = view.FindViewById<Button>(Resource.Id.binarizedFilter);
            binarizedFilter.Text = Texts.binarizedfilter;
            binarizedFilter.Click += delegate {
                (Activity as IFiltersListener).BinarizedFilter();
                DismissAllowingStateLoss();
            };

            var pureBinarizedFilter = view.FindViewById<Button>(Resource.Id.pureBinarizedFilter);
            pureBinarizedFilter.Text = Texts.pure_binarized_filter;
            pureBinarizedFilter.Click += delegate {
                (Activity as IFiltersListener).PureBinarizedFilter();
                DismissAllowingStateLoss();
            };

            var blackAndWhiteFilter = view.FindViewById<Button>(Resource.Id.blackAndWhiteFilter);
            blackAndWhiteFilter.Text = Texts.black_amp_white_filter;
            blackAndWhiteFilter.Click += delegate {
                (Activity as IFiltersListener).BlackAndWhiteFilter();
                DismissAllowingStateLoss();
            };

            var noneFilter = view.FindViewById<Button>(Resource.Id.none);
            noneFilter.Text = Texts.none;
            noneFilter.Click += delegate {
                (Activity as IFiltersListener).NoneFilter();
                DismissAllowingStateLoss();
            };

            return view;
        }
    }
}

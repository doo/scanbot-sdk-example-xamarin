
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using IO.Scanbot.Sdk;
using IO.Scanbot.Sdk.Persistence;
using IO.Scanbot.Sdk.Process;
using IO.Scanbot.Sdk.UI.View.Base;
using IO.Scanbot.Sdk.UI.View.Edit;
using IO.Scanbot.Sdk.UI.View.Edit.Configuration;
using ReadyToUseUIDemo.Droid.Fragments;
using ReadyToUseUIDemo.Droid.Listeners;
using ReadyToUseUIDemo.Droid.Repository;
using ReadyToUseUIDemo.Droid.Utils;
using ReadyToUseUIDemo.model;
using ScanbotSDK.Xamarin.Android;

namespace ReadyToUseUIDemo.Droid.Activities
{
    [Activity]
    public class PageFilterActivity : AppCompatActivity, IFiltersListener
    {
        const string PAGE_DATA = "PAGE_DATA";
        const string FILTERS_MENU_TAG = "FILTERS_MENU_TAG";
        const int CROP_DEFAULT_UI_REQUEST_CODE = 9999;

        public static Intent CreateIntent(Context context, Page page)
        {
            var intent = new Intent(context, typeof(PageFilterActivity));
            intent.PutExtra(PAGE_DATA, page as IParcelable);
            return intent;
        }

        Page selectedPage;
        Page SelectedPage
        {
            get => selectedPage;
            set => selectedPage = value;
        }

        ImageFilterType selectedFilter;
        FilterBottomSheetMenuFragment filterFragment;
        ProgressBar progress;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_filters);

            progress = FindViewById<ProgressBar>(Resource.Id.progress);

            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            SupportActionBar.Title = Texts.page_title;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

            var pageId = (Intent.GetParcelableExtra(PAGE_DATA) as Page).PageId;
            selectedPage = PageRepository.Pages.Find(p => p.PageId == pageId);

            selectedFilter = selectedPage.Filter;

            var crop = FindViewById<TextView>(Resource.Id.action_crop_and_rotate);
            crop.Text = Texts.crop_amp_rotate;

            var filter = FindViewById<TextView>(Resource.Id.action_filter);
            filter.Text = Texts.filter;
            filter.Click += delegate
            {
                filterFragment.Show(SupportFragmentManager, "CHOOSE_FILTERS_DIALOG_TAG");
            };

            var checkQuality = FindViewById<TextView>(Resource.Id.action_check_quality);
            checkQuality.Text = Texts.check_quality;
            checkQuality.Click += delegate
            {
                // below code returns a siingleton of ScanbotSDK class
                var scanbotSDK = new IO.Scanbot.Sdk.ScanbotSDK(this);
                var pageStorage = scanbotSDK.CreatePageFileStorage();
                var qualityAnalyzer = scanbotSDK.CreateDocumentQualityAnalyzer();
                var bitmap = pageStorage.GetPreviewImage(selectedPage.PageId, PageFileStorage.PageFileType.Document, null);
                var quality = qualityAnalyzer.AnalyzeInBitmap(bitmap, 0);

                Alert.Toast(this, "The Document quality is: " + quality.Name());
            };

            var delete = FindViewById<TextView>(Resource.Id.action_delete);
            delete.Text = Texts.delete;
            delete.Click += delegate
            {
                PageRepository.Remove(this, SelectedPage);
                Finish();
            };

            FindViewById(Resource.Id.action_crop_and_rotate).Click += delegate
            {
                var configuration = new CroppingConfiguration(SelectedPage);
                configuration.SetPolygonColor(Color.Red);
                configuration.SetPolygonColorMagnetic(Color.Blue);
                var intent = CroppingActivity.NewIntent(this, configuration);
                StartActivityForResult(intent, CROP_DEFAULT_UI_REQUEST_CODE);
            };

            var fragment = SupportFragmentManager.FindFragmentByTag(FILTERS_MENU_TAG);
            if (fragment != null)
            {
                SupportFragmentManager.BeginTransaction().Remove(fragment).CommitNow();
            }

            filterFragment = new FilterBottomSheetMenuFragment();

            if (!SBSDK.IsLicenseValid())
            {
                Alert.ShowLicenseDialog(this);
            }
            else
            {
                // Generate preview image
                progress.Visibility = ViewStates.Visible;
                Task.Run(delegate
                {
                    var uri = SBSDK.PageStorage.GetFilteredPreviewImageURI(selectedPage.PageId, selectedFilter);

                    if (!File.Exists(uri.Path))
                    {
                        SBSDK.PageProcessor.GenerateFilteredPreview(selectedPage, selectedFilter);
                    }

                    UpdateImage(uri);
                });
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok)
            {
                return;
            }

            if (requestCode == CROP_DEFAULT_UI_REQUEST_CODE)
            {
                var page = (Page)data.GetParcelableExtra(RtuConstants.ExtraKeyRtuResult);
                SelectedPage = PageRepository.Update(page);

                var uri = SBSDK.PageStorage.GetFilteredPreviewImageURI(SelectedPage.PageId, selectedFilter);

                if (!File.Exists(uri.Path))
                {
                    SBSDK.PageProcessor.GenerateFilteredPreview(selectedPage, selectedFilter);
                }

                UpdateImage(uri);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                base.OnBackPressed();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public void LowLightBinarizationFilter()
        {
            ApplyFilter(ImageFilterType.LowLightBinarization);
        }

        public void LowLightBinarizationFilter2()
        {
            ApplyFilter(ImageFilterType.LowLightBinarization2);
        }

        public void EdgeHighlightFilter()
        {
            ApplyFilter(ImageFilterType.EdgeHighlight);
        }

        public void DeepBinarizationFilter()
        {
            ApplyFilter(ImageFilterType.DeepBinarization);
        }

        public void OtsuBinarizationFilter()
        {
            ApplyFilter(ImageFilterType.OtsuBinarization);
        }

        public void CleanBackgroundFilter()
        {
            ApplyFilter(ImageFilterType.BackgroundClean);
        }

        public void ColorDocumentFilter()
        {
            ApplyFilter(ImageFilterType.ColorDocument);
        }

        public void ColorFilter()
        {
            ApplyFilter(ImageFilterType.ColorEnhanced);
        }

        public void GrayscaleFilter()
        {
            ApplyFilter(ImageFilterType.Grayscale);
        }

        public void BinarizedFilter()
        {
            ApplyFilter(ImageFilterType.Binarized);
        }

        public void PureBinarizedFilter()
        {
            ApplyFilter(ImageFilterType.PureBinarized);
        }

        public void BlackAndWhiteFilter()
        {
            ApplyFilter(ImageFilterType.BlackAndWhite);
        }

        public void NoneFilter()
        {
            ApplyFilter(ImageFilterType.None);
        }

        public void ApplyFilter(ImageFilterType type)
        {
            progress.Visibility = ViewStates.Visible;
            selectedFilter = type;
            Task.Run(delegate
            {
                selectedPage = PageRepository.Apply(selectedFilter, selectedPage);
                var uri = SBSDK.PageStorage.GetFilteredPreviewImageURI(selectedPage.PageId, selectedFilter);
                UpdateImage(uri);
            });
        }

        void UpdateImage(Android.Net.Uri uri)
        {
            RunOnUiThread(delegate
            {
                var image = FindViewById<ImageView>(Resource.Id.image);
                image.SetImageBitmap(null);
                image.SetImageBitmap(ImageLoader.Instance.Load(uri));
                progress.Visibility = ViewStates.Gone;
            });
        }
    }
}

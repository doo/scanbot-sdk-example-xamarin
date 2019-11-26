using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Pdf;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using IO.Scanbot.Sdk.Ocr;
using IO.Scanbot.Sdk.Persistence;
using IO.Scanbot.Sdk.Process;
using IO.Scanbot.Sdk.UI.View.Camera;
using IO.Scanbot.Sdk.UI.View.Camera.Configuration;
using Net.Doo.Snap.Camera;
using ReadyToUseUIDemo.Droid.Fragments;
using ReadyToUseUIDemo.Droid.Listeners;
using ReadyToUseUIDemo.Droid.Repository;
using ReadyToUseUIDemo.Droid.Utils;
using ReadyToUseUIDemo.model;
using ScanbotSDK.Xamarin.Android;

namespace ReadyToUseUIDemo.Droid.Activities
{
    [Activity]
    public class PagePreviewActivity : AppCompatActivity, IFiltersListener
    {
        const int FILTER_UI_REQUEST_CODE = 7777;
        const int CAMERA_ACTIVITY = 8888;

        const string FILTERS_MENU_TAG = "FILTERS_MENU_TAG";
        const string SAVE_MENU_TAG = "SAVE_MENU_TAG";

        Page selectedPage;

        PageAdapter adapter;
        RecyclerView recycleView;

        FilterBottomSheetMenuFragment filterFragment;
        SaveBottomSheetMenuFragment saveFragment;

        ProgressBar progress;
        TextView delete, filter;
        Button save;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_page_preview);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            SupportActionBar.Title = Texts.scan_results;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

            var fragment = SupportFragmentManager.FindFragmentByTag(FILTERS_MENU_TAG);
            if (fragment != null)
            {
                SupportFragmentManager.BeginTransaction().Remove(fragment).CommitNow();
            }

            filterFragment = new FilterBottomSheetMenuFragment();

            var fragment2 = SupportFragmentManager.FindFragmentByTag(SAVE_MENU_TAG);
            if (fragment2 != null)
            {
                SupportFragmentManager.BeginTransaction().Remove(fragment2).CommitNow();
            }

            saveFragment = new SaveBottomSheetMenuFragment();

            adapter = new PageAdapter();
            adapter.HasStableIds = true;
            adapter.Context = this;

            recycleView = FindViewById<RecyclerView>(Resource.Id.pages_preview);
            recycleView.HasFixedSize = true;
            recycleView.SetAdapter(adapter);
            
            var layout = new GridLayoutManager(this, 3);
            recycleView.SetLayoutManager(layout);

            adapter.SetItems(PageRepository.Pages);
            
            progress = FindViewById<ProgressBar>(Resource.Id.progressBar);

            var addPage = FindViewById<TextView>(Resource.Id.action_add_page);
            addPage.Text = Texts.add_page;
            addPage.Click += delegate
            {
                var configuration = new DocumentScannerConfiguration();
                configuration.SetCameraPreviewMode(CameraPreviewMode.FillIn);
                configuration.SetIgnoreBadAspectRatio(true);
                var intent = DocumentScannerActivity.NewIntent(this, configuration);
            };

            var results = FindViewById<TextView>(Resource.Id.scan_results);
            results.Text = Texts.scan_results;

            delete = FindViewById<TextView>(Resource.Id.action_delete_all);
            delete.Text = Texts.delete_all;
            delete.Click += delegate
            {
                PageRepository.Clear();
                adapter.NotifyDataSetChanged();
                delete.Enabled = false;
                filter.Enabled = false;
                save.Enabled = false;
            };

            filter = FindViewById<TextView>(Resource.Id.action_filter);
            filter.Text = Texts.filter;
            filter.Click += delegate
            {
                var existing = SupportFragmentManager.FindFragmentByTag(FILTERS_MENU_TAG);
                filterFragment.Show(SupportFragmentManager, FILTERS_MENU_TAG);
            };

            save = FindViewById<Button>(Resource.Id.action_save_document);
            save.Text = Texts.save;
            save.Click += delegate
            {
                var existing = SupportFragmentManager.FindFragmentByTag(SAVE_MENU_TAG);
                saveFragment.Show(SupportFragmentManager, SAVE_MENU_TAG);
            };
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (!SBSDK.IsLicenseValid())
            {
                Alert.ShowLicenseDialog(this);
            }

            delete.Enabled = !adapter.IsEmpty;
            filter.Enabled = !adapter.IsEmpty;
            save.Enabled = !adapter.IsEmpty;
        }

        public void SaveWithOcr()
        {
            SaveDocument(true);
        }

        public void SaveWithoutOcr()
        {
            SaveDocument(false);
        }

        void SaveDocument(bool withOCR)
        {
            if (!SBSDK.IsLicenseValid())
            {
                Alert.ShowLicenseDialog(this);
                return;
            }

            Task.Run(delegate
            {
                var input = adapter.GetUrls().ToArray();

                var external = GetExternalFilesDir(null).AbsolutePath;
                var targetFile = System.IO.Path.Combine(external, Guid.NewGuid() + ".pdf");
                var pdfOutputUri = Android.Net.Uri.FromFile(new Java.IO.File(targetFile));

                if (withOCR)
                {
                    var languages = SBSDK.GetOcrConfigs().InstalledLanguages.ToArray();
                    SBSDK.PerformOCR(input, languages, pdfOutputUri);
                }
                else
                {
                    SBSDK.CreatePDF(input, pdfOutputUri, ScanbotSDK.Xamarin.PDFPageSize.Auto);
                }

                Copier.Copy(this, pdfOutputUri);

                RunOnUiThread(delegate
                {
                    // TODO: Open first document
                });
            });
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

        public void ApplyFilter(ImageFilterType type)
        {
            progress.Visibility = ViewStates.Visible;
            Task.Run(delegate
            {
                PageRepository.Apply(type);
                RunOnUiThread(delegate {
                    adapter.NotifyDataSetChanged();
                    progress.Visibility = ViewStates.Gone;
                });
            });
        }

        public void OnRecycleViewItemClick(View v)
        {
            var position = recycleView.GetChildLayoutPosition(v);
            selectedPage = adapter.Items[position];

            var intent = PageFilterActivity.CreateIntent(this, selectedPage);
            StartActivityForResult(intent, FILTER_UI_REQUEST_CODE);
        }

        public void LowLightBinarizationFilter()
        {
            ApplyFilter(ImageFilterType.LowLightBinarization);
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
    }

    class PageAdapter : RecyclerView.Adapter
    {
        Context context;
        public Context Context
        {
            get => context;
            set
            {
                context = value;
                listener = new RecyclerViewItemClick(Context as PagePreviewActivity);
            }
        }

        public List<Page> Items { get; private set; } = new List<Page>();

        public override int ItemCount => Items.Count;

        public bool IsEmpty { get => ItemCount == 0; }

        RecyclerViewItemClick listener;

        public void SetItems(List<Page> pages)
        {
            Items.Clear();
            Items.AddRange(pages);
            NotifyDataSetChanged();
        }
        public List<Android.Net.Uri> GetUrls()
        {
            var urls = new List<Android.Net.Uri>();
            foreach (Page page in Items)
            {
                var path = GetUri(page, PageFileStorage.PageFileType.Document);
                var original = GetUri(page, PageFileStorage.PageFileType.Original);
                if (File.Exists(path.Path))
                {
                    urls.Add(path);
                }
                else
                {
                    urls.Add(original);
                }
            }

            return urls;
        }

        public override long GetItemId(int position)
        {
            return Items[position].GetHashCode();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(Context).Inflate(Resource.Layout.item_page, parent, false);
            view.SetOnClickListener(listener);
            return new PageViewHolder(view);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var page = Items[position];
            var path = GetUri(page, PageFileStorage.PageFileType.Document);
            var original = GetUri(page, PageFileStorage.PageFileType.Original);

            if (File.Exists(path.Path))
            {
                (holder as PageViewHolder).image.SetImageURI(path);
            }
            else
            {
                (holder as PageViewHolder).image.SetImageURI(original);
            }
        }

        Android.Net.Uri GetUri(Page page, PageFileStorage.PageFileType type)
        {
            return SBSDK.PageStorage.GetPreviewImageURI(page.PageId, type);
        }
    }

    class PageViewHolder : RecyclerView.ViewHolder
    {
        public ImageView image;
        public PageViewHolder(View item) : base(item)
        {
            image = item.FindViewById<ImageView>(Resource.Id.page);
        }
    }

    class RecyclerViewItemClick : Java.Lang.Object, View.IOnClickListener
    {

        public PagePreviewActivity Context { get; private set; }

        public RecyclerViewItemClick(PagePreviewActivity context)
        {
            Context = context;
        }

        public void OnClick(View v)
        {
            Context.OnRecycleViewItemClick(v);
        }
    }
}


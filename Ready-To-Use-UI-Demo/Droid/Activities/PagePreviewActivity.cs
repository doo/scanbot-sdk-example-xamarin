using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
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
using Net.Doo.Snap.Persistence;
using Net.Doo.Snap.Persistence.Cleanup;
using Net.Doo.Snap.Process;
using Net.Doo.Snap.Process.Draft;
using ReadyToUseUIDemo.Droid.Fragments;
using ReadyToUseUIDemo.Droid.Repository;
using ReadyToUseUIDemo.Droid.Utils;
using ScanbotSDK.Xamarin.Android;
using Square.Picasso;

namespace ReadyToUseUIDemo.Droid.Activities
{
    [Activity]
    public class PagePreviewActivity : AppCompatActivity
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
        View delete, filter, save;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_page_preview);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            SupportActionBar.Title = GetString(Resource.String.scan_results);
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
            recycleView.SetLayoutManager(new GridLayoutManager(this, 3));

            adapter.SetItems(PageRepository.Pages);
            
            progress = FindViewById<ProgressBar>(Resource.Id.progressBar);

            FindViewById(Resource.Id.action_add_page).Click += delegate
            {
                var configuration = new DocumentScannerConfiguration();
                configuration.SetCameraPreviewMode(CameraPreviewMode.FillIn);
                configuration.SetIgnoreBadAspectRatio(true);
                var intent = DocumentScannerActivity.NewIntent(this, configuration);
            };

            delete = FindViewById(Resource.Id.action_delete_all);
            filter = FindViewById(Resource.Id.action_filter);
            save = FindViewById(Resource.Id.action_save_document);

            delete.Click += delegate
            {
                PageRepository.Clear();
                adapter.NotifyDataSetChanged();
                delete.Enabled = false;
                filter.Enabled = false;
                save.Enabled = false;
            };

            filter.Click += delegate
            {
                var existing = SupportFragmentManager.FindFragmentByTag(FILTERS_MENU_TAG);
                filterFragment.Show(SupportFragmentManager, FILTERS_MENU_TAG);
            };

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
                ShowLicenseDialog();
            }

            delete.Enabled = !adapter.IsEmpty;
            filter.Enabled = !adapter.IsEmpty;
            save.Enabled = !adapter.IsEmpty;
        }


        void ShowLicenseDialog()
        {
            var text =
                "The demo app will terminate because of the missing license key. " +
                "Get your free 30-day license today!";
            Toast.MakeText(this, text, ToastLength.Long);
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
                ShowLicenseDialog();
                return;
            }

            Task.Run(delegate
            {
                var input = adapter.GetUrls().ToArray();

                var external = GetExternalFilesDir(null).AbsolutePath;
                var targetFile = Path.Combine(external, Guid.NewGuid() + ".pdf");
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
    }

    class PageAdapter : RecyclerView.Adapter
    {
        public Context Context { get; set; }

        List<Page> items = new List<Page>();

        public override int ItemCount => items.Count;

        public bool IsEmpty { get => ItemCount == 0; }

        public void SetItems(List<Page> pages)
        {
            items.Clear();
            items.AddRange(pages);
            NotifyDataSetChanged();
        }
        public List<Android.Net.Uri> GetUrls()
        {
            var urls = new List<Android.Net.Uri>();
            foreach (Page page in items)
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

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(Context).Inflate(Resource.Layout.item_page, parent, false);
            //view.setOnClickListener(mOnClickListener)
            return new PageViewHolder(view);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var page = items[position];
            var path = GetUri(page, PageFileStorage.PageFileType.Document).Path;
            var original = GetUri(page, PageFileStorage.PageFileType.Original).Path;

            if (File.Exists(path))
            {
                LoadResized(holder, path);
            }
            else
            {
                LoadResized(holder, original);
            }
        }

        void LoadResized(RecyclerView.ViewHolder holder, string path)
        {
            var size = Resource.Dimension.move_preview_size;
            Picasso.With(Context)
                .Load(path)
                .MemoryPolicy(MemoryPolicy.NoCache)
                .Resize(size, size)
                .CenterInside()
                .Into((holder as PageViewHolder).image);
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
}


﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using IO.Scanbot.Sdk.Camera;
using IO.Scanbot.Sdk.Persistence;
using IO.Scanbot.Sdk.Process;
using IO.Scanbot.Sdk.UI.View.Base;
using IO.Scanbot.Sdk.UI.View.Camera;
using IO.Scanbot.Sdk.UI.View.Camera.Configuration;
using IO.Scanbot.Sdk.Util.Thread;
using ReadyToUseUIDemo.Droid.Fragments;
using ReadyToUseUIDemo.Droid.Listeners;
using ReadyToUseUIDemo.Droid.Repository;
using ReadyToUseUIDemo.Droid.Utils;
using ReadyToUseUIDemo.model;
using ScanbotSDK.Xamarin;
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

            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
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
                StartActivityForResult(intent, CAMERA_ACTIVITY);
            };

            var results = FindViewById<TextView>(Resource.Id.scan_results);
            results.Text = Texts.scan_results;

            delete = FindViewById<TextView>(Resource.Id.action_delete_all);
            delete.Text = Texts.delete_all;
            delete.Click += delegate
            {
                PageRepository.Clear();
                adapter.Items.Clear();
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

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok && requestCode == CAMERA_ACTIVITY)
            {
                var pages = data.GetParcelableArrayExtra(RtuConstants.ExtraKeyRtuResult).Cast<Page>().ToList();
                PageRepository.Add(pages);
            }
            adapter.SetItems(PageRepository.Pages);
            adapter.NotifyDataSetChanged();
            UpdateVisibility();
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (!SBSDK.IsLicenseValid())
            {
                Alert.ShowLicenseDialog(this);
            }

            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            delete.Enabled = !adapter.IsEmpty;
            filter.Enabled = !adapter.IsEmpty;
            save.Enabled = !adapter.IsEmpty;
        }

        enum SaveType
        {
            Plain,
            OCR,
            TIFF
        }

        public void SaveWithOcr()
        {
            SaveDocument(SaveType.OCR);
        }

        public void SaveWithoutOcr()
        {
            SaveDocument(SaveType.Plain);
        }

        public void SaveTiff()
        {
            SaveDocument(SaveType.TIFF);
        }

        void SaveDocument(SaveType type)
        {
            if (!SBSDK.IsLicenseValid())
            {
                Alert.ShowLicenseDialog(this);
                return;
            }

            Task.Run(delegate
            {
                var input = adapter.GetDocumentUris().ToArray();
                var output = GetOutputUri(".pdf");

                if (type == SaveType.TIFF)
                {
                    output = GetOutputUri(".tiff");
                    // Please note that some compression types are only compatible for 1-bit encoded images (binarized black & white images)!
                    var options = new TiffOptions { OneBitEncoded = true, Compression = TiffCompressionOptions.CompressionCcittfax4, Dpi = 250 };
                    bool success = SBSDK.WriteTiff(input, output, options);
                }
                else if (type == SaveType.OCR)
                {
                    var languages = SBSDK.GetOcrConfigs().InstalledLanguages.ToArray();

                    if (languages.Length == 0)
                    {
                        RunOnUiThread(delegate
                        {
                            Alert.Toast(this, "OCR languages blobs are not available");
                        });
                        return;
                    }
                    SBSDK.PerformOCR(input, SBSDK.GetOcrConfigs(), output);
                }
                else
                {
                    SBSDK.CreatePDF(input, output, PDFPageSize.A4);
                }

                Java.IO.File file = Copier.Copy(this, output);

                var intent = new Intent(Intent.ActionView, output);
                
                var authority = ApplicationContext.PackageName + ".provider";
                var uri = FileProvider.GetUriForFile(this, authority, file);
                
                intent.SetDataAndType(uri, MimeUtils.GetMimeByName(file.Name));
                intent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);
                intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);

                RunOnUiThread(delegate
                {
                    StartActivity(Intent.CreateChooser(intent, output.LastPathSegment));
                    Alert.Toast(this, "File saved to: " + output.Path);
                });
            });
        }

        Android.Net.Uri GetOutputUri(string extension)
        {
            var external = GetExternalFilesDir(null).AbsolutePath;
            var filename = Guid.NewGuid() + extension;
            var targetFile = System.IO.Path.Combine(external, filename);
            return Android.Net.Uri.FromFile(new Java.IO.File(targetFile));
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

        public List<Android.Net.Uri> GetDocumentUris()
        {
            var uris = new List<Android.Net.Uri>();
            foreach (Page page in Items)
            {
                var documentUri = GetUri(page, PageFileStorage.PageFileType.Document);
                var originalUri = GetUri(page, PageFileStorage.PageFileType.Original);
                if (File.Exists(documentUri.Path))
                {
                    uris.Add(documentUri);
                }
                else
                {
                    uris.Add(originalUri);
                }
            }

            return uris;
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
            var path = GetPreviewUri(page, PageFileStorage.PageFileType.Document);
            var original = GetPreviewUri(page, PageFileStorage.PageFileType.Original);

            (holder as PageViewHolder).image.SetImageResource(0);

            var options = new BitmapFactory.Options();
            if (File.Exists(path.Path))
            {
                var bitmap = ImageLoader.Instance.Load(path);
                (holder as PageViewHolder).image.SetImageBitmap(bitmap);
            }
            else
            {
                var bitmap = ImageLoader.Instance.Load(original);
                (holder as PageViewHolder).image.SetImageBitmap(bitmap);
            }
        }

        Android.Net.Uri GetPreviewUri(Page page, PageFileStorage.PageFileType type)
        {
            // preview URI (low-res!)
            return SBSDK.PageStorage.GetPreviewImageURI(page.PageId, type);
        }

        Android.Net.Uri GetUri(Page page, PageFileStorage.PageFileType type)
        {
            // hi-res(!) URI
            return SBSDK.PageStorage.GetImageURI(page.PageId, type);
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


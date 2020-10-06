
using System.Collections.Generic;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using IO.Scanbot.Sdk.Businesscard;
using IO.Scanbot.Sdk.Persistence;

namespace ClassicalComponentsDemo.Droid.Activities
{
    [Activity(Label = "Business Cards Preview")]
    public class BusinessCardsPreviewActivity : BaseActivity
    {
        public RecyclerView RecyclerView { get; private set; }

        public BusinessCardsAdapter Adapter { get; private set; }
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.BusinessCardsPreview);

            RecyclerView = FindViewById<RecyclerView>(Resource.Id.pages_preview);
            RecyclerView.HasFixedSize = true;

            Adapter = new BusinessCardsAdapter(this);
            RecyclerView.SetAdapter(Adapter);

            RecyclerView.SetLayoutManager(new GridLayoutManager(this, 3));
        }

        public static Android.Net.Uri GetPath(Context context, string pageId, PageFileStorage.PageFileType type)
        {
            var sdk = new IO.Scanbot.Sdk.ScanbotSDK(context);
            return sdk.PageFileStorage.GetPreviewImageURI(pageId, type);
        }

    }

    public class BusinessCardsAdapter : RecyclerView.Adapter
    {
        public List<BusinessCardsImageProcessorBusinessCardProcessingResult> Items
        {
            get => BusinessCardsActivity.ProcessedResults;
        }

        public override int ItemCount => Items.Count;

        BusinessCardsPreviewActivity context;
        BusinessCardClickListener listener;
        public BusinessCardsAdapter(BusinessCardsPreviewActivity context)
        {
            this.context = context;
            listener = new BusinessCardClickListener(context);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var card = Items[position];

            var type = PageFileStorage.PageFileType.UnfilteredDocument;
            var documentPath = BusinessCardsPreviewActivity.GetPath(context, card.Page.PageId, type);

            type = PageFileStorage.PageFileType.Original;
            var originalImagePath = BusinessCardsPreviewActivity.GetPath(context, card.Page.PageId, type);

            (holder as PageViewHolder).imageView.SetImageResource(0);

            if (File.Exists(documentPath.Path))
            {
                (holder as PageViewHolder).imageView.SetImageURI(documentPath);
            }
            else
            {
                (holder as PageViewHolder).imageView.SetImageURI(originalImagePath);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.ItemPage, parent, false);
            view.SetOnClickListener(listener);
            return new PageViewHolder(view);
        }
    }

    public class BusinessCardClickListener : Java.Lang.Object, View.IOnClickListener
    {
        BusinessCardsPreviewActivity context;

        public BusinessCardClickListener(BusinessCardsPreviewActivity context)
        {
            this.context = context;
        }

        public void OnClick(View v)
        {
            var position = context.RecyclerView.GetChildLayoutPosition(v);
            BusinessCardPreviewActivity.SelectedItem = context.Adapter.Items[position];

            var intent = new Intent(context, typeof(BusinessCardPreviewActivity));
            context.StartActivity(intent);
        }
    }

    public class PageViewHolder : RecyclerView.ViewHolder
    {
        public ImageView imageView;

        public PageViewHolder(View item) : base(item)
        {
            imageView = item.FindViewById<ImageView>(Resource.Id.page);
        }
    }
}

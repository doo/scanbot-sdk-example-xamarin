using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using ReadyToUseUIDemo.Droid.Activities;
using ReadyToUseUIDemo.model;

namespace ReadyToUseUIDemo.Droid.Fragments
{
    internal enum SaveType
    {
        PDF,
        SANDWICH_PDF,
        OCR,
        TIFF
    }

    public class SaveBottomSheetMenuFragment : BottomSheetDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.save_bottom_sheet, container, false);

            var createPdf = view.FindViewById<Button>(Resource.Id.create_pdf);
            createPdf.Text = Texts.save_without_ocr;
            createPdf.Click += delegate { ItemSelected(SaveType.PDF); };

            var performOcr = view.FindViewById<Button>(Resource.Id.perform_ocr);
            performOcr.Text = Texts.perform_ocr;
            performOcr.Click += delegate { ItemSelected(SaveType.OCR); };

            var createSandwichPdf = view.FindViewById<Button>(Resource.Id.save_with_ocr);
            createSandwichPdf.Text = Texts.save_with_ocr;
            createSandwichPdf.Click += delegate { ItemSelected(SaveType.SANDWICH_PDF); };

            var saveTiff = view.FindViewById<Button>(Resource.Id.save_tiff);
            saveTiff.Text = Texts.Tiff;
            saveTiff.Click += delegate { ItemSelected(SaveType.TIFF); };

            return view;
        }

        private void ItemSelected(SaveType option)
        {
            (Activity as PagePreviewActivity).SaveDocument(option);
            DismissAllowingStateLoss();
        }
    }
}

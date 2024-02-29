namespace ReadyToUseUIDemo.Droid.Listeners
{
    interface IFiltersListener
    {
        void LowLightBinarizationFilter2();

        void LowLightBinarizationFilter();

        void EdgeHighlightFilter();

        void DeepBinarizationFilter();

        void OtsuBinarizationFilter();

        void CleanBackgroundFilter();
        
        void ColorDocumentFilter();

        void ColorFilter();

        void GrayscaleFilter();

        void BinarizedFilter();

        void PureBinarizedFilter();

        void BlackAndWhiteFilter();

        void NoneFilter();
    }
}

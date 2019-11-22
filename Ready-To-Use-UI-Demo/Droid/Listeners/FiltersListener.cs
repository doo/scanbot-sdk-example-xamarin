using System;
namespace ReadyToUseUIDemo.Droid.Listeners
{
    interface IFiltersListener
    {
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

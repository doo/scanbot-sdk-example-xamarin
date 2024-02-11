using IO.Scanbot.Sdk.UI.View.Generictext.Entity;
namespace ReadyToUseUIDemo.Droid.Listeners
{
    public class TextDataScannerListeners : Java.Lang.Object, TextDataScannerStep.IGenericTextValidationCallback, TextDataScannerStep.ICleanRecognitionResultCallback
    {
        public string Process(string rawText)
        {
            return rawText;
        }

        public bool Validate(string text)
        {
            return true;
        }
    }
}


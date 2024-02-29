using System;
using static IO.Scanbot.Sdk.UI.View.Generictext.Entity.TextDataScannerStep;

namespace ReadyToUseUIDemo.Droid.Utils
{
    public class ValidationCallback : Java.Lang.Object, IGenericTextValidationCallback
    {
        public bool Validate(string text)
        {
            Console.WriteLine("ValidationCallback.Validate: " + text);
            return text.Length > 0;
        }
    }

    public class RecognitionCallback : Java.Lang.Object, ICleanRecognitionResultCallback
    {
        public string Process(string rawText)
        {
            Console.WriteLine("RecognitionCallback.Process: " + rawText);
            return rawText;
        }
    }
}

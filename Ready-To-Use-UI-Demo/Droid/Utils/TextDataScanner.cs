using System;
using IO.Scanbot.Sdk.Generictext;

namespace ReadyToUseUIDemo.Droid.Utils
{
    public class ValidationCallback : Java.Lang.Object, IGenericTextRecognizerGenericTextValidationCallback
    {
        public bool Validate(string text)
        {
            Console.WriteLine("ValidationCallback.Validate: " + text);
            return true;
        }
    }

    public class RecognitionCallback : Java.Lang.Object, IGenericTextRecognizerCleanRecognitionResultCallback
    {
        public string Process(string rawText)
        {
            Console.WriteLine("RecognitionCallback.Process: " + rawText);
            return rawText;
        }
    }
}

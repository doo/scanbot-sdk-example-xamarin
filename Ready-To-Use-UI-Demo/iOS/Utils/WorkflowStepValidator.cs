
using System.Collections.Generic;
using System.Text;
using Foundation;
using ReadyToUseUIDemo.iOS.Controller;
using ReadyToUseUIDemo.model;
using ScanbotSDK.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Utils
{
    public class WorkflowStepValidator
    {
        public static UIViewController MainController { get; set; }
        public static UIViewController WorkflowController { get; set; }

        public static NSError OnDCFormStep(SBSDKUIWorkflowStepResult result)
        {
            var images = new List<UIImage>();

            if (result.DisabilityCertificateResult == null || !result.DisabilityCertificateResult.RecognitionSuccessful)
            {
                return new NSError();       
            }
            
            string text = result.DisabilityCertificateResult.StringRepresentation;

            if (result.Thumbnail != null)
            {
                images.Add(result.Thumbnail);
            }

            PresentResultPopup(text, images);
            return null;
        }

        public static NSError OnBarCodeStep(SBSDKUIWorkflowStepResult result)
        {
            var images = new List<UIImage>();

            if (result.BarcodeResults.Length == 0)
            {
                return new NSError();
            }

            if (result.Thumbnail != null)
            {
                images.Add(result.Thumbnail);
            }

            string text = result.BarcodeResults[0].StringValue;
            PresentResultPopup(text, images);
            return null;
        }

        public static NSError OnPayFormStep(SBSDKUIWorkflowStepResult result)
        {
            if (result.PayformResult == null || result.PayformResult.RecognizedFields.Length == 0)
            {
                return new NSError();
            }

            var images = new List<UIImage>();

            if (result.Thumbnail != null)
            {
                images.Add(result.Thumbnail);
            }

            var builder = new StringBuilder();
            foreach (var field in result.PayformResult.RecognizedFields)
            {
                builder.Append("• ");
                builder.Append(field.Token.Type.ToString());
                builder.Append(": ");
                builder.Append(field.Value);
                builder.Append("\n");
            }

            PresentResultPopup(builder.ToString(), images);
            return null;
        }

        static UIImage front;
        public static NSError OnIDCardFrontStep(SBSDKUIWorkflowStepResult result)
        {
            front = result.Thumbnail;
            return null;
        }

        public static NSError OnIDCardBackStep(SBSDKUIWorkflowStepResult result)
        {
            var images = new List<UIImage>();

            var builder = new StringBuilder();
            var step = result.MrzResult;

            if (step == null || !step.RecognitionSuccessfull)
            {
                return new NSError();
            }

            if (front != null)
            {
                images.Add(front);
            }
            if (result.Thumbnail != null)
            {
                images.Add(result.Thumbnail);
            }

            builder.Append(Texts.mrz_document_type).Append(" ").Append(step.TravelDocumentTypeField.Value).Append("\n");
            builder.Append(Texts.mrz_document_country).Append(" ").Append(step.NationalityField.Value).Append("\n");
            builder.Append(Texts.mrz_last_name).Append(" ").Append(step.LastNameField.Value).Append("\n");
            builder.Append(Texts.mrz_first_name).Append(" ").Append(step.FirstNameField.Value).Append("\n");
            builder.Append(Texts.mrz_document_code).Append(" ").Append(step.DocumentCodeField.Value).Append("\n");
            builder.Append(Texts.mrz_dob).Append(" ").Append(step.DateOfBirthField.Value).Append("\n");
            builder.Append(Texts.mrz_gender).Append(" ").Append(step.GenderField.Value).Append("\n");

            var validity = "Invalid";

            if (step.CheckDigitsCount == step.ValidCheckDigitsCount)
            {
                validity = "Valid";
            }

            builder.Append(Texts.mrz_checksums).Append(" ").Append(validity).Append("\n");

            PresentResultPopup(builder.ToString(), images);

            return null;
        }

        static void PresentResultPopup(string content, List<UIImage> images)
        {
            var popover = new PopupController(content, images);

            WorkflowController.DismissViewController(false, null);

            MainController.PresentViewController(popover, true, delegate
            {
                // Remove static instance of front scan after presented
                front = null;
                popover.Content.CloseButton.Click += delegate
                {
                    popover.Dismiss();
                };
            });
        }
    }
}


using System.Text;
using ReadyToUseUIDemo.iOS.Controller;
using ReadyToUseUIDemo.model;
using ScanbotSDK.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS
{
    public class ScanResultCallback : SBSDKUIWorkflowScannerViewControllerDelegate
    {
        public UIViewController Parent { get; set; }

        public override void WorkflowScanViewController(SBSDKUIWorkflowScannerViewController viewController,
            SBSDKUIWorkflow workflow, SBSDKUIWorkflowStepResult[] results)
        {
            var text = "No results to process, please try again";

            if (results.Length == 0)
            {
                PresentResultPopup(viewController, text);
                return;
            }

            // Default content, if everything else fails
            text = "Unable to process result, please try again";
            var result = results[0];

            if (result.DisabilityCertificateResult != null)
            {
                if (!results[0].DisabilityCertificateResult.RecognitionSuccessful)
                {
                    text = "Recognition failed";
                }
                else
                {
                    text = results[0].DisabilityCertificateResult.StringRepresentation;
                }
            }
            if (result.Step is SBSDKUIScanMachineReadableZoneWorkflowStep)
            {
                var builder = new StringBuilder();
                var step = result.MrzResult;
                
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

                text = builder.ToString();
            }

            PresentResultPopup(viewController, text);
        }

        void PresentResultPopup(UIViewController current, string content)
        {
            var popover = new PopupController(content);
            current.DismissViewController(false, delegate
            {
                Parent.PresentViewController(popover, true, delegate {
                    popover.Content.CloseButton.Click += delegate {
                        popover.Dismiss();
                    };
                    popover.Content.CopyButton.Click += delegate {
                        // TODO Clipboard copy
                        popover.Dismiss();
                    };

                });
            });
        }

    }
}


using System.Collections.Generic;
using System.Text;
using Plugin.Clipboard;
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
            var images = new List<UIImage>();

            if (results.Length == 0)
            {
                PresentResultPopup(viewController, text, images);
                return;
            }

            // Default content, if everything else fails
            text = "Unable to process result, please try again";
            var result = results[0];

            if (result.DisabilityCertificateResult != null)
            {
                if (!result.DisabilityCertificateResult.RecognitionSuccessful)
                {
                    text = "Recognition failed";
                }
                else
                {
                    text = result.DisabilityCertificateResult.StringRepresentation;
                }
            }
            else if (result.Step is SBSDKUIScanBarCodeWorkflowStep)
            {
                
                if (result.Thumbnail != null)
                {
                    images.Add(result.Thumbnail);
                }
                if (result.BarcodeResults.Length > 0)
                {
                    text = result.BarcodeResults[0].StringValue;
                }
            }
            else if (result.Step is SBSDKUIScanPayFormWorkflowStep)
            {
                if (result.Thumbnail != null)
                {
                    images.Add(result.Thumbnail);
                }

                if (result.PayformResult == null)
                {
                    text = "Failed to recognize form, please try again";
                }
                else
                {
                    var builder = new StringBuilder();
                    foreach (var field in result.PayformResult.RecognizedFields)
                    {
                        builder.Append("• ");
                        builder.Append(field.Token.Type.ToString());
                        builder.Append(": ");
                        builder.Append(field.Value);
                        builder.Append("\n");
                    }
                    text = builder.ToString();
                }
            }
            else if (result.Step is SBSDKUIScanMachineReadableZoneWorkflowStep)
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

                if (result.Thumbnail != null)
                {
                    images.Add(result.Thumbnail);
                }

                // Will be true when front side of the document was also scanned
                if (results.Length > 1 && results[1].Thumbnail != null)
                {
                    images.Add(results[1].Thumbnail);
                }

                text = builder.ToString();
            }

            PresentResultPopup(viewController, text, images);
        }

        void PresentResultPopup(UIViewController current, string content, List<UIImage> images)
        {
            var popover = new PopupController(content, images);

            current.DismissViewController(false, delegate
            {
                Parent.PresentViewController(popover, true, delegate {
                    popover.Content.CloseButton.Click += delegate {
                        popover.Dismiss();
                    };
                    popover.Content.CopyButton.Click += delegate {
                        // TODO Clipboard copy
                        CrossClipboard.Current.SetText(content);
                        popover.Dismiss();
                    };

                });
            });
        }

    }
}

using System;
using AVFoundation;
using CoreGraphics;
using Foundation;
using ScanbotSDK.iOS;

namespace ClassicalComponentsDemo.iOS
{

    public class WorkflowError : NSObject
    {
        public static NSError ErrorWithCode(int errorCode, NSString description)
        {
            NSDictionary userinfo = null;
            if (description != null)
            {
                userinfo = NSDictionary.FromObjectAndKey(description, NSError.LocalizedDescriptionKey);
            }

            return NSError.FromDomain(new NSString("WorkflowErrorDomain"), errorCode, userinfo);
        }
    }

    public class WorkflowFactory
    {

        public static SBSDKUIWorkflow[] AllWorkflows()
        {
            return new SBSDKUIWorkflow[] { GermanIDCard(), UkrainianPassport(), QrCodeAndDocument(), Payform(), DisabilityCertificate(), BlackWhiteDocument() };
        }

        static SBSDKUIWorkflow GermanIDCard()
        {
            SBSDKPageAspectRatio[] ratios = { new SBSDKPageAspectRatio(1.0, 0.6353) };

            SBSDKUIWorkflowStep frontSide = new SBSDKUIScanMachineReadableZoneWorkflowStep(
                "German ID card 1/2",
                "Please scan the front of your id card.",
                ratios,
                true,
                (result) =>
                {
                    SBSDKMachineReadableZoneRecognizerResult mrz = result.MrzResult;
                    if (mrz.RecognitionSuccessfull)
                    {
                        return WorkflowError.ErrorWithCode(1, new NSString("This does not seem to be the front side."));
                    }
                    return null;
                }
                );


            SBSDKUIWorkflowStep backSide = new SBSDKUIScanMachineReadableZoneWorkflowStep(
            "German ID card 2/2",
            "Please scan the back of your id card.",
            ratios,
            true,
            (result) =>
            {
                SBSDKMachineReadableZoneRecognizerResult mrz = result.MrzResult;
                if (mrz == null || !mrz.RecognitionSuccessfull)
                {
                    return WorkflowError.ErrorWithCode(2, new NSString("This does not seem to be the back side."));
                }

                if (mrz.DocumentType != SBSDKMachineReadableZoneRecognizerResultDocumentType.IDCard)
                {
                    return WorkflowError.ErrorWithCode(3, new NSString("This does not seem to be an ID card."));
                }

                if (mrz.DocumentCodeField.Value.Length != 9 || !(mrz.IssuingStateOrOrganizationField.Value != "D"))
                {
                    return WorkflowError.ErrorWithCode(4, new NSString("This does not seem to be an ID card."));
                }

                return null;
            }
            );

            SBSDKUIWorkflow workflow = new SBSDKUIWorkflow(new SBSDKUIWorkflowStep[] { frontSide, backSide }, "German ID card", null);
            return workflow;
        }

        static SBSDKUIWorkflow BlackWhiteDocument()
        {
            SBSDKPageAspectRatio[] portrait = { new SBSDKPageAspectRatio(1.0, 1.4143) };
            SBSDKPageAspectRatio[] landscape = { new SBSDKPageAspectRatio(1.4143, 1.0) };

            SBSDKUIWorkflowStep portraitStep = new SBSDKUIScanDocumentPageWorkflowStep(
                "Black & White Document 1/2",
                "Please scan a PORTRAIT A4 document.",
                portrait,
                (page) => page.Filter = SBSDKImageFilterType.BlackAndWhite,
                null
                );

            SBSDKUIWorkflowStep landscapeStep = new SBSDKUIScanDocumentPageWorkflowStep(
                "Black & White Document 2/2",
                "Please scan a LANDSCAPE A4 document.",
                landscape,
                (page) => page.Filter = SBSDKImageFilterType.BlackAndWhite,
                null
                );

            SBSDKUIWorkflow workflow = new SBSDKUIWorkflow(new SBSDKUIWorkflowStep[] { portraitStep, landscapeStep }, "2-page, black and white document", null);
            return workflow;
        }

        static SBSDKUIWorkflow QrCodeAndDocument()
        {

            SBSDKUIWorkflowStep qrcodeStep = new SBSDKUIScanBarCodeWorkflowStep(
                "QR code and Document 1/2",
                "Please scan a QR code",
                new String[] { AVConstants.AVMetadataObjectTypeQRCode.ToString() },
                new CGSize(1, 1),
                null
                );

            SBSDKUIWorkflowStep documentStep = new SBSDKUIScanDocumentPageWorkflowStep(
                "QR code and Document 2/2",
                "Please scan a document.",
                null,
                null,
                null
                );

            SBSDKUIWorkflow workflow = new SBSDKUIWorkflow(new SBSDKUIWorkflowStep[] { qrcodeStep, documentStep }, "QR code and document", null);
            return workflow;
        }

        static SBSDKUIWorkflow UkrainianPassport()
        {
            SBSDKUIWorkflowStep frontSide = new SBSDKUIWorkflowStep(
                "Ukrainian passport 1/1",
                "Please scan the front of your id card.",
                null,
                true,
                false,
                null,
                (result) =>
                {
                    SBSDKMachineReadableZoneRecognizer recognizer = new SBSDKMachineReadableZoneRecognizer();
                    SBSDKMachineReadableZoneRecognizerResult mrz = recognizer.RecognizePersonalIdentityFromImage(result.CapturedPage.DocumentImage);

                    if (mrz == null || !mrz.RecognitionSuccessfull)
                    {
                        return WorkflowError.ErrorWithCode(2, new NSString("This does not seem to be the correct page."));
                    }

                    if (mrz.DocumentType != SBSDKMachineReadableZoneRecognizerResultDocumentType.Passport)
                    {
                        return WorkflowError.ErrorWithCode(3, new NSString("This does not seem to be a passport."));
                    }

                    if (mrz.DocumentCodeField.Value.Length != 8 || !(mrz.IssuingStateOrOrganizationField.Value != "UKR"))
                    {
                        return WorkflowError.ErrorWithCode(4, new NSString("This does not seem to be a ukrainian passport."));
                    }

                    return null;

                }
                );


            SBSDKUIWorkflow workflow = new SBSDKUIWorkflow(new SBSDKUIWorkflowStep[] { frontSide }, "Ukrainian passport", null);
            return workflow;
        }

        static SBSDKUIWorkflow Payform()
        {
            SBSDKUIWorkflowStep payform = new SBSDKUIScanPayFormWorkflowStep(
                "Payform 1/1",
                "Please scan your SEPA payform.",
                false,
                (result) =>
                {
                    SBSDKPayFormRecognitionResult payformResult = result.PayformResult;
                    if (payformResult == null || payformResult.RecognizedFields.Length == 0)
                    {
                        return WorkflowError.ErrorWithCode(5, new NSString("No payform data detected."));
                    }
                    return null;
                }
                );


            SBSDKUIWorkflow workflow = new SBSDKUIWorkflow(new SBSDKUIWorkflowStep[] { payform }, "Payform", null);
            return workflow;
        }

        static SBSDKUIWorkflow DisabilityCertificate()
        {
            SBSDKPageAspectRatio[] ratios = {
                new SBSDKPageAspectRatio(1.0, 1.4143),
                new SBSDKPageAspectRatio(1.4143, 1.0),
                new SBSDKPageAspectRatio(1.0, 1.5715)
                };

            SBSDKUIWorkflowStep certificate = new SBSDKUIScanDisabilityCertificateWorkflowStep(
                "Disability Certificate 1/1",
                "Please scan your disability certificate.",
                ratios,
                true,
                (result) =>
                {
                    SBSDKDisabilityCertificatesRecognizerResult dc = result.DisabilityCertificateResult;
                    if (dc == null || !dc.RecognitionSuccessful)
                    {
                        return WorkflowError.ErrorWithCode(6, new NSString("This does not seem to be a valid certificate."));
                    }
                    return null;
                }
                );


            SBSDKUIWorkflow workflow = new SBSDKUIWorkflow(new SBSDKUIWorkflowStep[] { certificate }, "Disability Certificate", null);
            return workflow;
        }

        public static SBSDKUIWorkflowStep QrCodeStep()
        {

            return new SBSDKUIScanBarCodeWorkflowStep(
                "Scan your QR code",
                null,
                new String[] { AVConstants.AVMetadataObjectTypeQRCode.ToString() },
                new CGSize(1, 1),
                null
                );
        }

        public static SBSDKUIWorkflowStep DocumentStep()
        {
            return new SBSDKUIScanDocumentPageWorkflowStep(
                "QR code and Document 2/2",
                "Please scan a document.",
                null,
                null,
                null
                );
        }
    }
}

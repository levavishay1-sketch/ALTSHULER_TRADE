using System;
using System.Collections.Generic;

namespace Alt.DataModel.Crm.Core.Errors
{
    public static class CustomErrorCodes
    {
        private static Dictionary<int, string> errorMessages = new Dictionary<int, string>();

        public const int NotAllRequiredFieldsHaveBeenFilled = -100000000;
        public const int SystemParameterNotFound = -100000001;
        public const int CommonEmptyEnumerableRequiredFieldMessage = -100000002;
        public const int CommonRequiredFieldMessage = -100000003;
        public const int WebApiInvalidProperty = -100000004;
        public const int CommonMaxLengthValidationMessage = -100000005;
        public const int ApiNullableInput = -100000006;
        public const int XmlValidationModelNotFound = -100000007;
        public const int InvalidApiInput = -100000008;
        public const int InvalidTemplateType = -100000009;
        public const int InvalidRequest = -100000010;
        public const int TemplateNotExist = -100000011;
        public const int SchemanameRequired = -100000012;
        public const int SchemanameAndRegardingObjectNotMatched = -100000013;
        public const int PassedawayContactSendSmsErrorMessage = -100000014;
        public const int InvalidTraidLeadForQualify = -100000015;
        public const int InvalidTraidLeadForDisqualify = -100000016;
        public const int InternalServerError = -100000017;
        public const int InvalidResponseError = -100000018;
        public const int ApiConfigurationNotFound = -100000019;
        public const int EnvironmentVariableNotFound = -100000020;
        public const int MobilePhoneNotInWhiteList = -100000021;
        public const int InvalidStatusForSendToExternalService = -100000022;
        public const int FaildToParseEnvironmentVariableValue = -100000023;
        public const int EsbInvalidResponseError = -100000024;
        public const int ApiConfigurationMethodCodeNotDefind = -100000025;
        public const int InvalidHttpRequestMethod = -100000026;
        public const int InvalidCloseAsWonOpportunity = -100000027;
        public const int InvalidCloseAsLostOpportunity = -100000028;
        public const int BeneficiarySigningDeclarationError = -100000029;
        public const int MainAccountHolderError = -100000030;
        public const int EmptyDigitalFormVerification = -100000031;
        public const int AutomaticIncidentTemplateNotFound = -100000032;
        public const int IncidentOwnerNotDefined = -100000033;
        public const int OwnerSelectedIsNotTeam = -100000034;
        public const int CommonCantUpdateFieldMessage = -100000035;
        public const int CantAssignToTeam = -100000036;
        public const int DuplicatePortfolioOwnerError = -100000037;
        public const int SpousePortfolioOwnerError = -100000038;
        public const int BeneficiarySpousePortfolioOwnerError = -100000039;
        public const int AccountHolderToOpenInTradeOneNotFound = -100000040;
        public const int MultipleAccountHoldersToOpenInTradeOneError = -100000041;
        public const int LeadWithoutCustomerIdentityError = -100000042;
        public const int UnrecognizedApiCodeForDocument = -100000043;
        public const int ParserEntryPointSchemanameAndReferenceNotMatched = -100000044;
        public const int InvalidResponseContentError = -100000045;
        public const int DebugModeResponseContentError = -100000046;
        public const int InvalidEsbResultStatusError = -100000047;
        public const int SchedulerSetupNotFound = -100000048;
        public const int InvalidStatusForRunningScheduledOperation = -100000049;
        public const int ScheduledOperationAlreadyRunningError = -100000050;
        public const int PackageExecutionCompletedWithError = -100000051;
        public const int DataReceptionCompletedWithWarnings = -100000052;
        public const int OpenedInShenhavStatusWithoutPortfolio = -100000053;
        public const int OpportunityAlreadyClosedError = -100000054;
        public const int NotImplementedLogicForApiConfiguration = -100000055;
        public const int ApiIsInDebugMode = -100000056;
        public const int NotImplementedInterfaceError = -100000057;
        public const int NoImportFileReceivedError = -100000058;
        public const int FailedToGetImportFileError = -100000059;
        public const int ImportDataConfigurationsNotDefined = -100000060;
        public const int DataImportFailed = -100000061;
        public const int PdfProductionTemplateCodeNotFound = -100000062;
        public const int CustomerOperationTemplateNotFound = -100000063;
        public const int NotImplementedLogicForCustomerOperationTemplate = -100000064;
        public const int SearchSourceTypeCodeInvalid = -100000065;
        public const int PreservationStatusCodeRequired = -100000066;

        static CustomErrorCodes()
        {
            errorMessages.Add(NotAllRequiredFieldsHaveBeenFilled, "לא מילאו את כל השדות הנדרשים\n");
            errorMessages.Add(SystemParameterNotFound, "System parameter not found");
            errorMessages.Add(CommonEmptyEnumerableRequiredFieldMessage, "השדה {0} הוא שדה חובה ונשלח ריק");
            errorMessages.Add(CommonRequiredFieldMessage, "השדה {0} הוא שדה חובה");
            errorMessages.Add(WebApiInvalidProperty, "Recieved unexpected property: {0}");
            errorMessages.Add(CommonMaxLengthValidationMessage, "אורך השדה {0} גדול מהאורך המוגדר");
            errorMessages.Add(ApiNullableInput, "Nullable input");
            errorMessages.Add(XmlValidationModelNotFound, "Xml Validation Model Not Found In Api Configurations");
            errorMessages.Add(InvalidApiInput, "Invalid API input.");
            errorMessages.Add(InvalidTemplateType, "Invalid Template Type");
            errorMessages.Add(InvalidRequest, "בקשה לא תקינה התקבלה");
            errorMessages.Add(TemplateNotExist, "The selected template not exist");
            errorMessages.Add(SchemanameRequired, "Templae schemaname is a required field");
            errorMessages.Add(SchemanameAndRegardingObjectNotMatched, "The template schemaname and the regarding object type are different");
            errorMessages.Add(PassedawayContactSendSmsErrorMessage, "המסרון לא נשלח מאחר וללקוח קיים תאריך פטירה");
            errorMessages.Add(InvalidTraidLeadForQualify, "לא ניתן לאשר הפניה זו מכיוון שהיא מקושרת לתהליך הצטרפות דיגיטלי פעיל.");
            errorMessages.Add(InvalidTraidLeadForDisqualify, "לא ניתן לפסול הפניה זו מכיוון שהיא מקושרת לתהליך של טופס דיגיטלי פעיל. במידה ורוצים לבטל את ההפניה, יש להיכנס באמצעות הלינק לטופס הדיגיטלי ולבטל משם.");
            errorMessages.Add(InternalServerError, "Internal Server Error");
            errorMessages.Add(InvalidResponseError, "Invalid Response Error");
            errorMessages.Add(ApiConfigurationNotFound, "Api configuration not found.");
            errorMessages.Add(EnvironmentVariableNotFound, "{0} environment variable not found.");
            errorMessages.Add(MobilePhoneNotInWhiteList, "Send sms failed .Mobile phone {0} is not in white list");
            errorMessages.Add(InvalidStatusForSendToExternalService, "Attempting send to external service with status {0}.");
            errorMessages.Add(FaildToParseEnvironmentVariableValue, "Failed to parse {0} environment variable value.");
            errorMessages.Add(EsbInvalidResponseError, "Esb Invalid Response : {0}");
            errorMessages.Add(ApiConfigurationMethodCodeNotDefind, "ApiConfiguration MethodCode not defind");
            errorMessages.Add(InvalidHttpRequestMethod, "Invalid Http Request Method");
            errorMessages.Add(InvalidCloseAsWonOpportunity, "לא ניתן לאשר הזדמנות זו מכיוון שהיא מקושרת לתהליך של טופס דיגיטלי פעיל");
            errorMessages.Add(InvalidCloseAsLostOpportunity, string.Concat(
                  "לא ניתן לפסול הזדמנות זו מכיוון שהיא מקושרת לתהליך של טופס דיגיטלי פעיל."
                , Environment.NewLine
                , "במידה ורוצים לבטל את ההפניה, יש להיכנס באמצעות הלינק לטופס הדיגיטלי ולבטל משם"
                ));
            errorMessages.Add(BeneficiarySigningDeclarationError, "לפי ערך בהצהרה על נהנה חובה בעל חשבון נוסף מסוג נהנה.");
            errorMessages.Add(MainAccountHolderError, "לטופס ההצטרפות דיגיטלי חייב להיות בעל חשבון ראשי אחד.");
            errorMessages.Add(EmptyDigitalFormVerification, "טופס דיגיטלי בסטטוס נשלח לבקרה חייב לקבל פרטי הצטרפות.");
            errorMessages.Add(AutomaticIncidentTemplateNotFound, "Automatic Incident Template Not Found");
            errorMessages.Add(IncidentOwnerNotDefined, "Incident Owner Not Defined");
            errorMessages.Add(OwnerSelectedIsNotTeam, "הבעלים שנבחר מסוג משתמש לא יכולים להיות בעלים של {0}");
            errorMessages.Add(CommonCantUpdateFieldMessage, "לא ניתן לעדכן את שדה {0}");
            errorMessages.Add(CantAssignToTeam, "לא ניתן להקצות לצוות");
            errorMessages.Add(DuplicatePortfolioOwnerError, "התקבל יותר מבעל חשבון אחד עם מזהה {0}");
            errorMessages.Add(SpousePortfolioOwnerError, "לא התקבל בעל חשבון מסוג בעל חשבון לבן/בת זוג עם מזהה {0}");
            errorMessages.Add(BeneficiarySpousePortfolioOwnerError, "לא התקבל בעל חשבון מסוג נהנה לבן/בת זוג עם מזהה {0}");
            errorMessages.Add(AccountHolderToOpenInTradeOneNotFound, "לא נמצא בעל חשבון לפתיחת יוזר בטרייד 1 שעומד בתנאים");
            errorMessages.Add(MultipleAccountHoldersToOpenInTradeOneError, "נמצא יותר מאחד בעל חשבון לפתיחת יוזר בטרייד 1 שעומדים בתנאים");
            errorMessages.Add(LeadWithoutCustomerIdentityError, "לא ניתן לאשר הפניה ללא מספר מזהה לקוח.");
            errorMessages.Add(UnrecognizedApiCodeForDocument, "קוד API אינו קוד עבור מסמכים");
            errorMessages.Add(ParserEntryPointSchemanameAndReferenceNotMatched, "The parser custom entry point schemaname and the entry point object type are different");
            errorMessages.Add(InvalidResponseContentError, "Invalid Response Content");
            errorMessages.Add(DebugModeResponseContentError, "Debug Mode Response Content not Defined");
            errorMessages.Add(InvalidEsbResultStatusError, "Invalid Esb Result Status: ({0})");
            errorMessages.Add(SchedulerSetupNotFound, "לא נמצאה רשומת הגדרת תזמון");
            errorMessages.Add(InvalidStatusForRunningScheduledOperation, "Invalid Scheduled Operation Status ({0}) for Running!");
            errorMessages.Add(ScheduledOperationAlreadyRunningError, "Scheduled Operation Already Running!");
            errorMessages.Add(PackageExecutionCompletedWithError, "Package execution completed with error.");
            errorMessages.Add(DataReceptionCompletedWithWarnings, "Data reception completed with warnings.");
            errorMessages.Add(OpenedInShenhavStatusWithoutPortfolio, "Opened in Shenhav Status without Portfolio.");
            errorMessages.Add(OpportunityAlreadyClosedError, "Opportunity already closed.");
            errorMessages.Add(NotImplementedLogicForApiConfiguration, "Not Implemented Logic for ApiConfigurationCode ({0}).");
            errorMessages.Add(ApiIsInDebugMode, "!!!Api is in DebugMode!!!");
            errorMessages.Add(NotImplementedInterfaceError, "Not implemented interface for {0}.");
            errorMessages.Add(NoImportFileReceivedError, "No Import File Received.");
            errorMessages.Add(FailedToGetImportFileError, "Failed to Get Import File : {0}");
            errorMessages.Add(ImportDataConfigurationsNotDefined, "Import Data Configurations Not Defined");
            errorMessages.Add(DataImportFailed, "Data Import Failed");
            errorMessages.Add(PdfProductionTemplateCodeNotFound, "PDF Production Template Code not Found");
            errorMessages.Add(CustomerOperationTemplateNotFound, "Customer Operation Template not Found");
            errorMessages.Add(NotImplementedLogicForCustomerOperationTemplate, "Not Implemented Logic For Customer Operation Template");
            errorMessages.Add(SearchSourceTypeCodeInvalid, "The Search Source Code Provided Is Invalid");
            errorMessages.Add(PreservationStatusCodeRequired, "על מנת לפתור את האירוע יש לעדכן \"סטטוס שימור\" באירוע");
        }

        public static string GetErrorMessage(int errorCode)
        {
            string errorMessage = string.Empty;
            if (!errorMessages.TryGetValue(errorCode, out errorMessage))
            {
                errorMessage = "Supplied Error Key Not Found";
            }

            return errorMessage;
        }

        public static bool ContainsCode(int errorCode)
        {
            return errorMessages.ContainsKey(errorCode);
        }
    }
}

var AccountHolderRibbonBL = (function () {

    let formContext;

    const EMPTY_MOBILE_PHONE_ERROR_MESSAGE = 'יש להשלים מספר טלפון נייד, לצורך ביצוע אימות ב- SMS';
    const EMPTY_EMAIL_ERROR_MESSAGE = 'יש להשלים כתובת דוא"ל, לצורך ביצוע אימות בדוא"ל';
    const DRIVING_LICENSE_ERROR_MESSAGE = 'שים לב, לא ניתן לפנות';

    const identificationTypeCode = {
        DrivingLicense: 4,
    };

    const sendOTPViaSmsOnClick = function (primaryControl) {

        formContext = primaryControl;
        const to = formContext.getAttribute('alt_mobilephone').getValue();
        sendOTPCode(activityTemplateType.Sms, to)
    };

    const sendOTPViaEmailOnClick = function (primaryControl) {

        formContext = primaryControl;
        const to = formContext.getAttribute('alt_email').getValue();
        sendOTPCode(activityTemplateType.Email, to)
    };

    const populationRegisterValidationRequestOnClick = function (primaryControl) {

        formContext = primaryControl;
        createPopulationRegisterValidationRequest();
    };

    const createPopulationRegisterValidationRequest = function () {
        const customer = formContext.getAttribute('alt_customerid').getValue()
            && formContext.getAttribute('alt_customerid').getValue()[0];
        let dto = {
            customer: customer,
            identityNumber: formContext.getAttribute('alt_identificationnumber').getValue(),
            birthdate: formContext.getAttribute('alt_birthdate').getValue(),
        };

        if (formContext.getAttribute('alt_identificationtypecode').getValue() != identificationTypeCode.DrivingLicense) {
            dto.idissuanceDate = formContext.getAttribute('alt_idissuedate').getValue();
        }
        let select;
        let retrieveEntityLogicalName;
        let relatedRecord;
        if (formContext.getAttribute('alt_portfolioid').getValue()) {
            select = 'alt_joiningprocessnumber';
            retrieveEntityLogicalName = entityName.Portfolio;
            relatedRecord = formContext.getAttribute('alt_portfolioid').getValue()[0];
        }
        else if (formContext.getAttribute('alt_digitalformverificationid').getValue()) {
            select = "alt_digitalformnumber";
            retrieveEntityLogicalName = entityName.DigitalFormVerification;
            relatedRecord = formContext.getAttribute('alt_digitalformverificationid').getValue()[0];
        }
        if (select && retrieveEntityLogicalName) {
            Utils.Server.Retrieve(retrieveEntityLogicalName, relatedRecord.id, select, null, function (result) {

                if (result && result[select]) {
                    dto.joiningProcessNumber = result[select];
                }
                CustomerActivitiesCommonBL.OpenPopulationRegistryCustomerVerificationForm(formContext, dto, true, true);
            }, null);
        }
        else {
            CustomerActivitiesCommonBL.OpenPopulationRegistryCustomerVerificationForm(formContext, dto, true, true);
        }
    };

    const isPopulationRegisterValidationButtonEnabled = function (primaryControl) {

        formContext = primaryControl;
        const isEnabled = formContext.getAttribute('alt_customerid').getValue()
            && formContext.getAttribute('alt_customerid').getValue()[0] ? true : false;
        return isEnabled;
    };

    const sendOTPCode = function (templateType, to) {
        if (formContext.data.entity.getIsDirty()) {
            formContext.data.save().then(function () {
                sendOTPCodeBySmsOrEmail(templateType, to);
            });
        }
        else {
            sendOTPCodeBySmsOrEmail(templateType, to);
        }
    };

    const sendOTPCodeBySmsOrEmail = function (templateType, to) {

        if (to) {
            const regardingObjectId = formContext.getAttribute('alt_portfolioid').getValue()[0];
            const customerId = formContext.getAttribute('alt_customerid').getValue()[0];
            const parserCustomEntryPoint = Utils.Global.GenerateParserCustomEntryPointEntityReference(formContext.data.entity.getEntityName(), formContext.data.entity.getId());
            CustomerActivitiesCommonBL.SendOTPCode(formContext, templateType, to, regardingObjectId, parserCustomEntryPoint, customerId);
        }
        else {
            const errorMessage = templateType == activityTemplateType.Sms ?
                EMPTY_MOBILE_PHONE_ERROR_MESSAGE : EMPTY_EMAIL_ERROR_MESSAGE;
            Xrm.Navigation.openAlertDialog({ text: errorMessage });
        }
    };

    const sendOTPEnableRule = function (primaryControl) {
        formContext = primaryControl;
        return true;
    };

    return {
        SendOTPViaSmsOnClick: sendOTPViaSmsOnClick,
        SendOTPViaEmailOnClick: sendOTPViaEmailOnClick,
        SendOTPEnableRule: sendOTPEnableRule,
        IsPopulationRegisterValidationButtonEnabled: isPopulationRegisterValidationButtonEnabled,
        PopulationRegisterValidationRequestOnClick: populationRegisterValidationRequestOnClick
    };

}());
var AccountHolderCommonBL = (function () {

    const CREATE_FORBIDDEN_ALERT_TEXT = "לא ניתן ליצור רשומת בעל חשבון. ניתן ליצור בעל חשבון מסוג נהנה בלבד רק מתוך בקרת הצטרפות";

    const formAttributes = {
        alt_digitalformverificationid: 'alt_digitalformverificationid',
        statuscode: 'statuscode',
        alt_identificationtypecode: 'alt_identificationtypecode',
        alt_identificationnumber: 'alt_identificationnumber',
        alt_clubmembershipeligibilitycode: 'alt_clubmembershipeligibilitycode'
    };

    const identificationTypeCode = {
        GovernmentId: 1
    };

    let formContext;
    let legalityCreationSettings =
        [
            {
                attributeName: formAttributes.alt_digitalformverificationid,
                conditionCallback: null,
                errorMessage: CREATE_FORBIDDEN_ALERT_TEXT
            }
        ];

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();

        const formType = formContext.ui.getFormType();
        const crmFormTypes = Utils.CrmPage.FormType;
        switch (formType) {
            case crmFormTypes.Create: {
                Utils.CrmPage.HandleRecordLegalityCreation(formContext, legalityCreationSettings, legalFormSuccessCallback);
                initOnChange();
                break;
            }
            case crmFormTypes.Update: {
                initOnChange();
                break;
            }
            default: {
                break;
            }
        }

        disableClubmembershipEligibilityCode();
    };

    const initOnChange = function () {
        formContext.getAttribute(formAttributes.alt_identificationnumber).addOnChange(identificationNumberOnChange);
    };

    const identificationNumberOnChange = function () {

        validateIdentificationNumber();
    };

    const validateIdentificationNumber = function () {
        if (formContext.getAttribute(formAttributes.alt_identificationtypecode).getValue() == identificationTypeCode.GovernmentId
            && formContext.getAttribute(formAttributes.alt_identificationnumber).getValue()) {
            Utils.CrmPage.HandleGovIdAttribute(formContext, formAttributes.alt_identificationnumber);
        }
    };

    const legalFormSuccessCallback = function () {

        initOnChange();
    };
 
    const disableClubmembershipEligibilityCode = function () {

        let clubmembershipEligibilityCodeControl = formContext.getControl(formAttributes.alt_clubmembershipeligibilitycode);
        if (clubmembershipEligibilityCodeControl) {
            Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.alt_clubmembershipeligibilitycode, true);
        }
    };

    return {
        OnLoad: onLoad,
        ValidateIdentificationNumber: validateIdentificationNumber
    }
})();
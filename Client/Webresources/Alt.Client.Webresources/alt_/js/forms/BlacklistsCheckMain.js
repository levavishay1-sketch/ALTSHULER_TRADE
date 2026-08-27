/// <reference path="../utils/Utils.Validators.js" />
/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Enums.js" />
/// <reference path="../utils/Utils.JsExtantions.js" />
/// <reference path="../forms/BlacklistsCheckCommonBL.js" />

var BlacklistsCheckMain = (function () {

    const CHECK_REQUEST_NOTIFICATION_MESSAGE = 'מתבצעת בקשה לבדיקה מול רשימות שחורות...'
    const blacklistsCheckStatusCode = {
        Failed: 157350001,
        Sending: 157350003
    };

    let formAttributes;
    let formContext;

    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();

        if (Utils.CrmPage.IsFirstLoad()) {

            formAttributes = BlacklistsCheckCommonBL.FormAttributes;
            BlacklistsCheckCommonBL.OnLoad(executionContext);
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;

            switch (formType) {

                case crmFormTypes.Disable: {
                    handleFailureDetails();
                    break;
                }
                default:
                    break;
            }
        }
        else {
            refreshForm();
        }
    };

    const onSave = function (executionContext) {

        formContext = executionContext.getFormContext();
        if (formContext.ui.getFormType() == Utils.CrmPage.FormType.Create) {
            Xrm.Utility.showProgressIndicator(CHECK_REQUEST_NOTIFICATION_MESSAGE);
        }
    };

    const refreshForm = function () {

        formContext.data.refresh(false);
        var statusCodeValue = formContext.getAttribute(formAttributes.statuscode).getValue();
        if (statusCodeValue !== blacklistsCheckStatusCode.Sending) {

            Xrm.Utility.closeProgressIndicator();
            if (statusCodeValue == Failed.Faild) {

                Xrm.Navigation.openAlertDialog({ text: 'בדיקה מול רשימות שחורות נכשלה.' });
            }
            return;
        }
        setTimeout(refreshForm, 2000);
    };

    const handleFailureDetails = function () {

        if (formContext.getAttribute(formAttributes.statuscode).getValue() == blacklistsCheckStatusCode.Failed) {
            Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_failuredetails, true);
        }
    };

    return {
        OnLoad: onLoad,
        OnSave: onSave
    };
})();
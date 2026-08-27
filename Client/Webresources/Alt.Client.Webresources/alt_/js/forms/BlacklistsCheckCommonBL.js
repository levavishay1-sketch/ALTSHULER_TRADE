/// <reference path="../utils/Utils.CrmPage.js" />

var BlacklistsCheckCommonBL = (function () {

    const formAttributes = {
        statuscode: 'statuscode',
        alt_identitynumber: 'alt_identitynumber',
        alt_failuredetails: 'alt_failuredetails'
    };

    let formContext;

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();

        const formType = formContext.ui.getFormType();
        const crmFormTypes = Utils.CrmPage.FormType;
        switch (formType) {
            case crmFormTypes.Create:
            case crmFormTypes.Update: {
                initOnChange();
                break;
            }
            default: {
                break;
            }
        }
    };

    const initOnChange = function () {
        formContext.getAttribute(formAttributes.alt_identitynumber).addOnChange(identityNumberOnChange);
    };

    const identityNumberOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        Utils.CrmPage.HandleGovIdAttribute(formContext, formAttributes.alt_identitynumber);
    };

    return {
        OnLoad: onLoad,
        FormAttributes: formAttributes
    };
})();
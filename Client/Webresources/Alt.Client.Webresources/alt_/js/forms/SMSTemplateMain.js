/// <reference path="../utils/Utils.CrmPage.js" />

var SMSTemplateMain = (function () {

    const formAttributes = {
        alt_schemaname: 'alt_schemaname'
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
            default:
                break;
        }
    };

    const initOnChange = function () {
       // formContext.getAttribute(formAttributes.alt_schemaname).addOnChange(schemaNameOnChange);
    };

    const schemaNameOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        Utils.CrmPage.HandleOnlyEnglishLettersInLowcase(formContext, formAttributes.alt_schemaname);
    };

    return {
        OnLoad: onLoad
    };
})();
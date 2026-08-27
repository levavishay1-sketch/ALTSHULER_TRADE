/// <reference path="../utils/Utils.JsExtantions.js" />
/// <reference path="../utils/Utils.CrmPage.js" />

var PhoneCallMain = (function () {

    var yesNoCode = {
        Yes: 1,
        No: 2
    };

    const callStatusCodes = {
        CustomerNotInterested: 5
    };

    let formContext;
    let formAttributes = {
        alt_callbackrequiredcode: 'alt_callbackrequiredcode',
        alt_callbackdate: 'alt_callbackdate',
        alt_statuscode: 'alt_statuscode',
        alt_disqualificationreasoncode: 'alt_disqualificationreasoncode',
        regardingobjectid: 'regardingobjectid'
    };

    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();
        if (Utils.CrmPage.IsFirstLoad()) {

            const crmFormTypes = Utils.CrmPage.FormType;
            const formType = formContext.ui.getFormType();
            switch (formType) {
                case crmFormTypes.Create: {

                    initFormUI();
                    initOnChange();
                    break;
                }
                case crmFormTypes.Update: {

                    initFormUIUpdate();
                    initOnChange();
                    break;
                }
                default: {
                    initFormUIUpdate();
                    break;
                }
            }
        }
    };

    const onSave = function (executionContext) {

        formContext = executionContext.getFormContext();
    };

    const initFormUI = function () {

        setDisqualificationReasonCodeRequirementAndVisibility();
    };

    const initFormUIUpdate = function () {

        handleUIByCallStatusCode();
        handleUIByCallBackRequiredCode();
    };

    const initOnChange = function () {

        formContext.getAttribute(formAttributes.alt_statuscode).addOnChange(handleUIByCallStatusCode);
        formContext.getAttribute(formAttributes.alt_callbackrequiredcode).addOnChange(handleUIByCallBackRequiredCode);
    };

    const handleUIByCallStatusCode = function () {

        const regardingObject = formContext.getAttribute(formAttributes.regardingobjectid).getValue();
        const name = regardingObject[0].entityType;
        const callStatusCode = formContext.getAttribute(formAttributes.alt_statuscode).getValue();

        if (callStatusCode === callStatusCodes.CustomerNotInterested) {
            setCallbackRequiredCodeRequirementAndVisibility(false);
            setCallbackDateRequirementAndVisibility(false);
            if (name === "lead") {
                setDisqualificationReasonCodeRequirementAndVisibility(true);
            }
            else {
                setDisqualificationReasonCodeRequirementAndVisibility(false);
            }
        }
        else if (callStatusCode != null && callStatusCode !== callStatusCodes.CustomerNotInterested) {
            setDisqualificationReasonCodeRequirementAndVisibility(false);
            setCallbackRequiredCodeRequirementAndVisibility(true);
        }
        else {
            setDisqualificationReasonCodeRequirementAndVisibility(false);
            setCallbackRequiredCodeRequirementAndVisibility(false);
            setCallbackDateRequirementAndVisibility(false);
        }
    };

    const handleUIByCallBackRequiredCode = function () {

        const callBackRequiredCode = formContext.getAttribute(formAttributes.alt_callbackrequiredcode).getValue();
        setCallbackDateRequirementAndVisibility(callBackRequiredCode == yesNoCode.Yes ? true : false);
    };

    const setDisqualificationReasonCodeRequirementAndVisibility = function (isVisible) {

        const requiredLevel = isVisible ? Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.None;

        Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_disqualificationreasoncode, isVisible);
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_disqualificationreasoncode, requiredLevel);

        if (!isVisible) {
            formContext.getAttribute(formAttributes.alt_disqualificationreasoncode).setValue(null);
        }
    };

    const setCallbackRequiredCodeRequirementAndVisibility = function (isVisible) {

        const requiredLevel = isVisible ? Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.None;

        Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_callbackrequiredcode, isVisible);
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_callbackrequiredcode, requiredLevel);

        if (!isVisible) {
            formContext.getAttribute(formAttributes.alt_callbackrequiredcode).setValue(null);
        }
    };

    const setCallbackDateRequirementAndVisibility = function (isVisible) {

        const requiredLevel = isVisible ? Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.None;

        Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_callbackdate, isVisible);
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_callbackdate, requiredLevel);

        if (!isVisible) {
            formContext.getAttribute(formAttributes.alt_callbackdate).setValue(null);
        }
    };

    return {
        OnLoad: onLoad,
        OnSave: onSave
    }
})();
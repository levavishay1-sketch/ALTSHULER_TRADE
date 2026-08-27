/// <reference path="../utils/Utils.Enums.js" />
/// <reference path="../utils/Utils.CrmPage.js" />

var EmailTemplateMainBL = (function () {

    let formContext;
    let requirementLevel;

    const formAttributes = {
        alt_sendfromcode: 'alt_sendfromcode',
        alt_userdisplaybit: 'alt_userdisplaybit',
        alt_fromqueueid: 'alt_fromqueueid',
        alt_fromteamid: 'alt_fromteamid',
        alt_schemaname: 'alt_schemaname'
    };
    const sendFromCode =
    {
        Queue: 100000000,
        Team: 100000001,
        User: 100000002
    };

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();
        requirementLevel = Utils.CrmPage.RequirementLevel;

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
        initFormUI();
    };

    const initFormUI = function () {
        handleUIByUserDisplayBit();
        handleUIBySendFromCode();
    };

    const initOnChange = function () {
        formContext.getAttribute(formAttributes.alt_sendfromcode).addOnChange(sendFromCodeOnChange);
        formContext.getAttribute(formAttributes.alt_userdisplaybit).addOnChange(userDisplayBitOnChange);
        //formContext.getAttribute(formAttributes.alt_schemaname).addOnChange(schemaNameOnChange);
    };

    const sendFromCodeOnChange = function () {
        handleUIBySendFromCode();
    };

    const userDisplayBitOnChange = function () {
        setSendFromCodeValue();
        handleUIByUserDisplayBit();
        handleUIBySendFromCode();
    };

    const schemaNameOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        Utils.CrmPage.HandleOnlyEnglishLettersInLowcase(formContext, formAttributes.alt_schemaname);
    };

    const setSendFromCodeValue = function () {

        let userDisplayBitValue = formContext.getAttribute(formAttributes.alt_userdisplaybit).getValue() === true ? true : false;
        if (!userDisplayBitValue) {
            let sendFromCodeValue = formContext.getAttribute(formAttributes.alt_sendfromcode).getValue();
            if (sendFromCodeValue !== sendFromCode.Queue) {
                formContext.getAttribute(formAttributes.alt_sendfromcode).setValue(sendFromCode.Queue);
            }
        }
    };

    const handleUIByUserDisplayBit = function () {
        let userDisplayBitValue = formContext.getAttribute(formAttributes.alt_userdisplaybit).getValue();
        let fromQueueRequiredLevel = userDisplayBitValue ?
            requirementLevel.Required : requirementLevel.None;
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_sendfromcode, fromQueueRequiredLevel);
        Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.alt_sendfromcode, !userDisplayBitValue);
    };

    const handleUIBySendFromCode = function () {

        let fromQueueVisibleMode = false;
        let fromTeamVisibileMode = false;
        const sendFromCodeValue = formContext.getAttribute(formAttributes.alt_sendfromcode).getValue();
        switch (sendFromCodeValue) {
            case sendFromCode.Queue: {
                fromQueueVisibleMode = true;
                break;
            }
            case sendFromCode.Team: {
                fromTeamVisibileMode = true;
                break;
            }
            default:
                break;
        };
        Utils.CrmPage.HandleControlsVisibleMode(formContext, [formAttributes.alt_fromqueueid], fromQueueVisibleMode);
        Utils.CrmPage.HandleControlsVisibleMode(formContext, [formAttributes.alt_fromteamid], fromTeamVisibileMode);

        let fromQueueRequiredLevel = fromQueueVisibleMode == true ? requirementLevel.Required : requirementLevel.None;
        let fromTeamRequiredLevel = fromTeamVisibileMode == true ? requirementLevel.Required : requirementLevel.None
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_fromqueueid, fromQueueRequiredLevel);
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_fromteamid, fromTeamRequiredLevel);
    };

    return {
        OnLoad: onLoad
    };
})();
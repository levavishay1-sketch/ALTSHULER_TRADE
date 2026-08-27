/// <reference path="../utils/Utils.Validators.js" />
/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Enums.js" />
/// <reference path="../utils/Utils.JsExtantions.js" />
/// <reference path="../utils/Utils.Server.js" />

var PhoneCallQuickCreate = (function () {

    const yesNoCode = {
        Yes: 1,
        No: 2
    };

    const callStatusCodes = {
        CustomerNotInterested: 5
    };

    const regardingEntitiesToPhoneNumberMapping = {
        lead: {
            phoneField: "mobilephone"
        },
        opportunity: {
            phoneField: "alt_mobilephone"
        },
        alt_digitalformverification: {
            lookupField: "alt_opportunityid",
            relatedEntity: "opportunity",
            phoneField: "alt_mobilephone"
        },
        alt_portfolio: {
            phoneField: "alt_mobilephone"
        },
        incident: {
            lookupField: "alt_portfolioid",
            relatedEntity: "alt_portfolio",
            phoneField: "alt_mobilephone"
        }
    };

    let formContext;

    let formAttributes = {
        alt_callbackrequiredcode: 'alt_callbackrequiredcode',
        alt_callbackdate: 'alt_callbackdate',
        alt_statuscode: 'alt_statuscode',
        regardingobjectid: 'regardingobjectid',
        phonenumber: 'phonenumber',
        alt_disqualificationreasoncode: 'alt_disqualificationreasoncode',
        alt_completeactivitybit: 'alt_completeactivitybit',
        scheduledend: 'scheduledend',
        to: "to"
    };

    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();
        initOnChange();
        handleMappingFromAssignToMeButton();
    };

    const handleMappingFromAssignToMeButton = function () {

        const parameters =
            Xrm.Utility.getGlobalContext().getQueryStringParameters();

        console.log(parameters);

        if (parameters.OpenedFromLeadButton === "true") {

            const regarding = formContext
                .getAttribute(formAttributes.regardingobjectid)
                ?.getValue();

            if (regarding && regarding[0].entityType === "lead") {
                formContext.getAttribute(formAttributes.to)?.setValue([{
                    id: regarding[0].id,
                    entityType: "lead"
                }]);
            }
        }
    };


    const initOnChange = function () {

        formContext.getAttribute(formAttributes.alt_statuscode).addOnChange(handleUIByCallStatusCode);
        formContext.getAttribute(formAttributes.alt_callbackrequiredcode).addOnChange(handleUIByCallBackRequiredCode);
        handleMobilePhoneNumberByRegardingEntity();
    };

    const handleUIByCallStatusCode = function () {

        const regardingObject = formContext.getAttribute(formAttributes.regardingobjectid).getValue();
        const name = regardingObject[0].entityType;
        const callStatusCode = formContext.getAttribute(formAttributes.alt_statuscode).getValue();

        handleCompleteActivityBit(callStatusCode == null ? false : true);

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

    const handleMobilePhoneNumberByRegardingEntity = function () {

        const regardingObject = formContext.getAttribute(formAttributes.regardingobjectid).getValue();
        const name = regardingObject[0].entityType;
        const id = Utils.JsExtantions.String.RemoveBraces(regardingObject[0].id);

        const entityConfig = regardingEntitiesToPhoneNumberMapping[name];

        if (!entityConfig) {
            return;
        }

        if (!entityConfig.lookupField) {
            Utils.Server.Retrieve(name, id, entityConfig.phoneField, null,
                (result) => {
                    if (result) {
                        formContext.getAttribute(formAttributes.phonenumber).setValue(result[entityConfig.phoneField]);
                    }
                },
                (error) => {
                    console.log(error);
                }
            );
        }
        else {
            Utils.Server.Retrieve(name, id, `_${entityConfig.lookupField}_value`, null,
                (result) => {
                    if (result) {
                        const relatedRecordId = result[entityConfig.lookupField].Id;
                        if (relatedRecordId) {
                            Utils.Server.Retrieve(entityConfig.relatedEntity, relatedRecordId, entityConfig.phoneField, null,
                                (result2) => {
                                    if (result2) {
                                        formContext.getAttribute(formAttributes.phonenumber).setValue(result2[entityConfig.phoneField]);
                                    }
                                },
                                (error2) => {
                                    console.log(error2);
                                }
                            );
                        }
                    }
                },
                (error) => {
                    console.log(error);
                }
            );
        }
    };

    const handleCompleteActivityBit = function (complete) {

        formContext.getAttribute(formAttributes.alt_completeactivitybit).setValue(complete);
    };

    return {
        OnLoad: onLoad
    };
})();
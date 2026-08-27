var DigitalFormMain = (function () {

    const CREATE_FORBIDDEN_ALERT_TEXT = 'לא ניתן ליצור טופס ';
    const CREATE_FROM_LEAD_FORBIDDEN_ALERT_TEXT = 'לא ניתן ליצור טופס דיגיטלי נוסף להפניה  ';
    const CREATE_FROM_INACTIVE_LEAD_FORBIDDEN_ALERT_TEXT = 'לא ניתן ליצור טופס דיגיטלי להפניה לא פעילה';
    const CREATE_FROM_OPPORTUNITY_FORBIDDEN_ALERT_TEXT = 'לא ניתן ליצור טופס דיגיטלי מתוך הזדמנות';

    const formAttributes = {
        RegardingObjectId: 'regardingobjectid',
        alt_DigitalFormLink: 'alt_digitalformlink',
        StatusCode: 'statuscode',
        alt_TransferToOutSystemStatusCode: 'alt_transfertooutsystemstatuscode',
        alt_DigitalFormTypeCode: 'alt_digitalformtypecode',
        alt_DigitalFormStatusId: 'alt_digitalformstatusid'
    };

    let formContext;
    let legalityCreationSettings =
        [
            {
                attributeName: 'regardingobjectid',
                conditionCallback: null,
                errorMessage: CREATE_FORBIDDEN_ALERT_TEXT
            }
        ];

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();
        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;
            switch (formType) {
                case crmFormTypes.Create: {
                    handleLegalityCreation();
                    break;
                }
                case crmFormTypes.Update: {
                    addToOnPostSave();
                    initOnChange();
                    break;
                }
                default: {
                    break;
                }
            }
            addDigitalFormUrlQueryStringParameters();
        }
        else {
            refreshForm();
        }
    };

    const onSave = function (executionContext) {

        formContext = executionContext.getFormContext();
        if (formContext.ui.getFormType() == Utils.CrmPage.FormType.Create) {
            Xrm.Utility.showProgressIndicator(LINK_REQUEST_NOTIFICATION_MESSAGE);
        }
    };

    const addToOnPostSave = function () {

        formContext.data.entity.addOnPostSave(addDigitalFormUrlQueryStringParameters);
    }

    const initOnChange = function () {

        formContext.getAttribute(formAttributes.alt_TransferToOutSystemStatusCode).addOnChange(transferToOutSystemStatusCodeOnChange);
    };

    const handleFormUI = function () {
        initOnChange();
    };

    const transferToOutSystemStatusCodeOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        if (formContext.getAttribute(formAttributes.alt_TransferToOutSystemStatusCode).getValue() == transferStatusCode.Sent) {

            addDigitalFormUrlQueryStringParameters();
        }
    };

    const handleLegalityCreation = function () {

        const regardingObject = formContext.getAttribute(formAttributes.RegardingObjectId).getValue();
        if (regardingObject !== null) {
            let entityType = regardingObject[0].entityType;
            switch (entityType) {
                case entityName.Lead: {
                    handleLegalityCreationFromLead(regardingObject[0].id, regardingObject[0].entityType);
                    break;
                }
                case entityName.Opportunity: {
                    handleLegalityCreationFromOpportunity();
                    break;
                }
                default: {
                    break;
                }
            }
        }
        else {
            Utils.CrmPage.HandleRecordLegalityCreation(formContext, legalityCreationSettings, legalFormSuccessCallback);
        }
    };

    const handleLegalityCreationFromLead = function (leadId, entityName) {
        checkIfLeadIsInactive(leadId, entityName, function (isInactive) {
            if (isInactive) {
                const inactiveLeadSetting = [
                    {
                        conditionCallback: function () { return false; },
                        errorMessage: CREATE_FROM_INACTIVE_LEAD_FORBIDDEN_ALERT_TEXT
                    }
                ];

                Utils.CrmPage.HandleRecordLegalityCreation(formContext, inactiveLeadSetting, legalFormSuccessCallback);
                return;
            }
            proceedWithExistingDigitalFormCheck(leadId);
        });
    };

    const handleLegalityCreationFromOpportunity = function () {
        const legalityCreationFromOpportunitySetting =
            [
                {
                    conditionCallback: function () { return false; },
                    errorMessage: CREATE_FROM_OPPORTUNITY_FORBIDDEN_ALERT_TEXT
                }
            ];
        Utils.CrmPage.HandleRecordLegalityCreation(formContext, legalityCreationFromOpportunitySetting, legalFormSuccessCallback);
    };

    const checkIfLeadIsInactive = function (leadId, entityName, callback) {

        const id = Utils.JsExtantions.String.RemoveBraces(leadId);
        Xrm.WebApi.retrieveRecord(entityName, id, "?$select=statecode")
            .then(function (lead) {
                const isInactive = lead.statecode !== 0;
                callback(isInactive);
            })
            .catch(function (error) {
                console.error("Failed to check Lead statecode", error);
                callback(false);
            });
    };

    const proceedWithExistingDigitalFormCheck = function (leadId) {

        let entityName = formContext.data.entity.getEntityName();

        Utils.Server.GetObjectTypeCodeByEntityName(entityName, function (metadataResult) {
            let objectTypeCode = metadataResult && metadataResult[0] && metadataResult[0].ObjectTypeCode;

            getDigitalFormsByRegardingObject(leadId, objectTypeCode, function (result) {
                const legalityCreationFromLeadSetting = [
                    {
                        conditionCallback: function () { return result == null },
                        errorMessage: CREATE_FROM_LEAD_FORBIDDEN_ALERT_TEXT
                    }
                ];

                Utils.CrmPage.HandleRecordLegalityCreation(formContext, legalityCreationFromLeadSetting, legalFormSuccessCallback);
            });
        });
    };

    const legalFormSuccessCallback = function () {
        handleFormUI();
    };

    const getDigitalFormsByRegardingObject = function (regardingObjectid, objectTypeCode, successCallBack, errorCallback) {

        var fetchData = {
            "activitytypecode": objectTypeCode,
            "regardingobjectid": regardingObjectid
        };
        var fetchXml = [
            "<fetch top='1'>",
            "<entity name='activitypointer'>",
            "<attribute name='activitytypecode'/>",
            "<attribute name='regardingobjectid'/>",
            "<filter>",
            "<condition attribute='activitytypecode' operator='eq' value='", fetchData.activitytypecode, "'/>",
            "</filter>",
            "<filter>",
            "<condition attribute='regardingobjectid' operator='eq' value='", fetchData.regardingobjectid, "'/>",
            "</filter>",
            "</entity>",
            "</fetch>"
        ].join("");

        Utils.Server.Fetch("activitypointer", fetchXml, successCallBack, errorCallback);
    };

    const refreshForm = function () {
        formContext.data.refresh(false);
        var statusCodeValue = formContext.getAttribute(formAttributes.alt_TransferToOutSystemStatusCode).getValue();
        if (statusCodeValue !== transferStatusCode.Sending) {
            Xrm.Utility.closeProgressIndicator();
            if (statusCodeValue == transferStatusCode.Faild) {
                Xrm.Navigation.openAlertDialog({ text: 'בקשה לקבלת לינק לטופס דיגיטלי נכשלה.' });
                formContext.ui.refreshRibbon();
            }
            else if (statusCodeValue == transferStatusCode.Sent) {
                var digitalFormStatusId = formContext.getAttribute(formAttributes.alt_DigitalFormStatusId).getValue();
                var guid = Utils.JsExtantions.String.RemoveBraces(digitalFormStatusId[0].id);
                Xrm.WebApi.retrieveRecord(digitalFormStatusId[0].entityType, guid, "?$select=alt_code").then(
                    (status) => {
                        Utils.Global.GetGlobalParamValue('DigitalFormDuplicateStatusCode',
                            (param) => {
                                if (status.alt_code == param) {
                                    Xrm.Utility.closeProgressIndicator();
                                    Xrm.Navigation.openAlertDialog({ text: 'קיים ללקוח תהליך פעיל, יש לאתר הפניה פעילה.' });
                                    formContext.ui.refreshRibbon();
                                }
                            },
                            (error) => {
                                console.log(error);
                            })
                    },
                    (error) => {
                        console.log(error);
                    }
                )
            }
            return;
        }
        setTimeout(refreshForm, 2000);
    };

    const addDigitalFormUrlQueryStringParameters = function () {

        Utils.Global.AddQueryStringParamsToJoiningFormURL(formContext, formAttributes.alt_DigitalFormLink);
    };

    return {
        onLoad: onLoad,
        onSave: onSave
    }
})();
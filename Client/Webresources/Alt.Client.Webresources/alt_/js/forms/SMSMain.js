/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Server.js" />
/// <reference path="ActivityTemplateCommonBL.js" />

var SMSMain = (function () {

    const formAttributes = {
        alt_smstemplateid: 'alt_smstemplateid',
        alt_contactid: 'alt_contactid',
        alt_mobilephone: 'alt_mobilephone',
        regardingobjectid: 'regardingobjectid',
        alt_parsercustomentrypoint: 'alt_parsercustomentrypoint'
    };

    const relatedRegardingAttributesToMapp =
        [
            { entityName: "lead", attributes: [{ from: 'parentcontactid', to: 'alt_contactid' }, { from: 'mobilephone', to: 'alt_mobilephone' }] },
            { entityName: "opportunity", attributes: [{ from: 'parentcontactid', to: 'alt_contactid' }, { from: 'alt_mobilephone', to: 'alt_mobilephone' }] },
            {
                entityName: "contact",
                attributes: [
                    { from: 'mobilephone', to: 'alt_mobilephone' }
                ]
            },
            {
                entityName: "incident",
                relatedAttribute: 'alt_portfolioid',
                relatedAttributeValue: null,
                targetEntity: 'alt_accountholder',
                targetEntityCustomerAttributeName: 'alt_customerid',
                attributes: [
                    { from: 'customerid', to: 'alt_contactid', fireOnChange: true, disabled: true }
                ],
                targetEntityAttributes: [
                    { from: 'alt_mobilephone', to: 'alt_mobilephone' }
                ]
            },
            {
                entityName: ['alt_portfolio', 'alt_digitalformverification'],
                filterContact: true,
                targetEntity: 'alt_accountholder',
                targetEntityCustomerAttributeName: 'alt_customerid',
                targetEntityAttributes: [
                    { from: 'alt_mobilephone', to: 'alt_mobilephone' }
                ]
            }
        ];
    let formContext;
    let mappingSettings;

    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();

        if (Utils.CrmPage.IsFirstLoad()) {

            const activityCommonDtoObject =
            {
                templateEntityName: 'alt_smstemplate',
                templateType: activityTemplateType.Sms,
                templateAttributeName: 'alt_smstemplateid',
                effectedTemplateAttributesNamesArray: ['subject', 'description', 'alt_mobilephone', 'alt_contactid', 'alt_smstemplateid'],
                regardingOnChangeCallBacks: [mappAttributes],
                initDefaultValuesCallBacks: [mappAttributes],
            };

            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;
            switch (formType) {
                case crmFormTypes.Create: {
                    initOnChange();
                    ActivityTemplateCommonBL.OnLoad(executionContext, activityCommonDtoObject);

                    initFormUI();
                    break;
                }
                default:
                    {
                        Utils.CrmPage.DisableAllFormFields(formContext);
                        break;
                    }
            }
        }
        else {
            refreshForm();
        }
    };

    const onSave = function (executionContext) {
        formContext = executionContext.getFormContext();
        if (formContext.ui.getFormType() == Utils.CrmPage.FormType.Create
            && formContext.getAttribute("statuscode").getValue() == smsStatus.Send) {
            Utils.CrmPage.DisableAllFormFields(formContext);
            Xrm.Utility.showProgressIndicator(SENDING_NOW_SMS_NOTIFICATION_MESSAGE);
        }
    };

    const initOnChange = function () {
        formContext.getAttribute(formAttributes.alt_smstemplateid).addOnChange(smsTemplateOnChange);
        formContext.getAttribute(formAttributes.alt_contactid).addOnChange(contactOnChange);
        formContext.getAttribute(formAttributes.alt_mobilephone).addOnChange(Utils.CrmPage.HandleMobilePhoneAttributeChange);
    };

    const initFormUI = function () {

        handleContactCustomView();
    };

    const smsTemplateOnChange = function (executionContext) {
        formContext = executionContext.getFormContext();

        if (mappingSettings.targetEntity) {
            const smsTemplateId = formContext.getAttribute(formAttributes.alt_smstemplateid).getValue();
            let select = 'alt_parsercustomentrypointschemaname';
            if (smsTemplateId && smsTemplateId[0]) {
                Utils.Server.Retrieve('alt_smstemplate', smsTemplateId[0].id, select, null, function (result) {

                    if (result && result[select] && mappingSettings.targetEntity && result[select] == mappingSettings.targetEntity) {

                        const parserEntryPoint = formContext.getAttribute(formAttributes.alt_parsercustomentrypoint).getValue();
                        ActivityTemplateCommonBL.ParseTemplateMessagesHandler(parserEntryPoint);
                    }
                    else {
                        const errorMessage = 'הגדרת תבנית מסרון שגויה. נא פנה למנהל מערכת.';
                        Xrm.Navigation.openAlertDialog({ text: errorMessage });
                        Utils.Server.WriteLog(errorMessage + 'Id: ' + smsTemplateId[0].id, Utils.Server.MessageLevel.Warning);
                    }
                }, null);
            }
            else {
                ActivityTemplateCommonBL.ParseTemplateMessagesHandler();
            }
        }
        else {
            ActivityTemplateCommonBL.ParseTemplateMessagesHandler();
        }
    };

    const contactOnChange = function (executionContext) {
        formContext = executionContext.getFormContext();
        const contact = formContext.getAttribute(formAttributes.alt_contactid);
        const isRequired = contact.getRequiredLevel() === Utils.CrmPage.RequirementLevel.Required;
        const contactId = contact.getValue() && contact.getValue()[0];
        if (isRequired) {
            if (contactId) {
                mappAttributesByTargetEntity(contactId.id);
            }
            else {
                Utils.CrmPage.MapAttributesValues(formContext, mappingSettings.targetEntityAttributes, null);
                formContext.getAttribute(formAttributes.alt_smstemplateid).setValue(null);
                formContext.getAttribute(formAttributes.alt_smstemplateid).fireOnChange();
            }
            Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.alt_smstemplateid, !contactId);
        }
        else {
            if (contactId) {
                setMobilePhone();
            }
            else {
                formContext.getAttribute(formAttributes.alt_mobilephone).setValue(null);
            }
        }
    };

    const setMappingSettingsByRegardingObject = function () {

        if (!mappingSettings) {
            const regardingObject = formContext.getAttribute(formAttributes.regardingobjectid).getValue();
            if (regardingObject && regardingObject[0]) {

                const entityName = regardingObject[0].entityType;
                const filteredMappingSettings = relatedRegardingAttributesToMapp.filter(function (value) {
                    return Array.isArray(value.entityName) ? value.entityName.includes(entityName) : value.entityName === entityName;
                });
                mappingSettings = filteredMappingSettings[0];
            }
        }
    };

    const mappAttributes = function () {

        setMappingSettingsByRegardingObject();
        if (mappingSettings) {
            const regardingObject = formContext.getAttribute(formAttributes.regardingobjectid).getValue();
            if (regardingObject && regardingObject[0]) {
                if (mappingSettings.attributes) {
                    mappAttributesByRegardingObject(regardingObject[0]);
                }
            }
        }
    };

    const mappAttributesByRegardingObject = function (regardingObject) {

        let select = generateSelect(mappingSettings.attributes);
        if (mappingSettings.relatedAttribute) {
            select += ",_" + mappingSettings.relatedAttribute + "_value";
        }
        Utils.Server.Retrieve(mappingSettings.entityName, regardingObject.id, select, null, function (result) {
            if (result) {
                if (mappingSettings.relatedAttribute) {
                    mappingSettings.relatedAttributeValue = result[mappingSettings.relatedAttribute];
                }
                Utils.CrmPage.MapAttributesValues(formContext, mappingSettings.attributes, result, true);
            }
        }, null);
    };

    const mappAttributesByTargetEntity = function (contactId) {

        const regardingObject = formContext.getAttribute(formAttributes.regardingobjectid).getValue();
        if (regardingObject && regardingObject[0]) {
            const attributeToFilter = mappingSettings.relatedAttribute && mappingSettings.relatedAttributeValue ?
                mappingSettings.relatedAttribute : regardingObject[0].entityType + 'id';
            const attributeValueToFilter = mappingSettings.relatedAttribute && mappingSettings.relatedAttributeValue ?
                mappingSettings.relatedAttributeValue.Id : regardingObject[0].id;
            var fetchXml = [
                "<fetch top='1'>",
                "<entity name='" + mappingSettings.targetEntity + "'>",
                generateAttributes(mappingSettings.targetEntityAttributes),
                "<filter type='and'>",
                "<condition attribute='statecode' operator='eq' value='0' />",
                "<condition attribute='", mappingSettings.targetEntityCustomerAttributeName, "' operator='eq' value='", Utils.JsExtantions.String.RemoveBraces(contactId), "'/>",
                "<condition attribute='", attributeToFilter, "' operator='eq' value='", Utils.JsExtantions.String.RemoveBraces(attributeValueToFilter), "'/>",
                "</filter>",
                "</entity>",
                "</fetch>"
            ].join("");

            Utils.Server.Fetch(mappingSettings.targetEntity, fetchXml, function (result) {
                if (result && result[0]) {
                    const targetEntityIdAttributeName = mappingSettings.targetEntity + 'id';
                    formContext.getAttribute(formAttributes.alt_parsercustomentrypoint).setValue(generateCustomEntryPointString(mappingSettings.targetEntity, result[0][targetEntityIdAttributeName]));
                    Utils.CrmPage.MapAttributesValues(formContext, mappingSettings.targetEntityAttributes, result[0], true);
                }
            }, null);
        }   
    };

    const generateCustomEntryPointString = function (logicalName, id) {

        let entityReference = {

            "LogicalName": logicalName,
            "Id": id
        };

        return JSON.stringify(entityReference);
    };

    const generateAttributes = function (attributesToMapp) {

        let attributes = '';
        attributesToMapp.forEach(function (forToElement) {

            attributes += "<attribute name='" + forToElement.from + "'/>";
        });
        return attributes;
    };

    const generateSelect = function (attributesToMapp) {
        let select = [];
        attributesToMapp.forEach(function (forToElement) {
            var type = formContext.getAttribute(forToElement.to).getAttributeType();
            if (type === Utils.CrmPage.AttributeType.Lookup) {
                select.push("_" + forToElement.from + "_value");
            } else {
                select.push(forToElement.from);
            }
        });

        return select.join(',');
    };

    const setMobilePhone = function () {
        const mobilePhone = formContext.getAttribute(formAttributes.alt_mobilephone).getValue();
        if (!mobilePhone) {
            const contact = formContext.getAttribute(formAttributes.alt_contactid).getValue();
            const regardingObject = formContext.getAttribute(formAttributes.regardingobjectid).getValue();
            if (contact && contact[0]) {
                Utils.Server.Retrieve("contact", contact[0].id, 'mobilephone', null, function (result) {
                    if (mobilePhone != result.mobilephone) {
                        formContext.getAttribute(formAttributes.alt_mobilephone).setValue(result.mobilephone);
                        Utils.CrmPage.HandleMobilePhoneAttributeChange();
                    }
                }
                    , null);
            } else if (regardingObject && regardingObject[0]) {
                formContext.getControl(formAttributes.alt_mobilephone).setDisabled(false);
            }
        }
    };

    const refreshForm = function () {
        formContext.data.refresh(false);
        var statusCodeValue = formContext.getAttribute("statuscode").getValue();
        if (statusCodeValue !== smsStatus.SendingNow) {
            Xrm.Utility.closeProgressIndicator();
            return;
        }
        setTimeout(refreshForm, 2000);
    };

    const handleContactCustomView = function () {

        setMappingSettingsByRegardingObject();
        if (mappingSettings && mappingSettings.filterContact
            && mappingSettings.targetEntity
            && mappingSettings.targetEntityCustomerAttributeName) {

            const regardingObject = formContext.getAttribute(formAttributes.regardingobjectid).getValue()[0];
            let fetchXml = generateFetchXmlContactFilterByRegardingObject(regardingObject, mappingSettings.targetEntity, mappingSettings.targetEntityCustomerAttributeName);
            Utils.Global.FilterCustomers(formContext, formAttributes.alt_contactid, mappingSettings.targetEntity, fetchXml, mappingSettings.targetEntityCustomerAttributeName, true, entityTypeCode.Contact);
        }
    };

    const generateFetchXmlContactFilterByRegardingObject = function (regardingObject, entityName, contactAttributeName) {

        const fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
            "<entity name='" + entityName + "'>" +
            "<attribute name='alt_name' />" +
            "<attribute name='" + contactAttributeName + "' />" +
            "<filter type='and'>" +
            "<condition attribute='statecode' operator='eq' value='0' />" +
            "<condition attribute='" + regardingObject.entityType + 'id' + "' value='" + regardingObject.id + "' operator='eq' /> " +
            "</filter>" +
            "</entity>" +
            "</fetch>";
        return fetchXml;
    };

    return {
        OnLoad: onLoad,
        OnSave: onSave
    };

})();
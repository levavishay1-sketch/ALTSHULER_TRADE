/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Server.js" />

var ActivityTemplateCommonBL = (function () {

    const PARSING_TEMPLATE_MESSAGE = 'מתבצעת ניתוח תבנית...';
    const htmlEmptyContent = '<p class="MsoNormal" dir="RTL"><br/></p>';

    let formContext;
    let dtoObject;

    const onLoad = function (executionContext, templateDtoObject) {

        formContext = executionContext.getFormContext();
        dtoObject = templateDtoObject;
        const formType = formContext.ui.getFormType();
        const crmFormTypes = Utils.CrmPage.FormType;
        switch (formType) {
            case crmFormTypes.Create: {
                initOnChange();
                initDefaultValues();
                filterTemplateLookupByRegardingObject();
                break;
            }

            default:
                break;
        }       
        handleUIByRegardingObject();
    };

    const initOnChange = function () {
        formContext.getAttribute('regardingobjectid').addOnChange(regardingObjectOnChange);
    };

    const regardingObjectOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        filterTemplateLookupByRegardingObject();
        handleUIByRegardingObject();
        invokeCallBacksArrayDependOnRegarding(dtoObject.regardingOnChangeCallBacks);
    };

    const initDefaultValues = function () {

        const regardingObject = formContext.getAttribute("regardingobjectid")
            && formContext.getAttribute("regardingobjectid").getValue();

        if (regardingObject && regardingObject[0]) {

            dtoObject.initDefaultValuesCallBacks.forEach(function (callBack) {
                callBack();
            });
        } else {
            handleEmptyRegarding();
        }
    };

    const invokeCallBacksArrayDependOnRegarding = function (callBacksArray) {

        clearEffectedTemplateAttributes(dtoObject.effectedTemplateAttributesNamesArray);

        const regardingObject = formContext.getAttribute("regardingobjectid").getValue();
        if (regardingObject && regardingObject[0]) {
            formContext.getControl(dtoObject.templateAttributeName).setDisabled(false);
            callBacksArray.forEach(function (callBack) {
                callBack();
            });
        } else {
            handleEmptyRegarding();
        }
    }

    const handleEmptyRegarding = function () {

        formContext.getControl(dtoObject.templateAttributeName).setDisabled(true);
        clearEffectedTemplateAttributes(dtoObject.effectedTemplateAttributesNamesArray);
    };

    const clearEffectedTemplateAttributes = function (fieldsArray) {

        const attributeType = Utils.CrmPage.AttributeType;
        fieldsArray.forEach(function (attributeName) {
            let attribute = formContext.getAttribute(attributeName);
            switch (attribute.getAttributeType()) {
                case attributeType.Boolean:
                case attributeType.Optionset:
                case attributeType.Multioptionset: {
                    let initialValue = attribute.getInitialValue();
                    if (initialValue != attribute.getValue()) {
                        attribute.setValue(attribute.getInitialValue());
                    }
                    break;
                }
                case attributeType.Memo:
                    {
                        if (formContext.data.entity.getEntityName() === 'email' && attributeName === 'description') {
                            attribute.setValue('<body contenteditable="true" defaultvalue="" style="direction: rtl; overflow-wrap: break-word;"></body>');
                        } else {
                            attribute.setValue(null);
                        }
                        break;
                    }
                default:
                    {
                        attribute.setValue(null);
                        break;
                    }
            }
        });
    };

    const createTemplateObject = function (templateType, templateId, regardingObjectId, regardingObjectName) {
        crmDataTypes = Utils.Server.CrmDataTypes;
        return [
            { 'key': 'TemplateType', 'value': templateType, 'type': crmDataTypes.Int },
            { 'key': 'TemplateId', 'value': templateId, 'type': crmDataTypes.String },
            { 'key': 'RegardingObjectId', 'value': regardingObjectId, 'type': crmDataTypes.String },
            { 'key': 'RegardingObjectName', 'value': regardingObjectName, 'type': crmDataTypes.String }
        ];
    };

    const parseAndSetMessage = function (templateObjectData) {

        Xrm.Utility.showProgressIndicator(PARSING_TEMPLATE_MESSAGE);     

        Utils.Server.CallAction("alt_TemplateGenerator", null, formContext.data.entity.getId(), templateObjectData, function (result) {
            if (result.IsSucceeded) {
                formContext.getAttribute("description").setValue(result.DescriptionMessage);
                formContext.getAttribute("subject").setValue(result.SubjectTemplateMessage);
            } else {
                alert(result.DescriptionMessage);
                clearDependedAttribute();
            }
            Xrm.Utility.closeProgressIndicator();
        }, function (error) {

            console.log(error);
            Xrm.Utility.closeProgressIndicator();
            Xrm.Navigation.openAlertDialog({ text: INTERNAL_SERVER_ERROR });
        });
    };

    const clearDependedAttribute = function () {

        let descriptionValue = activityTemplateType.Email ? htmlEmptyContent : null;

        formContext.getAttribute("description").setValue(descriptionValue);
        formContext.getAttribute("subject").setValue(null);
    };

    const parseTemplateMessagesHandler = function (parserEntryPoint) {

        let templateType = dtoObject.templateType;
        let templateId = formContext.getAttribute(dtoObject.templateAttributeName).getValue();
        let parserEntryPointId;
        let parserEntryPointLogicalName;
        if (parserEntryPoint) {
            let parserEntryPointObject = JSON.parse(parserEntryPoint);
            parserEntryPointId = parserEntryPointObject.Id;
            parserEntryPointLogicalName = parserEntryPointObject.LogicalName
        }
        else {
            let regardingObject = formContext.getAttribute("regardingobjectid").getValue();
            if (regardingObject && regardingObject[0]) {
                parserEntryPointId = regardingObject[0].id;
                parserEntryPointLogicalName = regardingObject[0].entityType;
            }
        }

        let isValidInputs = false;
        if (parserEntryPointId && parserEntryPointLogicalName) {
            if (templateId !== null && templateId[0] !== null) {
                isValidInputs = true;
                const templateObjectData = createTemplateObject(templateType, templateId[0].id, parserEntryPointId, parserEntryPointLogicalName);
                parseAndSetMessage(templateObjectData);
            }
        }

        if (!isValidInputs) {
            formContext.getAttribute("description").setValue(null);
            formContext.getAttribute("subject").setValue(null);
            if (templateType.value === activityTemplateType.Email) {
                formContext.getAttribute("description").setValue(htmlEmptyContent);
            }
        }
    };

    const filterTemplateLookupByRegardingObject = function (customViewId) {

        const customViewDisplayName = 'תבניות לפי שם סכמה';
        const defaultViewId = '{00000000-0000-0000-0000-000000000001}';
        const regardingObject = formContext.getAttribute("regardingobjectid").getValue();
        if (regardingObject && regardingObject[0]) {

            let reagardingObjectEntityType = regardingObject[0].entityType;
            let entityName = dtoObject.templateEntityName;

            let fetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">' +
                '<entity name="' + entityName + '">' +
                '<attribute name="alt_name" />' +
                '<attribute name="' + entityName + 'id" />' +
                '<attribute name="alt_schemaname" />' +
                '<order attribute="createdon" descending="false" />' +
                '<filter type="and">' +
                '<condition attribute="alt_userdisplaybit" operator="eq" value="1" />' +
                '<condition attribute="alt_schemaname" operator="eq" value="' + reagardingObjectEntityType + '" />' +
                '</filter>' +
                '</entity>' +
                '</fetch>';

            let layoutXml = '<grid name="resultset" jump="alt_name" select="1" preview="1" icon="1"><row name="result" id="' + entityName + 'id">' +
                '<cell name="alt_name" width="100"/>' +
                '</row></grid>';

            let lookupControl = formContext.getControl(dtoObject.templateAttributeName);
            lookupControl.addCustomView(defaultViewId, entityName, customViewDisplayName, fetchXml, layoutXml, true);
        }
    };

    const handleUIByRegardingObject = function () {

        let regardingObject = formContext.getAttribute("regardingobjectid").getValue();
        if (regardingObject && regardingObject[0]) {
            if (formContext.getAttribute('alt_contactid')) {
                switch (regardingObject[0].entityType) {
                    case entityName.Incident:
                    case entityName.Portfolio:
                    case entityName.DigitalFormVerification:
                        {
                            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_contactid', Utils.CrmPage.RequirementLevel.Required);
                            break;
                        }
                    default:
                }
            }       
        }
        let isDesabled = formContext.getAttribute('alt_contactid') ?
            regardingObject == null ||
            (formContext.getAttribute('alt_contactid').getRequiredLevel() === Utils.CrmPage.RequirementLevel.Required
               && formContext.getAttribute('alt_contactid').getValue() == null) : regardingObject == null;

        formContext.getControl(dtoObject.templateAttributeName).setDisabled(isDesabled);
    };

    return {
        OnLoad: onLoad,
        ParseTemplateMessagesHandler: parseTemplateMessagesHandler,
        FilterTemplateLookupByRegardingObject: filterTemplateLookupByRegardingObject
    };

})();
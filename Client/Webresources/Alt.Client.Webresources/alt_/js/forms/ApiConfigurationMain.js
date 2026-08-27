/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.UIHandler.js" />

var ApiConfigurationMain = (function () {

    let formContext;

    const apiTypeCode = {
        Incoming: 1,
        Outgoing: 2,
        Redirected: 3
    };

    const systemCode = {
        CRM: 1
    };
    const apiTypeJSON = {
        "1": { "tabs": [{ "name": "GeneralTab", "sections": [{ "name": "GeneralSection", "controls": [{ "name": "alt_sourcesystemcode" }, { "name": "alt_destinationsystemcode" }, { "name": "alt_description" }, { "name": "alt_codeint" }, { "name": "alt_name" }, { "name": "alt_url", "required": "required" }, { "name": "alt_requestmethodcode", "required": "required" }, { "name": "alt_apitypecode" }] }, { "name": "IncomingApiSection" }] }, { "name": "SettingsTab", "sections": [{ "name": "SettingsSection" }] }, { "name": "XmlValidationModelTab", "sections": [{ "name": "XmlValidationModelSection" }] }] },
        "2": { "tabs": [{ "name": "GeneralTab", "sections": [{ "name": "GeneralSection", "controls": [{ "name": "alt_sourcesystemcode" }, { "name": "alt_destinationsystemcode" }, { "name": "alt_description" }, { "name": "alt_codeint" }, { "name": "alt_name" }, { "name": "alt_requestmethodcode", "required": "required" }, { "name": "alt_url", "required": "required" }, { "name": "alt_apitypecode" }] }, { "name": "OutgoingApiSection" }] }, { "name": "SettingsTab", "sections": [{ "name": "SettingsSection" }] }, { "name": "HttpHeadersTab", "sections": [{ "name": "HttpHeadersSection" }] }] },
        "3": { "tabs": [{ "name": "GeneralTab", "sections": [{ "name": "GeneralSection", "controls": [{ "name": "alt_sourcesystemcode", "disabled": true }, { "name": "alt_destinationsystemcode", "disabled": true }, { "name": "alt_description" }, { "name": "alt_codeint" }, { "name": "alt_name" }, { "name": "alt_apitypecode" }] }] }, { "name": "SettingsTab", "sections": [{ "name": "SettingsSection" }] }] },
        "NULL": { "tabs": [{ "name": "GeneralTab", "sections": [{ "name": "GeneralSection", "controls": [{ "name": "alt_sourcesystemcode" }, { "name": "alt_destinationsystemcode" }, { "name": "alt_description" }, { "name": "alt_codeint" }, { "name": "alt_name" }, { "name": "alt_apitypecode" }] }] }] }
    };

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();

        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;
            Utils.UIHandler.Initialize(formContext, 'GeneralTab');
            switch (formType) {
                case crmFormTypes.Create:
                case crmFormTypes.Update: {
                    initOnChange();
                    break;
                }
                default:
                    break;
            }
            initUI();
        }
    };

    const initOnChange = function () {
        formContext.getAttribute('alt_apitypecode').addOnChange(apiTypeCodeOnChange);
    };

    const apiTypeCodeOnChange = function (executionContext) {
        formContext = executionContext.getFormContext();
        handleUIByApiTypeCode();
        setSystemValuesByApiTypeCode();
    };

    const initUI = function () {
        handleUIByApiTypeCode();
    };

    const handleUIByApiTypeCode = function () {

        const apiType = formContext.getAttribute('alt_apitypecode').getValue();
        const uiConfigurationJson = apiType ? apiTypeJSON[apiType] : apiTypeJSON.NULL;
        const key = apiType ? apiType : 0;
        Utils.UIHandler.Clear().SetByJson(uiConfigurationJson, 'alt_apitypecode_' + key).RenderUI();
    };

    const setSystemValuesByApiTypeCode = function () {
        const apiType = formContext.getAttribute('alt_apitypecode').getValue();
        let sysemCodeValue = apiType == apiTypeCode.Redirected ? systemCode.CRM : null;
        if (formContext.getAttribute('alt_sourcesystemcode').getValue() != sysemCodeValue) {
            formContext.getAttribute('alt_sourcesystemcode').setValue(sysemCodeValue);
        }
        if (formContext.getAttribute('alt_destinationsystemcode').getValue() != sysemCodeValue) {
            formContext.getAttribute('alt_destinationsystemcode').setValue(sysemCodeValue);
        }      
    };

    return {
        OnLoad: onLoad
    };
})();
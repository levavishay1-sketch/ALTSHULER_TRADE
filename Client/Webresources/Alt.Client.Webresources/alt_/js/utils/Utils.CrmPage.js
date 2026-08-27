/// <reference path="Utils.Validators.js" />

function CloseProgressIndicatorOnUnload(event) {
    Xrm.Utility.closeProgressIndicator();
    window.removeEventListener("unload", CloseProgressIndicatorOnUnload);
}

window.addEventListener("unload", CloseProgressIndicatorOnUnload);


if (typeof (Utils) === "undefined")
    Utils = {};

Utils.CrmPage = (function () {

    const COMMON_REQUEST_FAILED_MESSAGE = 'לא הייתה אפשרות לבצע את הפעולה, אנא פנה למנהל מערכת';

    const formType = {
        Undefined: 0,
        Create: 1,
        Update: 2,
        ReadOnly: 3,
        Disable: 4,
        BulkEdit: 6,
        ReadOptimized: 11
    };

    const attributeType = {
        Boolean: "boolean",
        String: "string",
        Memo: "memo",
        Lookup: "lookup",
        DateTime: "datetime",
        Integer: "integer",
        Money: "money",
        Decimal: "decimal",
        Double: "double",
        Multioptionset: "multioptionset",
        Optionset: "optionset"
    };

    let controlType = {
        Standard: "standard",
        Iframe: "iframe",
        Kbsearch: "kbsearch",
        Lookup: "lookup",
        Multiselectoptionset: "multiselectoptionset",
        Notes: "notes",
        Optionset: "optionset",
        Quickform: "quickform",
        Subgrid: "subgrid",
        Timercontrol: "timercontrol",
        Timelinewall: "timelinewall",
        Webresource: "webresource"
    };

    const saveModes = {
        Save: 1,
        SaveAndClost: 2,
        Deactivate: 5,
        Reactivate: 6,
        Send: 7, //email only
        Disqualify: 15, //lead only
        Qualify: 16, //lead only
        Assign: 47,
        SaveAsCompleted: 58, //activites only
        SaveAndNew: 59,
        AutoSave: 70
    };

    const displayState = {
        Expanded: "expanded",
        Collapsed: "collapsed"
    };
    const requirementLevel = {
        Required: "required",
        Recommended: "recommended",
        None: "none"
    };

    const defaultViewId = '{00000000-0000-0000-0000-000000000001}';

    let formDefaultSettings;

    let formLoadCounter = window.formLoadCounter || 0;

    const isFirstLoad = function () {
        formLoadCounter++;
        window.formLoadCounter = formLoadCounter;
        return parseInt(formLoadCounter) === 1;
    };

    const handleTelephoneAttributeChange = function (executionContext) {
        handleTelephoneAttribute(executionContext.getFormContext(), executionContext.getEventSource().getName());
    };

    const handleMobilePhoneAttributeChange = function (executionContext) {
        handleMobilePhoneAttribute(executionContext.getFormContext(), executionContext.getEventSource().getName());
    };

    const handleEmailAddressAttributeChange = function (executionContext) {
        handleEmailAddressAttribute(executionContext.getFormContext(), executionContext.getEventSource().getName());
    };

    const handleGovIdAttributeChange = function (executionContext) {
        handleGovIdAttribute(executionContext.getFormContext(), executionContext.getEventSource().getName());
    };

    const handleAccountNumberAttributeChange = function (executionContext) {
        handleAccountNumberAttribute(executionContext.getFormContext(), executionContext.getEventSource().getName());
    };

    const handlePassportAttributeChange = function (executionContext) {
        handlePassportAttribute(executionContext.getFormContext(), executionContext.getEventSource().getName());
    };

    const handleDateTimeAttributeForFutureDateChange = function (executionContext) {
        handleDateTimeAttributeForFutureDate(executionContext.getFormContext(), executionContext.getEventSource().getName());
    };

    const handleDateTimeAttributeForPastDateChange = function (executionContext) {
        handleDateTimeAttributeForPastDate(executionContext.getFormContext(), executionContext.getEventSource().getName());
    };

    const handleLandlinePhoneNumberAttributeChange = function (executionContext) {
        handleLandlinePhoneNumberAttribute(executionContext.getFormContext(), executionContext.getEventSource().getName());
    };

    const handleTelephoneAttribute = function (formContext, attributeName) {

        const PHONE_CONTROL_NOTIFICATION_MESSAGE = "עליך להזין מספר טלפון חוקי.";
        const PHONE_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY = "3543543413645";
        const phoneControl = formContext.getControl(attributeName);
        let phoneValue = phoneControl.getAttribute().getValue();


        if (phoneValue !== null && !Utils.Validators.IsValidPhoneNumber(phoneValue)) {

            phoneControl.setNotification(PHONE_CONTROL_NOTIFICATION_MESSAGE, PHONE_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
            return false;
        }
        else {
            if (phoneValue !== null && phoneValue.indexOf("-") !== -1) {
                phoneControl.getAttribute().setValue(phoneValue.replace("-", ""));
            }
            phoneControl.clearNotification(PHONE_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
            return true;
        }
    };

    const handleLandlinePhoneNumberAttribute = function (formContext, attributeName) {

        const PHONE_CONTROL_NOTIFICATION_MESSAGE = "עליך להזין מספר טלפון קווי חוקי.";
        const PHONE_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY = "32569874581";
        const phoneControl = formContext.getControl(attributeName);
        let phoneValue = phoneControl.getAttribute().getValue();


        if (phoneValue !== null && !Utils.Validators.IsValidLandlinePhoneNumber(phoneValue)) {

            phoneControl.setNotification(PHONE_CONTROL_NOTIFICATION_MESSAGE, PHONE_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
            return false;
        }
        else {
            if (phoneValue !== null && phoneValue.indexOf("-") !== -1) {
                phoneControl.getAttribute().setValue(phoneValue.replace("-", ""));
            }
            phoneControl.clearNotification(PHONE_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
            return true;
        }
    };

    const handleMobilePhoneAttribute = function (formContext, attributeName) {

        const MOBILEPHONE_CONTROL_NOTIFICATION_MESSAGE = "עליך להזין מספר טלפון נייד חוקי.";
        const MOBILEPHONE_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY = "93959964999";

        const phoneControl = formContext.getControl(attributeName);
        let phoneValue = phoneControl.getAttribute().getValue();

        if (phoneValue !== null && !Utils.Validators.IsValidMobileNumber(phoneValue)) {

            phoneControl.setNotification(MOBILEPHONE_CONTROL_NOTIFICATION_MESSAGE, MOBILEPHONE_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
            return false;
        }
        else {
            if (phoneValue !== null && phoneValue.indexOf("-") !== -1) {
                phoneControl.getAttribute().setValue(phoneValue.replace("-", ""));
            }
            phoneControl.clearNotification(MOBILEPHONE_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
            return true;
        }
    };

    const handleEmailAddressAttribute = function (formContext, attributeName) {

        const EMAIL_ADDRESS_CONTROL_NOTIFICATION_MESSAGE = "עליך להזין כתובת דואר אלקטרוני חוקית.";
        const EMAIL_ADDRESS_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY = "565468";
        const emailAdressControl = formContext.getControl(attributeName);
        const emailAdressValue = emailAdressControl.getAttribute().getValue();

        if (emailAdressValue !== null && !Utils.Validators.IsValidEmailAddress(emailAdressValue)) {
            emailAdressControl.setNotification(EMAIL_ADDRESS_CONTROL_NOTIFICATION_MESSAGE, EMAIL_ADDRESS_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
            return false;
        }
        else {
            emailAdressControl.clearNotification(EMAIL_ADDRESS_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
            return true;
        }
    };

    const handleGovIdAttribute = function (formContext, attributeName) {

        const GOVID_CONTROL_NOTIFICATION_MESSAGE = 'עליך להזין מספר תעודת זהות חוקי.';
        const GOVID_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY = "755449";
        const govIdControl = formContext.getControl(attributeName);
        const govIdValue = govIdControl.getAttribute().getValue();

        if (govIdValue !== null && !Utils.Validators.IsValidGovId(govIdValue)) {

            govIdControl.setNotification(GOVID_CONTROL_NOTIFICATION_MESSAGE, GOVID_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
            return false;
        }
        else {
            govIdControl.clearNotification(GOVID_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
            return true;
        }
    };

    const handleAccountNumberAttribute = function (formContext, attributeName) {

        const GOVID_CONTROL_NOTIFICATION_MESSAGE = 'עליך להזין מספר ח"פ חוקי.';
        const GOVID_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY = "91877323857";
        const accountNumberControl = formContext.getControl(attributeName);
        const accountNumberValue = accountNumberControl.getAttribute().getValue();

        if (accountNumberValue !== null && !Utils.Validators.IsValidAccountNumber(accountNumberValue)) {

            accountNumberControl.setNotification(GOVID_CONTROL_NOTIFICATION_MESSAGE, GOVID_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
            return false;
        }
        else {
            accountNumberControl.clearNotification(GOVID_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
            return true;
        }
    };

    const handlePassportAttribute = function (formContext, attributeName) {

        const GOVID_CONTROL_NOTIFICATION_MESSAGE = 'עליך להזין מספר דרכון חוקי.';
        const GOVID_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY = "2153189218";
        const passportControl = formContext.getControl(attributeName);
        const passportValue = passportControl.getAttribute().getValue();

        if (passportValue !== null && !Utils.Validators.IsOnlyDigitsAndEnglishLetters(passportValue)) {

            passportControl.setNotification(GOVID_CONTROL_NOTIFICATION_MESSAGE, GOVID_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
            return false;
        }
        else {
            passportControl.clearNotification(GOVID_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
            return true;
        }
    };

    const handleDateTimeAttributeForFutureDate = function (formContext, attributeName) {
        const DATETIME_CONTROL_NOTIFICATION_MESSAGE = "עליך להזין תאריך עתידי בלבד";
        const DATETIME_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY = "75545645436489";
        const dateTimeControl = formContext.getControl(attributeName);
        const dateTimeAttribute = dateTimeControl.getAttribute();
        if (dateTimeAttribute.getValue() !== null && new Date(dateTimeAttribute.getValue()) < new Date()) {
            dateTimeControl.setNotification(DATETIME_CONTROL_NOTIFICATION_MESSAGE, DATETIME_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
        }
        else {
            dateTimeControl.clearNotification(DATETIME_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
        }
    };

    const handleDateTimeAttributeForPastDate = function (formContext, attributeName) {
        const DATETIME_CONTROL_NOTIFICATION_MESSAGE = "אין אפשרות להזין תאריך עתידי";
        const DATETIME_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY = "100000001";
        const dateTimeControl = formContext.getControl(attributeName);
        const dateTimeAttribute = dateTimeControl.getAttribute();
        if (dateTimeAttribute.getValue() !== null && new Date(dateTimeAttribute.getValue()) > new Date()) {
            dateTimeControl.setNotification(DATETIME_CONTROL_NOTIFICATION_MESSAGE, DATETIME_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
        }
        else {
            dateTimeControl.clearNotification(DATETIME_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
        }
    };

    const handleOnlyEnglishLettersInLowcase = function (formContext, attributeName) {

        const ONLY_INGLISH_LETTERS_ERROR_MESSAGE = "עליך לספק ערך שמכיל רק אותיות קטנות באנגלית";
        const ONLY_INGLISH_LETTERS_ERROR_MESSAGE_UNIQUE_KEY = "100000000";
        const control = formContext.getControl(attributeName);
        const value = control.getAttribute().getValue();

        if (value !== null && !Utils.Validators.IsOnlyEnglishDigitsInLowcase(value)) {

            control.setNotification(ONLY_INGLISH_LETTERS_ERROR_MESSAGE, ONLY_INGLISH_LETTERS_ERROR_MESSAGE_UNIQUE_KEY);
            return false;
        }
        else {
            control.clearNotification(ONLY_INGLISH_LETTERS_ERROR_MESSAGE_UNIQUE_KEY);
            return true;
        }
    };

    const setLookup = function (formContext, lookupName, id, text, logicalName) {

        //if (lookupName !== null && lookupName !== "undefined"
        //    && id !== null && id !== "undefined"
        //    && text !== null && text !== "undefined"
        //    && logicalName !== null && logicalName !== "undefined") {
        if (lookupName && id && logicalName) {

            if (id.indexOf('{') === -1)
                id = '{' + id;
            if (id.indexOf('}') === -1)
                id = id + '}';
            id = id.toUpperCase();

            const value = new Array();
            value[0] = new Object();
            value[0].id = id;
            value[0].name = text;
            value[0].entityType = logicalName;
            formContext.getAttribute(lookupName).setValue(value);
        }
        else {
            formContext.getAttribute(lookupName).setValue(null);
        }
    };

    const getAllDirtyFields = function (formContext) {
        const attr = formContext.data.entity.attributes.get();
        const listOfDirty = new Array();
        if (attr !== null) {
            for (let i in attr) {
                if (attr[i].getIsDirty()) {
                    listOfDirty.push(attr[i].getName());
                }
            }
        }
        return listOfDirty;
    };

    const disableAllFormFields = function (formContext) {
        formContext.ui.controls.forEach(function (control) {
            if (control && doesControlHaveAttribute(control) && control.getDisabled && !control.getDisabled()) {
                control.setDisabled(true);
            }
        });
    };

    const disableAttributes = function (formContext, attributesArray, disabledMode) {
        for (let i = 0; i < attributesArray.length; i++) {
            setControlDisabledMode(formContext, attributesArray[i], disabledMode);
        }
    };

    const disableSectionAttributesMode = function (formContext, tabName, sectionName, disableMode) {
        const section = formContext.ui.tabs.get(tabName).sections.get(sectionName);
        const controls = section.controls.get();
        const controlsLength = controls.length;
        for (let i = 0; i < controlsLength; i++) {
            if (controls[i].getDisabled() !== disableMode) {
                controls[i].setDisabled(disableMode);
            }
        }
    };

    const mapAttributesValues = function (formContext, detailsArray, valuesArray, slashLongerTextAttributes) {
        detailsArray.forEach(function (detail) {
            if (detail.to !== null) {
                let type = formContext.getAttribute(detail.to).getAttributeType();
                if (valuesArray) {
                    if (valuesArray[detail.from] !== null) {
                        switch (type) {
                            case attributeType.DateTime: {
                                formContext.getAttribute(detail.to).setValue(new Date(valuesArray[detail.from]));
                                break;
                            }
                            case attributeType.Memo:
                            case attributeType.String:
                                {
                                    if (formContext.getAttribute(detail.to).getMaxLength && formContext.getAttribute(detail.to).getMaxLength() >= valuesArray[detail.from].length) {
                                        formContext.getAttribute(detail.to).setValue(valuesArray[detail.from]);
                                    } else if (slashLongerTextAttributes === true) {
                                        let slashedText = valuesArray[detail.from].substring(0, formContext.getAttribute(detail.to).getMaxLength());
                                        formContext.getAttribute(detail.to).setValue(slashedText);
                                    }
                                    break;
                                }
                            case attributeType.Integer:
                            case attributeType.Decimal:
                            case attributeType.Double:
                            case attributeType.Money:
                                {
                                    if (formContext.getAttribute(detail.to).getMax && formContext.getAttribute(detail.to).getMax() >= valuesArray[detail.from]) {
                                        formContext.getAttribute(detail.to).setValue(valuesArray[detail.from]);
                                    }
                                    break;
                                }
                            case attributeType.Boolean:
                            case attributeType.Optionset:
                                {
                                    formContext.getAttribute(detail.to).setValue(valuesArray[detail.from]);
                                    break;
                                }
                            case attributeType.Lookup:
                                {
                                    Utils.CrmPage.SetLookup(formContext, detail.to, valuesArray[detail.from].Id, valuesArray[detail.from].Name, valuesArray[detail.from].LogicalName);
                                    break;
                                }
                            case attributeType.Multioptionset:
                                // Add logic here
                                break;
                            default:
                                break;
                        }
                    }
                }
                else {
                    formContext.getAttribute(detail.to).setValue(null);
                }
            }
            if (detail.fireOnChange) {
                formContext.getAttribute(detail.to).fireOnChange();
            }
            if (detail.disabled) {
                setControlDisabledMode(formContext, detail.to, true);
            }
        });
    };

    const getLookupETC = function (formContext, attrName) {
        return formContext.getAttribute(attrName).getLookupDataAttribute().getSupportedLookupTypes()[0];
    };

    const generateSelectFromArrayOfObjectsByProperty = function (arrayOfObjects, propertyName, entityName) {
        if (!arrayOfObjects || !propertyName) {
            return null;
        }

        const propertyArray = [];
        for (let i = 0; i < arrayOfObjects.length; i++) {
            if (arrayOfObjects[i].hasOwnProperty(propertyName)) {
                if (arrayOfObjects[i][propertyName] !== entityName + 'id') {
                    propertyArray.push(arrayOfObjects[i][propertyName]);
                }
            }
            else {
                return null;
            }
        }
        if (entityName) {
            propertyArray.push(entityName + 'id');
        }

        return propertyArray.join(',');
    };

    const clearFields = function (arrayOfFieldsToClear, formContext) {
        if (arrayOfFieldsToClear) {
            arrayOfFieldsToClear.forEach(function (value) {
                if (formContext.getAttribute(value).getValue() !== null) {
                    formContext.getAttribute(value).setValue(null);
                }
            });
        }
    };

    const handleRecordLegalityCreation = function (formContext, settingsArray, legalCallBack) {
        const filterdSettingsArray = settingsArray.filter(function (settingObj) {
            return !((settingObj.conditionCallback && settingObj.conditionCallback(formContext))
                || (settingObj.attributeName && formContext.getAttribute(settingObj.attributeName).getValue()));
        });

        if (filterdSettingsArray && filterdSettingsArray.length > 0) {
            Xrm.Navigation.openAlertDialog({ text: filterdSettingsArray[0].errorMessage }).then(
                function success(result) {
                    formContext.ui.close();
                },
                function (error) {
                    console.log(error.message);
                }
            );
        } else if (legalCallBack) {
            legalCallBack();
        }
    };

    const isUCI = function () {

        let globalContext = Xrm.Utility.getGlobalContext();
        let currentAppUrl = globalContext.getCurrentAppUrl().toLowerCase();
        return currentAppUrl.indexOf("appid") === -1 ? false : true;

        // return Xrm.Internal.isUci();
    };

    const NotifyEmptyRequiredReadOnlyFieldsOnQuickCreate = function (executionContext) {
        formContext = executionContext.getFormContext();
        let isFieldsPopulated = true;
        formContext.ui.controls.forEach(function (control) {
            if (doesControlHaveAttribute(control) && control.getDisabled()
                && control.getAttribute().getRequiredLevel() === "required") {
                let controlLabel = control.getLabel();
                let controloAttribute = control.getAttribute();

                let REQUIRED_CONTROL_NOTIFICATION_MESSAGE = "עליך לספק ערך עבור " + controlLabel;
                let REQUIRED_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY = controloAttribute.getName();
                if (!controloAttribute.getValue()) {
                    control.setNotification(REQUIRED_CONTROL_NOTIFICATION_MESSAGE, REQUIRED_CONTROL_NOTIFICATION_MESSAGE_UNIQUE_KEY);
                    if (isFieldsPopulated) {
                        isFieldsPopulated = false;
                    }
                }
            }
        });
        if (!isFieldsPopulated) {
            executionContext.getEventArgs().preventDefault();
        }
    };

    const doesControlHaveAttribute = function (control) {
        if (control) {
            var type = control.getControlType();
            switch (type) {
                case controlType.Standard:
                case controlType.Lookup:
                case controlType.Optionset:
                case controlType.Multiselectoptionset: {
                    return true;
                }
                default: {
                    return false;
                }
            }
        }
    };

    const setAttributeRequiredLevel = function (formContext, attributeName, requiredLevel) {
        if (formContext.getAttribute(attributeName).getRequiredLevel() !== requiredLevel) {
            formContext.getAttribute(attributeName).setRequiredLevel(requiredLevel);
        }
    };

    const setControlDisabledMode = function (formContext, controlName, disabledMode) {
        if (formContext.getControl(controlName) && formContext.getControl(controlName).getDisabled() !== disabledMode) {
            formContext.getControl(controlName).setDisabled(disabledMode);
        }
    };

    const setControlVisibleMode = function (formContext, controlName, visibleMode, isClearAttributeValue) {

        if (formContext.getControl(controlName).getVisible() !== visibleMode) {

            let control = formContext.getControl(controlName);
            if (!visibleMode && isClearAttributeValue) {
                let attribute = control.getAttribute();
                if (attribute.getValue()) {
                    clearAttributeValue(attribute);
                }
                clearAttributeValue(attribute);
            }
            control.setVisible(visibleMode);
        }
    };

    const isGridButtonEnabledByPrimaryEntityName = function (formContext, disabledEntites) {
        let primaryEntityName = formContext.data.entity.getEntityName();
        for (var i = 0; i < disabledEntites.length; i++) {
            if (disabledEntites[i] === primaryEntityName) {
                return false;
            }
        }
        return true;
    };

    const doesFormHaveFooter = function (formContext) {
        let formWithFooter = false;
        formContext.ui.controls.forEach(function (control) {
            if (control.getName().startsWith('footer')) {

                formWithFooter = true;
            }
        });
        return formWithFooter;
    };

    const doesFormHaveHeader = function (formContext) {
        let formWithFooter = false;
        formContext.ui.controls.forEach(function (control) {
            if (control.getName().startsWith('header')) {

                formWithFooter = true;
            }
        });
        return formWithFooter;
    };

    const setTabVisibilityMode = function (formContext, tabName, visibilityMode) {
        if (tabName && formContext) {
            let isVisible = visibilityMode === true ? true : false;
            if (isVisible !== formContext.ui.tabs.get(tabName).getVisible()) {
                formContext.ui.tabs.get(tabName).setVisible(isVisible);
            }
        }
    };

    const clearAttributeValue = function (attribute) {
        let attributeSettings = getAttributeSettings(attribute.getName());
        if (attributeSettings) {
            attribute.setValue(attributeSettings.value);
        }
        else {
            switch (attribute.getAttributeType()) {
                case attributeType.Boolean: {
                    if (attribute.getValue() !== false) {
                        attribute.setValue(false);
                    }
                    break;
                }
                case attributeType.Optionset:
                case attributeType.Multioptionset: {
                    const initialValue = attribute.getInitialValue();
                    if (attribute.getValue() !== initialValue) {
                        attribute.setValue(initialValue);
                    }
                    break;
                }
                default:
                    {
                        if (attribute.getValue() !== null) {
                            attribute.setValue(null);
                        }
                        break;
                    }
            }
        }
    };

    const handleControlsVisibleMode = function (formContext, controlsNameArray, visibleMode) {
        controlsNameArray.forEach(function (controlName) {
            let control = formContext.getControl(controlName);
            if (!visibleMode && doesControlHaveAttribute(control)) {
                setAttributeRequiredLevel(formContext, controlName, requirementLevel.None);
                let attribute = control.getAttribute();
                if (attribute.getValue()) {
                    clearAttributeValue(attribute);
                }
            }
            setControlVisibleMode(formContext, controlName, visibleMode);
        });
    };

    const saveFormSettings = function (formContext) {
        formDefaultSettings = {};
        formContext.ui.tabs.forEach(function (formTab) {
            let tabName = formTab.getName();
            formDefaultSettings[tabName] = {};
            formDefaultSettings[tabName]["visible"] = formTab.getVisible();
            formDefaultSettings[tabName]["displayState"] = formTab.getDisplayState();
            formTab.sections.forEach(function (formSection) {
                let formSectionName = formSection.getName();
                formDefaultSettings[tabName][formSectionName] = {};
                formDefaultSettings[tabName][formSectionName]["visible"] = formSection.getVisible();
                formSection.controls.forEach(function (formControl) {
                    let controlOrAttributeName = doesControlHaveAttribute(formControl) ? formControl.getAttribute().getName() : formControl.getName();
                    formDefaultSettings[tabName][formSectionName][controlOrAttributeName] = {};
                    if (doesControlHaveAttribute(formControl)) {
                        let attribute = formControl.getAttribute();
                        formDefaultSettings[tabName][formSectionName][controlOrAttributeName]["required"] = attribute.getRequiredLevel();
                        formDefaultSettings[tabName][formSectionName][controlOrAttributeName]["value"] = attribute.getValue();
                        formDefaultSettings[tabName][formSectionName][controlOrAttributeName]["disabled"] = formControl.getDisabled();
                    }
                    formDefaultSettings[tabName][formSectionName][controlOrAttributeName]["visible"] = formControl.getVisible();
                });
            });
        });
        // console.log(formDefaultSettings);
    };

    const getAttributeSettings = function (attributeName) {
        if (formDefaultSettings) {
            let tabs = Object.keys(formDefaultSettings);
            for (let i = 0; i < tabs.length; i++) {
                let tab = formDefaultSettings[tabs[i]];
                if (typeof tab === 'object') {
                    let sections = Object.keys(tab);
                    for (var j = 0; j < sections.length; j++) {
                        let section = formDefaultSettings[tabs[i]][sections[j]];
                        if (typeof section === 'object' && section[attributeName]) {
                            return formDefaultSettings[tabs[i]][sections[j]][attributeName];
                        }
                    }
                }
            }
        }
    };

    const displayAttributesWithValue = function (formContext, attributes) {
        attributes.forEach(function (attributeName) {
            let fieldValue = formContext.getAttribute(attributeName).getValue();
            let visibleMode = fieldValue ? true : false;
            setControlVisibleMode(formContext, attributeName, visibleMode);
        });
    };

    const setHiddenFieldsToUnRequired = function (formContext) {
        formContext.data.entity.attributes.forEach(function (attr, index) {
            var control = formContext.getControl(attr.getName());
            if (control) {
                if (!control.getVisible()) {
                    control.getAttribute().setRequiredLevel('none');
                }
            }
        });
    };

    const setSectionVisibleMode = function (formContext, tabName, sectionName, isVisible) {

        const section = formContext.ui.tabs.get(tabName).sections.get(sectionName);
        if (section && section.getVisible() != isVisible) {
            section.setVisible(isVisible);
        };
    };

    return {
        FormType: formType,
        IsFirstLoad: isFirstLoad,
        AttributeType: attributeType,
        DefaultViewId: defaultViewId,
        MapAttributesValues: mapAttributesValues,
        HandleTelephoneAttribute: handleTelephoneAttribute,
        HandleMobilePhoneAttribute: handleMobilePhoneAttribute,
        HandleEmailAddressAttribute: handleEmailAddressAttribute,
        HandleDateTimeAttributeForFutureDate: handleDateTimeAttributeForFutureDate,
        HandleGovIdAttribute: handleGovIdAttribute,
        HandleAccountNumberAttribute: handleAccountNumberAttribute,
        HandlePassportAttribute: handlePassportAttribute,
        HandleTelephoneAttributeChange: handleTelephoneAttributeChange,
        HandleMobilePhoneAttributeChange: handleMobilePhoneAttributeChange,
        HandleEmailAddressAttributeChange: handleEmailAddressAttributeChange,
        HandleDateTimeAttributeForFutureDateChange: handleDateTimeAttributeForFutureDateChange,
        HandleDateTimeAttributeForPastDateChange: handleDateTimeAttributeForPastDateChange,
        HandleGovIdAttributeChange: handleGovIdAttributeChange,
        HandleAccountNumberAttributeChange: handleAccountNumberAttributeChange,
        HandlePassportAttributeChange: handlePassportAttributeChange,
        HandleOnlyEnglishLettersInLowcase: handleOnlyEnglishLettersInLowcase,
        SetLookup: setLookup,
        GetAllDirtyFields: getAllDirtyFields,
        DisableAttributes: disableAttributes,
        DisableAllFormFields: disableAllFormFields,
        DisableSectionAttributesMode: disableSectionAttributesMode,
        GetLookupETC: getLookupETC,
        GenerateSelectFromArrayOfObjectsByProperty: generateSelectFromArrayOfObjectsByProperty,
        ClearFields: clearFields,
        HandleRecordLegalityCreation: handleRecordLegalityCreation,
        SaveModes: saveModes,
        IsUCI: isUCI,
        RequirementLevel: requirementLevel,
        DisplayState: displayState,
        NotifyEmptyRequiredReadOnlyFieldsOnQuickCreate: NotifyEmptyRequiredReadOnlyFieldsOnQuickCreate,
        SetAttributeRequiredLevel: setAttributeRequiredLevel,
        SetControlDisabledMode: setControlDisabledMode,
        SetControlVisibleMode: setControlVisibleMode,
        HandleLandlinePhoneNumberAttributeChange: handleLandlinePhoneNumberAttributeChange,
        HandleLandlinePhoneNumberAttribute: handleLandlinePhoneNumberAttribute,
        CommonRequestFailedMessage: COMMON_REQUEST_FAILED_MESSAGE,
        IsGridButtonEnabledByPrimaryEntityName: isGridButtonEnabledByPrimaryEntityName,
        DoesFormHaveFooter: doesFormHaveFooter,
        DoesFormHaveHeader: doesFormHaveHeader,
        SetTabVisibilityMode: setTabVisibilityMode,
        ClearAttributeValue: clearAttributeValue,
        HandleControlsVisibleMode: handleControlsVisibleMode,
        SaveFormSettings: saveFormSettings,
        DisplayAttributesWithValue: displayAttributesWithValue,
        SetHiddenFieldsToUnRequired: setHiddenFieldsToUnRequired,
        SetSectionVisibleMode: setSectionVisibleMode
    };

})(window.Utils.CrmPage = window.Utils.CrmPage || {});
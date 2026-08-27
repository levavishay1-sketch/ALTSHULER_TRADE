
if (typeof (Utils) === "undefined")
    Utils = {};

Utils.UIHandler = (function () {
    const JSON_INVALID_FORMAT_MESSAGE = "UIHandler Error: Invalid Json format: ";
    const SETTINGS_INVALID_MESSAGE = "הגדרות תצוגה לא תקינות";

    let jsonObject = {};
    let errors = [];
    let hashSet = {};
    let fieldsDefaultSettings = {};
    let retrivesInProgress = 0;

    const writeToLog = true;
    const writeToConsole = true;

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

    const controlType = {
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

    const messageLevel = {
        Information: 1,
        Warning: 2,
        Error: 3,
        Critical: 4
    };

    const CrmDataTypes = {
        Int: "int",
        String: "string",
        EntityRefernce: "EntityReference",
        Bool: "bool",
        DateTime: "DateTime",
        OptionSet: "OptionSetValue",
        Money: "Money"
    };

    let publisher = "alt_";
    const uiHandlerEntityName = "UIHandler";
    let jsonFieldName = publisher + "json";
    let codeFieldName = publisher + "codeint";
    let entityFieldName = publisher + "entityname";
    let currentEntityName;
    let formContext;

    const initialize = function (context, primaryTabName, organizationPrefix) {
        if (context) {
            formContext = context;
            currentEntityName = formContext.data.entity.getEntityName();

            if (organizationPrefix) {
                publisher = organizationPrefix.toLowerCase();
                jsonFieldName = publisher + "json";
                codeFieldName = publisher + "codeint";
                entityFieldName = publisher + "entityname";
            }
            saveFormOnLoadSetting();
        }
        else {
            console.error(SETTINGS_INVALID_MESSAGE);
        }
        return this;
    };

    const setByLookupSuccessCallback = function (receivedData, lookupId, key) {
        let json = {};
        if (receivedData && receivedData[jsonFieldName]) {
            try {
                json = JSON.parse(receivedData[jsonFieldName]);
                hashSet[receivedData[codeFieldName]] = key + "/" + lookupId;
            } catch (e) {
                console.error(JSON_INVALID_FORMAT_MESSAGE + receivedData[jsonFieldName]);
                if (writeToLog) {
                    writeLog(JSON_INVALID_FORMAT_MESSAGE + '\n' + receivedData[jsonFieldName], messageLevel.Critical);
                }
            }
        }
        addJsonToObjectKey(json, key, lookupId);
    };

    const setByCodeSuccessCallback = function (receivedData, code, key) {
        let json = {};
        if (receivedData && receivedData[0] && receivedData[0][jsonFieldName]) {
            try {
                json = JSON.parse(receivedData[0][jsonFieldName]);
            } catch (e) {
                console.error(JSON_INVALID_FORMAT_MESSAGE + receivedData[0][jsonFieldName]);
                if (writeToLog) {
                    writeLog(JSON_INVALID_FORMAT_MESSAGE + '\n' + receivedData[0][jsonFieldName], messageLevel.Critical);
                }
            }
        }
        addJsonToObject(json, key);
        hashSet[code] = key;
    };

    const setByLookup = function (lookupName, key) {

        if (lookupName && key && isControlLookup(lookupName)) {
            console.log(Date.now() + ': Start setbyLookup -> Key: ' + key + ' LookupName: ' + lookupName);

            let lookupValue = formContext.getAttribute(lookupName).getValue() && formContext.getAttribute(lookupName).getValue()[0];
            if (lookupValue) {
                addKeyToMargeOrderArray(key);
                if (!jsonObject[key] || !jsonObject[key][lookupValue.id]) {

                    retriveJsonByLookupValue(lookupValue.entityType, lookupValue.id, key, setByLookupSuccessCallback);
                }
                else {
                    addJsonToCollectionToMarge(key, jsonObject[key][lookupValue.id]);
                }
            }
            else {
                console.log(Date.now() + ': Finish set -> Key: ' + key + ' Lookup value is null');
                clear(key);
            }
        }
        else {
            console.error(SETTINGS_INVALID_MESSAGE);
        }
        return this;
    };

    const setByCode = function (code, key) {

        if (code && key) {
            console.log(Date.now() + ': Start setByCode -> Key: ' + key + ' Code: ' + code);

            let jsonFromHashSet = hashSet[code] ? getJsonByKey(hashSet[code]) : null;
            if (!jsonFromHashSet) {
                addKeyToMargeOrderArray(key);
                retriveJsonByCode(code, key, setByCodeSuccessCallback);
            }
            else {

                addKeyToMargeOrderArray(key);
                addJsonToCollectionToMarge(key, jsonFromHashSet);
            }
        }
        else {
            console.error(SETTINGS_INVALID_MESSAGE);
        }
        return this;
    };

    const setByString = function (jsonString, key) {

        if (jsonString && key) {
            console.log(Date.now() + ': Start setByString -> Key: ' + key + ' JsonString: ' + jsonString);

            let json = {};
            try {
                json = JSON.parse(jsonString);
                setByJson(json, key);
            } catch (e) {
                console.error(JSON_INVALID_FORMAT_MESSAGE + jsonString);
                if (writeToLog) {
                    writeLog(JSON_INVALID_FORMAT_MESSAGE + '\n' + jsonString, messageLevel.Critical);
                }
            }
        }
        else {
            console.error(SETTINGS_INVALID_MESSAGE);
        }
        return this;
    };

    const setByJson = function (json, key) {

        if (json && key) {
            console.log(Date.now() + ': Start setByJson -> Key: ' + key + ' Json: ' + JSON.stringify(json));

            addKeyToMargeOrderArray(key);
            addJsonToObject(json, key);
        }
        else {
            console.error(SETTINGS_INVALID_MESSAGE);
        }
        return this;
    };

    const getJsonByKey = function (key) {

        let keyParts = key.split("/");
        return keyParts.length > 1 ? jsonObject[keyParts[0]][keyParts[1]] : jsonObject[keyParts[0]];
    };

    const renderUI = function (renderOrderArray, callback) {

        if (retrivesInProgress > 0) {
            setTimeout(function () {
                renderUI(renderOrderArray, callback);
            }, 150);
        }
        else {
            console.log(Date.now() + ": Start renderUI");
            jsonObject.mergedJson = null;
            let renderOrder = renderOrderArray ? renderOrderArray : jsonObject.jsonsToMarge;

            if (renderOrder && renderOrder.length > 0 && jsonObject.collectionToMarge) {
                renderOrder.forEach(function (key) {
                    merge(jsonObject.collectionToMarge[key]);
                });
                resetSettings();
                setFormSettings();
            } else {
                resetSettings();
            }
            console.log(Date.now() + ": Finish renderUI");
            printErrors();
            printToConsole();
            if (callback) {
                callback();
            }
        }
    };

    // --------- add to json object ----------

    const addKeyToMargeOrderArray = function (key) {

        if (!jsonObject.jsonsToMarge) {
            jsonObject.jsonsToMarge = [];
        }
        else {
            clearKeyFromJsonsToMargeArray(key);
        }
        jsonObject.jsonsToMarge.push(key);
    };

    const addJsonToObject = function (json, key, value) {

        jsonObject[key] = json;
        addJsonToCollectionToMarge(key, json);
    };

    const addJsonToObjectKey = function (json, key, value) {
        if (key && value && json) {
            if (!jsonObject[key]) {
                jsonObject[key] = {};
            }
            jsonObject[key][value] = json;
            addJsonToCollectionToMarge(key, json);
        }
    };

    const addJsonToCollectionToMarge = function (key, json) {
        if (key && json) {
            if (!jsonObject.collectionToMarge) {
                jsonObject.collectionToMarge = {};
            }
            jsonObject.collectionToMarge[key] = json;
            console.log(Date.now() + ": Finish set key " + key);
        }
    };

    // -------------- clear ---------------

    const clear = function (keys) {
        if (!keys) {
            jsonObject.jsonsToMarge = [];
            jsonObject.collectionToMarge = {};
        }
        if (keys) {
            if (keys.constructor === Array) {
                keys.forEach(function (key) {
                    clearKeyFromJsonsToMargeArray(key);
                    clearJsonFromCollectionToMarge(key);
                });
            }
            else {
                clearKeyFromJsonsToMargeArray(keys);
                clearJsonFromCollectionToMarge(keys);
            }
        }
        return this;
    };

    const clearJsonFromCollectionToMarge = function (key) {
        if (jsonObject.collectionToMarge && key && jsonObject.collectionToMarge[key]) {
            delete jsonObject.collectionToMarge[key];
        }
    };

    const clearKeyFromJsonsToMargeArray = function (key) {
        if (jsonObject.jsonsToMarge && key) {
            clearFromArray(jsonObject.jsonsToMarge, key);
        }
    };

    const clearFromArray = function (array, key) {

        let index = array.indexOf(key);
        if (index >= 0) {
            array.splice(index, 1);
        }
    };

    // -------------- retrive -------------

    const retriveJsonByCode = function (code, key, successCallback) {
        retrivesInProgress++;
        let query = publisher +
            getEntityPluralName(uiHandlerEntityName).toLowerCase() +
            "?$select=" + jsonFieldName + "," + codeFieldName +
            "&$filter=" + codeFieldName + " eq " + code + " and " + entityFieldName + " eq " + "'" + currentEntityName + "'";
        var serverUrl = Xrm.Utility.getGlobalContext().getClientUrl();

        var req = new XMLHttpRequest();
        var reqUrl = serverUrl + "/api/data/v9.0/" + query;
        req.open("GET", reqUrl, true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var response = JSON.parse(this.response);
                    let receivedData = response.value ? response.value : response;
                    console.log(receivedData);
                    if (successCallback) {
                        successCallback(receivedData, code, key);
                    }
                } else {
                    clearKeyFromJsonsToMargeArray(key);
                    receivedData = JSON.parse(this.req.responseText).error;
                    console.error(receivedData.message);
                    let dataObj = { "key": key, "code": code };
                    writeHTTPRequestErrorLog("GET", reqUrl, receivedData.message, dataObj);
                }
                retrivesInProgress--;
            }
        };
        req.send();
    };

    const retriveJsonByLookupValue = function (entityType, id, key, successCallback) {
        retrivesInProgress++;
        let fieldName = publisher + capitalize(currentEntityName) + uiHandlerEntityName + "Id";
        let query = getEntityPluralName(entityType) + "(" + removeBraces(id) + ")" +
            "?$expand=" + fieldName + "($select=" + jsonFieldName + "," + codeFieldName + ")";
        var serverUrl = Xrm.Utility.getGlobalContext().getClientUrl();
        let receivedData;
        var req = new XMLHttpRequest();
        req.open("GET", serverUrl + "/api/data/v9.0/" + query, true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var response = JSON.parse(this.response);
                    receivedData = response.value ? response.value : response;
                    console.log(receivedData);
                    if (successCallback) {
                        successCallback(receivedData[fieldName], id, key);
                    }

                } else {
                    clearKeyFromJsonsToMargeArray(key);
                    receivedData = JSON.parse(this.req.responseText).error;
                    console.error(receivedData.message);
                    let dataObj = { "key": key, "id": id };
                    writeHTTPRequestErrorLog("GET", reqUrl, receivedData.message, dataObj);
                }
                retrivesInProgress--;
            }
        };
        req.send();
    };

    // -------------- merge ---------------

    const merge = function (json) {

        if (json && json.tabs) {
            if (!jsonObject.mergedJson) {
                jsonObject.mergedJson = JSON.parse(JSON.stringify(json));
            }
            mergeTabs(json.tabs);
        }
    };

    const mergeTabs = function (jsonTabs) {
        if (jsonTabs) {
            jsonTabs.forEach(function (jsonTab) {
                addTabNameToTabsArray(jsonTab.name);
                let mergedTab = getObjectByName(jsonObject.mergedJson.tabs, jsonTab.name);

                if (jsonTab.visible === "false" || jsonTab.visible === false) {
                    if (mergedTab) {
                        clearFromArray(jsonObject.mergedJson.tabs, mergedTab);
                    }
                }
                else if (!mergedTab) {
                    jsonObject.mergedJson.tabs.push(JSON.parse(JSON.stringify(jsonTab)));
                }
                else if (jsonTab.sections && jsonTab.sections.length > 0) {
                    if (!mergedTab.sections) {
                        mergedTab.sections = JSON.parse(JSON.stringify(jsonTab.sections));
                    }
                    else {
                        mergeSections(jsonTab.sections, mergedTab);
                    }
                }
            });
        }
    };

    const mergeSections = function (jsonSections, mergedTab) {
        if (jsonSections, mergedTab) {
            jsonSections.forEach(function (jsonSection) {

                let mergedSection = getObjectByName(mergedTab.sections, jsonSection.name);

                if (jsonSection.visible === "false" || jsonSection.visible === false) {
                    if (mergedSection) {
                        clearFromArray(mergedTab.sections, mergedSection);
                    }
                }
                else if (!mergedSection) {
                    mergedTab.sections.push(JSON.parse(JSON.stringify(jsonSection)));
                }
                else if (jsonSection.controls) {
                    if (!mergedSection.controls) {
                        mergedSection.controls = JSON.parse(JSON.stringify(jsonSection.controls));
                    }
                    else {
                        mergeControls(jsonSection.controls, mergedSection);
                    }
                }
            });
        }
    };

    const mergeControls = function (jsonControls, mergedSection) {

        if (jsonControls && mergedSection) {
            jsonControls.forEach(function (jsonControl) {

                let mergedControl = getObjectByName(mergedSection.controls, jsonControl.name);
                if (!mergedControl) {
                    mergedSection.controls.push(JSON.parse(JSON.stringify(jsonControl)));
                }
                else {
                    if (jsonControl.visible) {
                        mergedControl.visible = jsonControl.visible;
                    }
                    if (jsonControl.disabled) {
                        mergedControl.disabled = jsonControl.disabled;
                    }

                    if (mergedControl.required) {
                        mergedControl.required = jsonControl.required;
                    }
                }
            });
        }
    };

    // ------------- set settings -------------

    const setFormSettings = function () {

        if (jsonObject.mergedJson && jsonObject.mergedJson.tabs) {

            jsonObject.mergedJson.tabs.forEach(function (jsonTab) {

                let formTab = formContext.ui.tabs.get(jsonTab.name);
                if (formTab) {
                    setTabSettings(formTab, jsonTab);
                }
                else {
                    addError(jsonTab.name);
                }
            });
        }
    };

    const setTabSettings = function (formTab, jsonTab) {

        formTab.setVisible(true);

        if (jsonTab.sections) {
            formTab.sections.forEach(function (formSection) {
                let formSectionName = formSection.getName();
                let jsonSection = getObjectByName(jsonTab.sections, formSectionName);
                if (jsonSection) {
                    setSectionSettings(formSection, jsonSection, jsonTab.name);
                }
            });
            checkJsonTabSectionsForErrors(formTab, jsonTab);
        }
        if (!isUCI() && jsonTab.displayState) {
            formTab.setDisplayState(jsonTab.displayState);
        }
    };

    const setSectionSettings = function (formSection, jsonSection, formTabName) {

        formSection.setVisible(true);
        if (formSection.controls) {
            formSection.controls.forEach(function (formControl) {

                let controlOrAttributeName = doesControlHaveAttribute(formControl) ? formControl.getAttribute().getName() : formControl.getName();
                let jsonControl = getObjectByName(jsonSection.controls, controlOrAttributeName);

                if (jsonControl) {
                    if (doesControlHaveAttribute(formControl)) {
                        setAttributeSettings(formControl, jsonControl, jsonSection.name, formTabName);
                    }
                    else {
                        setControlSettings(formControl, jsonControl);
                    }
                }
            });
            checkJsonSectionControlsForErrors(formSection, jsonSection, formTabName);
        }
    };

    const setAttributeSettings = function (formControl, jsonControl, sectionName, tabName) {

        if (formControl && jsonControl && sectionName && tabName) {
            if (jsonControl.visible === "false" || jsonControl.visible === false) {
                formControl.setVisible(false);
            }
            else {
                let attribute = formControl.getAttribute();
                let attributeName = attribute.getName();
                let fieldDefaultSettings = getFieldDefaultSettings(tabName, sectionName, attributeName);

                let disabledMode = jsonControl.disabled ? jsonControl.disabled : fieldDefaultSettings && fieldDefaultSettings["disabled"];
                let requiredLevel = jsonControl.required ? jsonControl.required : fieldDefaultSettings && fieldDefaultSettings["required"];
                formControl.setDisabled(disabledMode);
                attribute.setRequiredLevel(requiredLevel);
                formControl.setVisible(true);
            }
        }
    };

    const setControlSettings = function (formControl, jsonControl) {
        if (jsonControl.visible === "false" || jsonControl.visible === false) {

            formControl.setVisible(false);
        }
        else {
            formControl.setVisible(true);
        }
    };

    // ----------- reset ------------

    const resetSettings = function () {

        if (jsonObject.tabsArray && jsonObject.mergedJson && jsonObject.mergedJson.tabs) {
            // Reset settings to default which are not in merged json
            jsonObject.tabsArray.forEach(function (tabName) {
                let formTab = formContext.ui.tabs.get(tabName);
                if (formTab) {
                    let jsonTab = getObjectByName(jsonObject.mergedJson.tabs, tabName);
                    if (!jsonTab) {
                        resetTabSettings(formTab);
                    }
                    else {
                        formTab.sections.forEach(function (formSection) {
                            let sectionName = formSection.getName();
                            let jsonSection = getObjectByName(jsonTab.sections, sectionName);
                            if (!jsonSection) {
                                resetSectionSettings(formSection, tabName);
                            }
                            else {
                                if (formSection.controls) {
                                    formSection.controls.forEach(function (control) {
                                        let controlOrAttributeName = doesControlHaveAttribute(control) ? control.getAttribute().getName() : control.getName();

                                        let jsonControls = getObjectByName(jsonSection.controls, controlOrAttributeName);
                                        if (!jsonControls) {
                                            if (doesControlHaveAttribute(control)) {
                                                resetAttributeSettings(control, sectionName, tabName);
                                            }
                                            else {
                                                resetControlSettings(control, sectionName, tabName);
                                            }
                                        }
                                    });
                                }
                            }
                        });
                    }
                }
            });
        }
        else {
            resetDefaultFormSettings();
        }
    };

    const resetTabSettings = function (formTab) {
        if (formTab) {
            let formTabName = formTab.getName();
            formTab.sections.forEach(function (tabSection) {
                resetSectionSettings(tabSection, formTabName);
            });
            formTab.setVisible(false);
        }
    };

    const resetSectionSettings = function (tabSection, formTabName) {
        if (tabSection && formTabName) {
            let formSectionName = tabSection.getName();
            if (tabSection.controls) {
                tabSection.controls.forEach(function (control) {
                    if (doesControlHaveAttribute(control)) {
                        resetAttributeSettings(control, formSectionName, formTabName);
                    }
                    else {
                        resetControlSettings(control, formSectionName, formTabName);
                    }
                });
            }
            tabSection.setVisible(false);
        }
    };

    const resetAttributeSettings = function (control, sectionName, tabName) {

        let attribute = control.getAttribute();
        let attributeName = attribute.getName();
        let fieldSettings = getFieldDefaultSettings(tabName, sectionName, attributeName);
        if (fieldSettings) {
            attribute.setRequiredLevel(fieldSettings["required"]);
            control.setVisible(fieldSettings["visible"]);
            control.setDisabled(fieldSettings["disabled"]);
        }
        if (attribute.getIsDirty()) {
            resetAttributeValue(attribute, fieldSettings["value"]);

        }
    };

    const resetControlSettings = function (control, sectionName, tabName) {

        if (control && sectionName && tabName) {

            let controlName = control.getName();
            let fieldSettings = getFieldDefaultSettings(tabName, sectionName, controlName);

            if (fieldSettings) {
                control.setVisible(fieldSettings["visible"]);
            }
        }
    };

    const resetAttributeValue = function (attribute, value) {

        switch (attribute.getAttributeType()) {
            case attributeType.Boolean:
            case attributeType.Optionset:
            case attributeType.Multioptionset: {
                attribute.setValue(value);
                attribute.fireOnChange();
                break;
            }
            default:
                {
                    attribute.setValue(value);
                    break;
                }
        }

    };

    const resetDefaultFormSettings = function () {
        if (jsonObject.tabsArray && jsonObject.tabsArray.length > 0) {
            jsonObject.tabsArray.forEach(function (tabName) {
                let formTab = formContext.ui.tabs.get(tabName);
                resetTabSettings(formTab);
            });
        }
    };

    // ----------- errors -----------

    const checkJsonTabSectionsForErrors = function (formTab, jsonTab) {
        jsonTab.sections.forEach(function (jsonSection) {
            if (!formTab.sections.get(jsonSection.name)) {
                addError(jsonTab.name, jsonSection.name);
            }
        });
    };

    const checkJsonSectionControlsForErrors = function (formSection, jsonSection, formTabName) {
        if (jsonSection.controls) {
            jsonSection.controls.forEach(function (jsonControl) {
                if (!formSection.controls.get(jsonControl.name)) {
                    addError(formTabName, jsonSection.name, jsonControl.name);
                }
            });
        }
    };

    const addError = function (tabName, sectionName, controlName) {
        let errorMessage;
        if (tabName && sectionName && controlName) {

            errorMessage = "Control name " + controlName + " in section name " + sectionName + " in tab name " + tabName + " do not exist.";
        }
        else if (tabName && sectionName) {
            errorMessage = "Section name " + sectionName + " in tab name " + tabName + " do not exist.";
        }
        else if (tabName) {
            errorMessage = "Tab name " + tabName + " do not exist.";
        }
        if (errorMessage && !isInArray(errors, errorMessage)) {
            errors.push(errorMessage);
        }
    };

    const printErrors = function () {
        if (errors.length > 0) {
            let messageToLog = "UIHandler Error: ";
            errors.forEach(function (error) {
                console.error(error);
                messageToLog += "\n" + error;
            });
            if (writeToLog) {
                writeLog(messageToLog, messageLevel.Error);
            }
        }
    };

    // ------------------------------

    const isInArray = function (array, item) {

        if (array && item) {
            return array.indexOf(item) >= 0;
        }
        return false;
    };

    const isControlLookup = function (controlName) {
        let control = formContext.getControl(controlName);
        let controlType = control.getControlType();
        return controlType === attributeType.Lookup;
    };

    const getFieldDefaultSettings = function (tabName, sectionName, attributeName) {
        return fieldsDefaultSettings[tabName]
            && fieldsDefaultSettings[tabName][sectionName]
            && fieldsDefaultSettings[tabName][sectionName][attributeName];
    };

    const doesControlHaveAttribute = function (control) {
        var type = control.getControlType();
        switch (type) {
            case controlType.Standard:
            case controlType.Lookup:
            case controlType.Optionset: {
                return true;
            }
            default: {
                return false;
            }
        }
    };

    const addTabNameToTabsArray = function (tabName) {
        let formTab = formContext.ui.tabs.get(tabName);
        if (formTab) {
            if (!jsonObject.tabsArray) {
                jsonObject.tabsArray = [];
            }
            if (!isInArray(jsonObject.tabsArray, tabName)) {
                jsonObject.tabsArray.push(tabName);
            }
        }
    };

    const saveFormOnLoadSetting = function () {
        formContext.ui.tabs.forEach(function (formTab) {
            let tabName = formTab.getName();
            fieldsDefaultSettings[tabName] = {};
            fieldsDefaultSettings[tabName]["visible"] = formTab.getVisible();
            fieldsDefaultSettings[tabName]["displayState"] = formTab.getDisplayState();
            formTab.sections.forEach(function (formSection) {
                let formSectionName = formSection.getName();
                fieldsDefaultSettings[tabName][formSectionName] = {};
                fieldsDefaultSettings[tabName][formSectionName]["visible"] = formSection.getVisible();
                formSection.controls.forEach(function (formControl) {
                    let controlOrAttributeName = doesControlHaveAttribute(formControl) ? formControl.getAttribute().getName() : formControl.getName();
                    fieldsDefaultSettings[tabName][formSectionName][controlOrAttributeName] = {};
                    if (doesControlHaveAttribute(formControl)) {
                        let attribute = formControl.getAttribute();
                        fieldsDefaultSettings[tabName][formSectionName][controlOrAttributeName]["required"] = attribute.getRequiredLevel();
                        fieldsDefaultSettings[tabName][formSectionName][controlOrAttributeName]["value"] = attribute.getValue();
                        fieldsDefaultSettings[tabName][formSectionName][controlOrAttributeName]["disabled"] = formControl.getDisabled();
                    }
                    fieldsDefaultSettings[tabName][formSectionName][controlOrAttributeName]["visible"] = formControl.getVisible();
                });
            });
        });
    };

    const getObjectByName = function (array, name) {

        if (array && name)
            for (let i = 0; i < array.length; i++) {
                if (array[i].name.toLowerCase() === name.toLowerCase()) {
                    return array[i];
                }
            }
        return null;
    };

    const getEntityPluralName = function (entityName) {

        if (entityName.endsWith('s') || entityName.endsWith('sh') || entityName.endsWith('ch') || entityName.endsWith('x') || entityName.endsWith('z')) {

            return entityName + 'es';
        }

        if (entityName.endsWith('y')) {

            return entityName.substr(0, entityName.length - 1) + 'ies';
        }

        return entityName + 's';
    };

    const removeBraces = function (string) {

        if (string && string.indexOf('{') !== -1 && string.indexOf('}') !== -1) {
            string = string.replace(/{|}/g, '');
        }

        return string;
    };

    let endsWith = function (string) {
        return this.substr(this.length - string.length) === string;
    };

    let capitalize = function (str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    };

    if (!String.prototype.endsWith) {
        String.prototype.endsWith = this.endsWith;
    }

    if (!String.prototype.capitalize) {
        String.prototype.capitalize = this.capitalize;
    }

    const isNullOrEmpty = function (string) {

        return !string || string === "" || string.trim().length === 0;
    };

    const isUCI = function () {
        let globalContext = Xrm.Utility.getGlobalContext();
        let currentAppUrl = globalContext.getCurrentAppUrl().toLowerCase();

        return (currentAppUrl.indexOf("appid") === -1) ? false : true;
    };

    const printToConsole = function () {
        if (writeToConsole) {
            console.log("Json Object ->");
            console.log(jsonObject);
            console.log("HashSet ->");
            console.log(hashSet);
            console.log("Default fields settings on form load ->");
            console.log(fieldsDefaultSettings);
        }
    };

    const messageBuilder = function (message) {

        let messageToLog = [];
        let stack = [];
        const entityName = formContext.data.entity.getEntityName();
        const recordId = formContext.data.entity.getId();
        let appUrl = Xrm.Utility.getGlobalContext().getCurrentAppUrl();
        let clientUrl = isUCI() ? "&pagetype=entityrecord&etn=" + entityName + "&id=" + recordId
            : appUrl + "/main.aspx?etn=" + entityName + "&id=" + recordId + "&pagetype=entityrecord";
        messageToLog.push('Url: ' + clientUrl);
        messageToLog.push(message);
        messageToLog.push('Stack Trace:');

        try {
            throw new Error('dummy');
        } catch (e) {
            if (e.stack) {
                stack = e.stack.replace(/^[^\(]+?[\n$]/gm, '')
                    .replace(/^\s+at\s+/gm, '')
                    .replace(/^Object.\s*\(/gm, '{anonymous}()@')
                    .split('\n');
                for (var i = 0; i < stack.length; i++) {
                    messageToLog.push(stack[i]);
                }
            }
            else {
                messageToLog.push("Not supported browser to log trace");
            }
        }
        return messageToLog;
    };

    const writeLog = function (message, messageLevel) {
        let webResourcesPath = formContext.data.entity.getEntityName();
        webResourcesPath += formContext.ui.tabs.get()[0].getName().toLowerCase() === "quickformtab" ? "QuickCreate.js" : "Main.js";
        let messageBlock = messageBuilder(message);
        let caller = messageBlock[5].split('(');
        let name = webResourcesPath + ' ⇒ ' + caller[0];
        let userSettings = Xrm.Utility.getGlobalContext().userSettings;
        const userId = userSettings.userId;
        let logMessage = [
            { 'key': 'MessageBlock', 'value': messageBlock.join('\n\n'), 'type': CrmDataTypes.String },
            { 'key': 'MessageLevelCode', 'value': messageLevel, 'type': CrmDataTypes.Int },
            { 'key': 'EntryPointTypeCode', 'value': 4, 'type': CrmDataTypes.Int },
            { 'key': 'OverrideCreatedOn', 'value': new Date(), 'type': CrmDataTypes.DateTime },
            { 'key': 'Name', 'value': name, 'type': CrmDataTypes.String },
            { 'key': 'ExecutingSystemUserId', 'value': userId, 'type': CrmDataTypes.String },
            { 'key': 'CorrelationId', 'value': '', 'type': CrmDataTypes.String },
            { 'key': 'Depth', 'value': 1, 'type': CrmDataTypes.Int },
            { 'key': 'PerformanceExecutionDuration', 'value': 1, 'type': CrmDataTypes.Int },
            { 'key': 'OperationDuration', 'value': 1, 'type': CrmDataTypes.Int },
            { 'key': 'RequestId', 'value': '', 'type': CrmDataTypes.String }
        ];
        callAction("alt_Logger", null, null, logMessage, false, null, null);
    };

    const writeHTTPRequestErrorLog = function (requestMessage, reqUrl, receivedData, dataObj) {

        let errorMessage = [];
        errorMessage.push('Request Message: ' + requestMessage);
        const queryParameters = Xrm.Utility.getGlobalContext().getQueryStringParameters();
        errorMessage.push('Query String Parameters: ' + JSON.stringify(queryParameters));
        errorMessage.push('Request Url: ' + reqUrl);

        if (receivedData) {
            errorMessage.push('Error : ' + JSON.stringify(receivedData));
        }
        if (dataObj) {
            errorMessage.push('Data Object : ' + JSON.stringify(dataObj));
        }

        writeLog(errorMessage.join('\n\n'), messageLevel.Critical);
    };

    const callAction = function (actionName, targetEntityName, targetId, dataArray, successCallBack, errorCallback) {

        const Types = CrmDataTypes;
        let data = null;

        if (dataArray) {
            data = new Object();

            for (let i = 0; i < dataArray.length; i++) {
                const item = dataArray[i];
                switch (item.type) {
                    case Types.Int: {
                        data[item.key] = item.value.toString();
                        break;
                    }
                    case Types.String: {
                        data[item.key] = item.value;
                        break;
                    }
                    case Types.EntityRefernce: {
                        const guid = item.value.entityActivity === true ? "activityid" : item.value.entityType + "id";
                        const refObj = {};
                        refObj[guid] = item.value.id;
                        refObj["@odata.type"] = "Microsoft.Dynamics.CRM." + item.value.entityType;
                        refObj[item.value.parameterName.key] = item.value.parameterName.value;

                        data[item.key] = refObj;
                        break;
                    }
                    case Types.Bool: {
                        data[item.key] = item.value;
                        break;
                    }
                    case Types.DateTime: {
                        data[item.key] = item.value;
                        break;
                    }
                    case Types.OptionSet: {
                        data[item.key] = item.value.toString();
                        break;
                    }
                    case Types.Money: {

                        break;
                    }
                    default:

                }
            }
        }

        let reqEntityName = "";
        let reqActionName = "";
        if (!isNullOrEmpty(targetEntityName) && !isNullOrEmpty(targetId)) {
            reqEntityName = getEntityPluralName(targetEntityName) + "(" + removeBraces(targetId) + ")/";
            reqActionName = "Microsoft.Dynamics.CRM." + actionName;
        }
        else {
            reqActionName = actionName;
        }
        const serverUrl = Xrm.Utility.getGlobalContext().getClientUrl();
        const webApiPath = serverUrl + "/api/data/v9.0/";
        reqUrl = webApiPath.toLowerCase() + reqEntityName.toLowerCase() + reqActionName;

        return executeHttpRequest("POST", reqUrl, successCallBack, errorCallback, data);

    };

    const executeHttpRequest = function (message, reqUrl, successCallBack, errorCallback, dataObj) {

        let receivedData = null;
        const request = new XMLHttpRequest();
        const isAsyncReq = true;
        request.open(message, encodeURI(reqUrl), isAsyncReq);

        request.setRequestHeader("Accept", "application/json");
        request.setRequestHeader("Content-Type", "application/json;charset=utf-8");
        request.setRequestHeader("OData-MaxVersion", "4.0");
        request.setRequestHeader("OData-Version", "4.0");
        request.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");

        request.onreadystatechange = function () {
            if (request.readyState === 4) {
                if (request.status === 200) {

                    const responce = JSON.parse(request.responseText);
                    receivedData = responce.value ? responce.value : responce;
                    console.log("success\n", receivedData);
                    if (successCallBack) successCallBack(receivedData);
                }
                else if (request.status === 204) {

                    receivedData = true;
                    if (successCallBack) successCallBack(receivedData);
                }
                else {
                    receivedData = JSON.parse(request.responseText).error;
                    writeHTTPRequestErrorLog(message, reqUrl, receivedData, dataObj);
                    console.error(receivedData.message);

                    if (errorCallback) errorCallback(receivedData);
                }
            }
        };

        if ((message === "POST" || message === "PATCH") && dataObj) {

            request.send(JSON.stringify(dataObj));
        }
        else {
            request.send();
        }

        return receivedData;
    };

    return {
        SetByLookup: setByLookup,
        SetByJson: setByJson,
        SetByCode: setByCode,
        Clear: clear,
        RenderUI: renderUI,
        SetByString: setByString,
        Initialize: initialize
    };
})(window.Utils.UIHandler = window.Utils.UIHandler || {});

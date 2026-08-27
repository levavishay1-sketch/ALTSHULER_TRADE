/// <reference path="Utils.Server.js" />

if (typeof (Utils) == "undefined")
    Utils = {};

Utils.Global = (function () {
    let formContext;
    const maxPadLength = 20;
    const getAllTeamsByUserId = function (context, userId, successCallback, errorCallback) {
        formContext = context;
        const select = 'teamid';
        const filter = 'systemuserid eq ' + userId;
        const chainingKeys = { key: 'Users', innerKey: { key: userId, innerKey: { key: 'userTeams' } } };


        return Utils.Server.RetrieveMultipleAndCache('teammembership', chainingKeys, select, filter, null, null, successCallback, errorCallback);
    };

    const getAllUsersByTeamId = function (context, teamId, successCallback, errorCallback) {
        formContext = context;
        const select = 'systemuserid';
        const filter = 'teamid eq ' + teamId;
        const chainingKeys = { key: 'Teams', innerKey: { key: teamId, innerKey: { key: 'teamUsers' } } };

        return Utils.Server.RetrieveMultipleAndCache('teammembership', chainingKeys, select, filter, null, null, successCallback, errorCallback);
    };

    const GetGlobalParamValue = function (paramKey, successCallback, errorCallback) {

        const filter = "alt_name eq '" + paramKey + "'";
        const chainingKeys = { key: 'GlobalParameters', innerKey: { key: paramKey } };

        Utils.Server.RetrieveMultipleAndCache("alt_globalparameter", chainingKeys, "alt_value", filter, null, null, function (result) {

            let paramValue = null;

            if (result) {
                paramValue = result[0]['alt_value'];
            }

            if (successCallback) {
                successCallback(paramValue);
            }
            else {
                return paramValue;
            }
        }, errorCallback);
    };

    const getAllActiveActivitiesByRegardingObject = function (regardingObjectid, successCallBack, errorCallback) {

        let fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
            "<entity name='activitypointer'>" +
            "<attribute name='subject' />" +
            "<attribute name='activitytypecode' />" +
            "<attribute name='statuscode' />" +
            "<attribute name='statecode' />" +
            "<filter type='and'>" +
            "<condition attribute='regardingobjectid' operator='eq' value='" + regardingObjectid + "' />" +
            "<condition attribute='statecode' operator='in'>" +
            "<value>3</value>" +
            "<value>0</value>" +
            "</condition>" +
            "</filter>" +
            "</entity>" +
            "</fetch>";
        Utils.Server.Fetch("activitypointer", fetchXml, successCallBack, errorCallback);
    };

    const AsyncEnableRoleButtonManager = (function () {
        const buttons = {};
        let formContext = null;
        function AsyncEnableRoleButton(isEnableButtonCheckCallback) {

            this.isOperationFinished = false;
            this.functionResult = null;
            this.isEnableButtonCheckCallback = isEnableButtonCheckCallback;
            this.init = function () {

                this.isOperationFinished = false;
                this.functionResult = null;
            };

            this.setOperatrionToExecutedHandler = function (result) {
                this.functionResult = result;
                this.isOperationFinished = true;
                formContext.ui.refreshRibbon();
            };

            this.asyncOperation = function (asyncOperationCallbackFunction) { // asyncOperationCallbackFunction could be promise

                asyncOperationCallbackFunction(this.setOperatrionToExecutedHandler.bind(this));
            };

            this.execute = function (asyncOperationCallbackFunction) {
                var isEnabled = false;

                if (this.isOperationFinished) {
                    isEnabled = this.isEnableButtonCheckCallback(this.functionResult)
                } else {
                    this.asyncOperation(asyncOperationCallbackFunction);
                }
                return isEnabled;
            };
        }

        const executeAsyncButtonOperation = function (context, buttonKey, enableCallback, asyncOperationCallbackFunction) {
            if (!buttons[buttonKey]) {
                formContext = context;
                buttons[buttonKey] = { button: new AsyncEnableRoleButton(enableCallback) };
            }
            return buttons[buttonKey].button.execute(asyncOperationCallbackFunction);
        };

        const initAsyncButtonOperation = function (buttonKey) {
            if (buttons[buttonKey]) {
                buttons[buttonKey].button.init();
            }
        };
        return {
            executeAsyncButtonOperation: executeAsyncButtonOperation,
            initAsyncButtonOperation: initAsyncButtonOperation,
        };
    })();

    const removeOptionsetValuesByGlobalParams = function (formContext, globalParamName, entityName, controlName) {
        GetGlobalParamValue(globalParamName, function (result) {
            if (result) {
                let optionSetsValuesToRemove = JSON.parse(result);

                let entityObject = optionSetsValuesToRemove.entities.filter(function (entity) {
                    return entity["entityName"] === entityName;
                });

                const valuesToRemove = getOptionSetsValuesToRemoveFromEntityObjectHandler(globalParamName, entityName, entityObject, controlName);
                if (valuesToRemove && valuesToRemove.length > 0) {
                    valuesToRemove.forEach(function (value) {
                        formContext.getControl(controlName).removeOption(value);
                    });
                }
            }
        }, null);
    };

    const getOptionSetsValuesToRemoveFromEntityObjectHandler = function (globalParamName, entityName, entityObject, controlName) {
        let messageToLog;
        let optionsToRemove = null;
        if (entityObject.length === 1) {
            let attributes = entityObject[0]["attributes"];
            let attributeObject = attributes.filter(function (attribute) {
                return attribute["name"] === controlName;
            });

            if (attributeObject.length === 1) {
                optionsToRemove = attributeObject[0]["values"];
            }
            else if (attributeObject.length === 0) {
                messageToLog = Utils.JsExtantions.String.Format(" {0} attribute from {1} entity not exist in {2} system param", controlName, entityName, globalParamName);
                Utils.Server.WriteLog(messageToLog, Utils.Server.MessageLevel.Warning);
            }
            else {
                messageToLog = Utils.JsExtantions.String.Format("Declared more then ones {0} attribute in {1} entity in {2} system param", controlName, entityName, globalParamName);
                Utils.Server.WriteLog(messageToLog, Utils.Server.MessageLevel.Warning);
            }
        }
        else if (entityObject.length === 0) {
            messageToLog = Utils.JsExtantions.String.Format("{0} entity not exist in {1} system param", entityName, globalParamName);
            Utils.Server.WriteLog(messageToLog, Utils.Server.MessageLevel.Warning);
        }
        else {
            messageToLog = Utils.JsExtantions.String.Format("Declared the entity : {0}, more than one time in {1} system param", entityName, globalParamName);
            Utils.Server.WriteLog(messageToLog, Utils.Server.MessageLevel.Error);
        }
        return optionsToRemove;
    };

    const getSystemUserDefaultTeam = function (context, userId, successCallback, errorCallback) {
        formContext = context;
        const chainingKeys = { key: 'Users', innerKey: { key: userId, innerKey: { key: 'defaultTeam' } } };
        return Utils.Server.RetrieveAndCache('systemuser', userId, chainingKeys, '_alt_defaultteamid_value', successCallback || getSystemUserDefaultTeamSuccessCallback, errorCallback);
    };

    const getSystemUserDefaultTeamSuccessCallback = function (result) {
        const team = result.alt_defaultteamid;
        if (team.Id) {
            Utils.CrmPage.SetLookup(formContext, 'ownerid', team.Id, team.Name, team.LogicalName)
        }
    };

    const CustomerFilterManager = (function () {

        let isAddedPreSearch = false;
        let customerControlName;
        let isSetDefaultValue;
        let relatedCustomers = {
            Accounts: {
                EntityName: "accountid",
                Ids: []
            },
            Contacts: {
                EntityName: "contactid",
                Ids: []
            }
        };
        const customerTypeCode = {
            Account: 1,
            Contact: 2,
            Customer: 3
        };
        let customerType;
        const addCustomerCustomFilter = function (formcontext, customerAttrubuteName, fetchPrimaryEntityName, fetchXml, fetchCustomerAttributeName, setDefaultValue, customerEntityTypeCode) {
            formContext = formcontext;
            customerControlName = customerAttrubuteName;
            isSetDefaultValue = setDefaultValue;
            relatedCustomers.Contacts.Ids = [];
            relatedCustomers.Accounts.Ids = [];
            customerType = customerEntityTypeCode ? customerEntityTypeCode : customerTypeCode.Customer;
            if (fetchPrimaryEntityName && fetchXml && fetchCustomerAttributeName) {
                Utils.Server.Fetch(fetchPrimaryEntityName, fetchXml, function (result) {
                    if (result) {
                        if (result.length === 1 && isSetDefaultValue) {
                            let defaultValue = result[0][fetchCustomerAttributeName];
                            Utils.CrmPage.SetLookup(formContext, customerControlName, defaultValue.Id, defaultValue.Name, defaultValue.LogicalName);
                            formContext.getAttribute(customerControlName).fireOnChange();
                        }
                        result.forEach(function (accountHolder) {
                            addToRelatedCustomers(accountHolder[fetchCustomerAttributeName]);
                        });
                    }
                    filterCustomers();
                }, function (error) {
                    filterCustomers();
                    Xrm.Navigation.openAlertDialog({ text: "איורע שגיאה בסינון לקוח. נא פנה למנהל מערכת." });
                });
            }
            else {
                filterCustomers();
            }

        };

        const addToRelatedCustomers = function (customer) {
            if (customer.LogicalName === "contact") {
                relatedCustomers.Contacts.Ids.push(customer.Id);
            }
            else {
                relatedCustomers.Accounts.Ids.push(customer.Id);
            }
        };

        const createCustomerFilter = function (customers) {

            let filter = "<filter type='or'>";
            if (!customers || customers.Ids.length === 0) {
                filter += "<condition attribute='" + customers.EntityName + "' operator='eq' value='{00000000-0000-0000-0000-000000000000}' />";
            } else {
                customers.Ids.forEach(function (relatedCustomerId) {
                    filter += "<condition attribute='" + customers.EntityName + "' operator='eq' value='{" + relatedCustomerId + "}' />";
                });
            }
            filter += "</filter>";
            return filter;
        };

        const addCustomFilter = function () {

            const customerControl = formContext.getControl(customerControlName);
            switch (customerType) {
                case customerTypeCode.Account: {
                    customerControl.addCustomFilter(createCustomerFilter(relatedCustomers.Accounts), 'account');
                    break;
                }
                case customerTypeCode.Contact: {
                    customerControl.addCustomFilter(createCustomerFilter(relatedCustomers.Contacts), 'contact');
                    break;
                }
                case customerTypeCode.Customer: {
                    customerControl.addCustomFilter(createCustomerFilter(relatedCustomers.Contacts), 'contact');
                    customerControl.addCustomFilter(createCustomerFilter(relatedCustomers.Accounts), 'account');
                    break;
                }
                default:
            }
        };

        const filterCustomers = function () {

            if (isAddedPreSearch) {
                addCustomFilter();
            }
            else {
                isAddedPreSearch = true;
                formContext.getControl(customerControlName).addPreSearch(addCustomFilter);
            }
        };

        return {
            addCustomerCustomFilter: addCustomerCustomFilter
        };
    })();

    const createLookupCustomFilter = function (filteredObjects, filteredFieldName) {
        let filter = "<filter type='or'>";
        if (!filteredObjects || filteredObjects.length === 0) {
            filter += "<condition attribute='" + filteredFieldName + "' operator='eq' value='{00000000-0000-0000-0000-000000000000}' />";
        } else {
            filteredObjects.forEach(function (objectId) {
                filter += "<condition attribute='" + filteredFieldName + "' operator='eq' value='{" + objectId + "}' />";
            });
        }
        filter += "</filter>";
        return filter;
    };

    const addQueryStringParamsToJoiningFormURL = function (formContext, urlAttributeName, successCallback, errorCallback) {

        const urlAttribute = formContext.getAttribute(urlAttributeName);
        if (urlAttribute && urlAttribute.getValue()) {
            let newUrlValue = urlAttribute.getValue();
            const userSettings = Xrm.Utility.getGlobalContext().userSettings;
            const currentUserId = Utils.JsExtantions.String.RemoveBraces(userSettings.userId);
            newUrlValue += currentUserId;

            urlAttribute.setValue(newUrlValue);
            urlAttribute.setSubmitMode('never');
            if (successCallback) {
                successCallback();
            }
            if (errorCallback) {
                errorCallback();
            }
        }
    };

    var generateLookupObjectPropertyName = function (entityName, complexEntityType) {

        return complexEntityType ? complexEntityType + entityName + "@odata.bind" : entityName + "@odata.bind";
    };

    var generateLookupObjectValue = function (entityName, entityId) {

        let id = Utils.JsExtantions.String.RemoveBraces(entityId);
        return "/" + Utils.JsExtantions.Entity.GetEntityPluralName(entityName) + "(" + id + ")";
    };

    var generateParserCustomEntryPointEntityReference = function (entityName, entityId) {
        let entityReference = {

            "LogicalName": entityName,
            "Id": Utils.JsExtantions.String.RemoveBraces(entityId)
        };
        return JSON.stringify(entityReference);
    };

    var DocumentGrid = (function () {
        let gridLoadExecutionaContext = null;
        const searchFileFromArchiveTimeout = 2 * 60;
        const archiveMessages = {
            LoadingFromArchiveMessage: "טוען מסמכים מארכיון...",
            SearchTimeoutMessage: "חריגה בזמן בטעינת מסמכים מארכיון",
            SearchErrorMessage: "שגיאה בטעינת מסמכים מארכיון"
        };

        var LoadDocumentGrid = function (entityName, entityIdWithoutBraces, onLoadExecutionaContext) {
            createOrRetrieveDocumentSearchEntity(entityIdWithoutBraces, entityName);
            gridLoadExecutionaContext = onLoadExecutionaContext;
        }

        const createOrRetrieveDocumentSearchEntity = function (entityID, entityLogicalName) {

            const documentSearchEntityLogicalName = 'alt_documentsearchforentity';
            const select = 'alt_lastsearchdate';
            const filter = '_regardingobjectid_value eq ' + entityID

            Utils.Server.RetrieveMultiple(documentSearchEntityLogicalName, select, filter, null, null, function (receivedData) {
                if (receivedData) {
                    console.log(receivedData[0].alt_lastsearchdate)
                    const retrievedTime = new Date(receivedData[0].alt_lastsearchdate);
                    const currentTime = new Date();
                    if (((currentTime - retrievedTime) / 1000) / 60 > 15) {
                        callSearchFilesCustomAction(receivedData[0].activityid, documentSearchEntityLogicalName);
                    }
                }
                else {
                    createDocumentSearchEntity(entityID, entityLogicalName)
                }
            })
        };

        const callSearchFilesCustomAction = function (entityID, entityLogicalName) {
            var documentSearchData = {
                EntityID: entityID,
                EntityLogicalName: entityLogicalName
            };

            var actionInput = [{
                key: 'Data',
                value: JSON.stringify(documentSearchData),
                type: Utils.Server.CrmDataTypes.String
            }];

            Xrm.Utility.showProgressIndicator(archiveMessages.LoadingFromArchiveMessage);

            Utils.Server.CallAction('alt_SearchFiles', entityLogicalName, null, actionInput,
                function (res) {
                    let timer = 0;
                    let id = setInterval(check, 1000)
                    function check() {
                        if (timer >= searchFileFromArchiveTimeout) {
                            Xrm.Navigation.openAlertDialog({ text: archiveMessages.SearchTimeoutMessage });
                            clearInterval(id);
                            Xrm.Utility.closeProgressIndicator();
                        }
                        else {
                            Utils.Server.Retrieve(entityLogicalName, entityID, "alt_searchfromarchivestatus", null,
                                function (result) {
                                    console.log(result.alt_searchfromarchivestatus);
                                    if (result.alt_searchfromarchivestatus == 4 || result.alt_searchfromarchivestatus == 5) {
                                        clearInterval(id);
                                        Xrm.Utility.closeProgressIndicator();
                                        Xrm.Page.data.refresh();
                                    }
                                }, null)
                            timer += 1;
                        }
                    }
                    console.log(res);
                }, function (error) {
                    console.log(error);
                    Xrm.Navigation.openAlertDialog({ text: archiveMessages.SearchErrorMessage });
                    Xrm.Utility.closeProgressIndicator();
                });
        };

        const createDocumentSearchEntity = function (entityId, entityName) {
            let entitySetName;

            Xrm.Utility.getEntityMetadata(entityName, "")
                .then(function (result) {
                    entitySetName = result.EntitySetName;
                    let data = {
                        "subject": `document search for ${entityName} - ${entityId}`
                    }
                    data[`regardingobjectid_${entityName}@odata.bind`] = `/${entitySetName}(${entityId})`;

                    Xrm.WebApi.createRecord('alt_documentsearchforentity', data).then(
                        function success(result) {
                            console.log("new search created - " + result.id);
                            CallSearchFilesCustomAction(result.id, result.entityType);
                        },
                        function (error) {
                            console.log(error.message);
                        }
                    );
                }, function (error) {
                    console.log(error);
                });
        };
        return { LoadDocumentGrid: LoadDocumentGrid }
    })();


    var padLeftString = function (textToPad) {
        return textToPad.length < maxPadLength ? padLeftString("0" + textToPad, maxPadLength) : textToPad;
    };

    const getContactByGovernmentId = function (governmentId, select, successCallback, errorCallback) {

        if (governmentId) {
            const padedLeftGovernmentId = padLeftString(governmentId);
            const filter = "alt_internalgovernmentid eq '" + padedLeftGovernmentId + "'";
            Utils.Server.RetrieveMultiple("contact", select, filter, null, null, successCallback, errorCallback);
        }
        else if (successCallback) {
            successCallback(null);
        }
        else {
            return null;
        }
    };

    const getAccountByAccountNumber = function (accountNumber, select, successCallback, errorCallback) {
        if (accountNumber) {
            const padedLeftAccountNumber = padLeftString(accountNumber);
            const filter = "alt_internalaccountnumber eq '" + padedLeftAccountNumber + "'";
            Utils.Server.RetrieveMultiple("account", select, filter, null, null, successCallback, errorCallback);
        }
        else if (successCallback) {
            successCallback(null);
        }
        else {
            return null;
        }
    };

    const getActiveAccountHoldersByRelatedEntity = function (relatedEntityId, successCallback, errorCallback) {

        const fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
            "<entity name='alt_accountholder'>" +
            "<attribute name='alt_name' />" +
            "<attribute name='alt_email' />" +
            "<attribute name='alt_mobilephone' />" +
            "<attribute name='alt_customerid' />" +
            "<attribute name='alt_accountholderid' />" +
            "<filter type='and'>" +
            "<condition attribute='statecode' operator='eq' value='0' />" +
            "<condition attribute='" + relatedEntityId.entityType + "id' value='" + relatedEntityId.id + "' operator='eq' /> " +
            "</filter>" +
            "</entity>" +
            "</fetch>";

        Utils.Server.Fetch('alt_accountholder', fetchXml, function (result) {
            if (result) {
                successCallback(result);
            }
        }, function (error) {
            if (errorCallback) {
                errorCallback(error);
            }
        });
    };

    const createAccountHoldersCustomerFilter = function (accountHolders, customerEntityName) {

        let filter = "<filter type='or'>";
        let emptyFilterCondition = "<condition attribute='" + customerEntityName + "id" + "' operator='eq' value='{00000000-0000-0000-0000-000000000000}' />";
        if (!accountHolders) {
            filter += emptyFilterCondition;
        }
        else {
            const customers = accountHolders.filter(function (value) {
                return value.alt_customerid && value.alt_customerid.LogicalName == customerEntityName;
            });
            if (!customers || customers.length === 0) {
                filter += emptyFilterCondition;
            } else {
                customers.forEach(function (accountHolder) {
                    filter += "<condition attribute='" + customerEntityName + "id" + "' operator='eq' value='{" + accountHolder.alt_customerid.Id + "}' />";
                });
            }
        }
        filter += "</filter>";
        return filter;
    };

    const getEntityStatusCodesByStateCode = function (entityName, stateCode) {

        return new Promise(function (resolve, reject) {

            const webApiQuery =
                Xrm.Utility.getGlobalContext().getClientUrl() +
                "/api/data/v9.2/EntityDefinitions(LogicalName='" + entityName + "')" +
                "/Attributes/Microsoft.Dynamics.CRM.StatusAttributeMetadata?$expand=OptionSet";

            const req = new XMLHttpRequest();
            req.open("GET", webApiQuery, true); // async = true
            req.setRequestHeader("Accept", "application/json");
            req.setRequestHeader("OData-MaxVersion", "4.0");
            req.setRequestHeader("OData-Version", "4.0");

            req.onreadystatechange = function () {
                if (req.readyState !== 4) return;

                if (req.status === 200) {
                    const results = JSON.parse(req.responseText);
                    const data = results.value && results.value.length
                        ? results.value[0]
                        : null;

                    let options = data.OptionSet.Options;
                    let optionsByState = [];

                    options.forEach(function (opt) {
                        if (opt.State == stateCode) {
                            optionsByState.push(opt.Value);
                        }
                    });

                    resolve(optionsByState);
                } else {
                    let errorMsg = "Unknown error";
                    try {
                        const error = JSON.parse(req.responseText).error;
                        errorMsg = error.message;
                    } catch (e) { }

                    reject(new Error(errorMsg));
                }
            };

            req.send();
        });
    }

    const getTreatmentStatusesByRelatedEntityStatusCodes = function (statusCodes, relatedEntityCode, relatedEntityStatusCodeAttribute) {

        return new Promise(function (resolve, reject) {

            let statusCodesConditions = [];

            statusCodes.forEach(function (opt) {
                let condition = relatedEntityStatusCodeAttribute + " eq " + opt;
                statusCodesConditions.push(condition);
            });

            let generalFilter = "(statecode eq " + customEntityStateCode.Active
                + " and alt_userdisplaybit eq true)";

            let statusCodesFilter = "(" + statusCodesConditions.join(" or ") + ")";
            let filter = generalFilter + " and " + statusCodesFilter;

            Utils.Server.RetrieveMultiple("alt_treatmentstatus", "alt_treatmentstatusid", filter, null, null,
                function (retrievedTreatmentStatuses) {
                    resolve(retrievedTreatmentStatuses);
                },
                function (error) {
                    console.log(error);
                    Xrm.Navigation.openAlertDialog({ text: 'לא הייתה אפשרות לבצע את הפעולה, אנא פנה למנהל מערכת.' });
                    reject(null);
                }
            );
        });
    };

    return {
        GetAllTeamsByUserId: getAllTeamsByUserId,
        GetAllUsersByTeamId: getAllUsersByTeamId,
        GetGlobalParamValue: GetGlobalParamValue,
        ExecuteAsyncEnableRoleButton: AsyncEnableRoleButtonManager.executeAsyncButtonOperation,
        InitAsyncButtonOperation: AsyncEnableRoleButtonManager.initAsyncButtonOperation,
        GetAllActiveActivitiesByRegardingObject: getAllActiveActivitiesByRegardingObject,
        RemoveOptionsetValuesByGlobalParams: removeOptionsetValuesByGlobalParams,
        GetSystemUserDefaultTeam: getSystemUserDefaultTeam,
        FilterCustomers: CustomerFilterManager.addCustomerCustomFilter,
        CreateLookupCustomFilter: createLookupCustomFilter,
        AddQueryStringParamsToJoiningFormURL: addQueryStringParamsToJoiningFormURL,
        GenerateLookupObjectPropertyName: generateLookupObjectPropertyName,
        GenerateLookupObjectValue: generateLookupObjectValue,
        GenerateParserCustomEntryPointEntityReference: generateParserCustomEntryPointEntityReference,
        DocumentGrid: DocumentGrid,
        GetContactByGovernmentId: getContactByGovernmentId,
        GetAccountByAccountNumber: getAccountByAccountNumber,
        GetActiveAccountHoldersByRelatedEntity: getActiveAccountHoldersByRelatedEntity,
        CreateAccountHoldersCustomerFilter: createAccountHoldersCustomerFilter,
        GetEntityStatusCodesByStateCode: getEntityStatusCodesByStateCode,
        GetTreatmentStatusesByRelatedEntityStatusCodes: getTreatmentStatusesByRelatedEntityStatusCodes
    };

})(window.Utils.Global = window.Utils.Global || {});
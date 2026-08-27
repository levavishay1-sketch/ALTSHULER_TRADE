
if (typeof (Utils) == "undefined")
    Utils = {};

Utils.Server = (function () {

    const serverUrl = Xrm.Utility.getGlobalContext().getClientUrl();
    const webApiPath = serverUrl + "/api/data/v9.2/";

    const CrmDataTypes = {
        Int: "int",
        String: "string",
        EntityRefernce: "EntityReference",
        Bool: "bool",
        DateTime: "DateTime",
        OptionSet: "OptionSetValue",
        Money: "Money",
        EntityCollection: "EntityCollection"
    };

    const messageLevel = {
        Information: 1,
        Warning: 2,
        Error: 3,
        Critical: 4
    };

    const systemParametersCache = {};

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
                    case Types.EntityCollection: {
                        //EntityCollection parameter
                        data[item.key] = [];
                        if (item && item.value && item.value.length > 0) {
                            item.value.forEach(function (record) {
                                let objectToAdd = {};
                                for (var attribute in record) {
                                    if (attribute === "entityType") {
                                        objectToAdd["@odata.type"] = "Microsoft.Dynamics.CRM." + record.entityType;
                                    }
                                    else {
                                        objectToAdd[attribute] = record[attribute];
                                    }
                                }
                                data[item.key].push(objectToAdd);
                            });
                        }

                        break;
                    }

                    default:

                }
            }
        }


        let reqEntityName = "";
        let reqActionName = "";
        if (!Utils.JsExtantions.String.IsNullOrEmpty(targetEntityName) && !Utils.JsExtantions.String.IsNullOrEmpty(targetId)) {
            reqEntityName = getEntityPluralName(targetEntityName) + "(" + Utils.JsExtantions.String.RemoveBraces(targetId) + ")/";
            reqActionName = "Microsoft.Dynamics.CRM." + actionName;
        }
        else {
            reqActionName = actionName;
        }

        reqUrl = webApiPath.toLowerCase() + reqEntityName.toLowerCase() + reqActionName;

        return executeHttpRequest("POST", reqUrl, successCallBack, errorCallback, data);

    };

    const retrieve = function (entityName, recordId, select, expand, successCallBack, errorCallback) {

        if (!recordId) return null;

        recordId = Utils.JsExtantions.String.RemoveBraces(recordId);

        return generalRetrieve(entityName, recordId, select, null, null, expand, successCallBack, errorCallback);
    };

    const retrieveAndCache = function (entityName, recordId, cacheChainnigNodes, select, successCallBack, errorCallback) {

        if (!cacheChainnigNodes) {
            console.error("No cacheChainnigNodes passed to retrieveAndCache");
            return;
        }

        let receivedData = getValueBychainingKeys(cacheChainnigNodes);
        if (!receivedData) {
            retrieve(entityName, recordId, select, null, function (result) {
                receivedData = result;
                setValueToCacheTreeObject(cacheChainnigNodes, receivedData);

                if (successCallBack) {
                    successCallBack(result);
                }

            }, function (error) {
                receivedData = error;
                if (errorCallback) errorCallback(receivedData);
            });
        }

        else if (successCallBack) {
            console.log('data from cache:', receivedData);
            successCallBack(receivedData);
        } else { console.log('data from cache:', receivedData); }

        return receivedData;
    };

    const retrieveMultiple = function (entityName, select, filter, orderby, expand, successCallBack, errorCallback) {

        return generalRetrieve(entityName, null, select, filter, orderby, expand, successCallBack, errorCallback);
    };

    const retrieveMultipleAndCache = function (entityName, cacheChainnigNodes, select, filter, orderby, expand, successCallBack, errorCallback) {

        if (!cacheChainnigNodes) {
            console.error("No cacheChainnigNodes passed to retrieveMultipleAndCache");
            return;
        }

        let receivedData = getValueBychainingKeys(cacheChainnigNodes);
        if (!receivedData) {
            retrieveMultiple(entityName, select, filter, orderby, expand, function (result) {
                receivedData = result;
                setValueToCacheTreeObject(cacheChainnigNodes, receivedData);

                if (successCallBack) {
                    successCallBack(result);
                }

            }, function (error) {
                receivedData = error;
                if (errorCallback) errorCallback(receivedData);
            });
        }
        else if (successCallBack) {
            console.log('data from cache:', receivedData);
            successCallBack(receivedData);
        } else { console.log('data from cache:', receivedData); }

        return receivedData;
    };

    const generalRetrieve = function (entityName, recordId, select, filter, orderby, expand, successCallBack, errorCallback) {

        let entityNameString = getEntityPluralName(entityName);
        entityNameString += !Utils.JsExtantions.String.IsNullOrEmpty(recordId) ? "(" + recordId + ")" : "";

        let query = "";

        query += !Utils.JsExtantions.String.IsNullOrEmpty(select) ? "$select=" + select : "";

        query += !Utils.JsExtantions.String.IsNullOrEmpty(expand)
            && !Utils.JsExtantions.String.IsNullOrEmpty(query)
            ? "&$expand=" + expand
            : !Utils.JsExtantions.String.IsNullOrEmpty(expand)
                ? "$expand=" + expand
                : "";

        query += !Utils.JsExtantions.String.IsNullOrEmpty(filter)
            && !Utils.JsExtantions.String.IsNullOrEmpty(query)
            ? "&$filter=" + filter
            : !Utils.JsExtantions.String.IsNullOrEmpty(filter)
                ? "$filter=" + filter
                : "";

        query += !Utils.JsExtantions.String.IsNullOrEmpty(orderby)
            && !Utils.JsExtantions.String.IsNullOrEmpty(query)
            ? "&$orderby=" + orderby
            : !Utils.JsExtantions.String.IsNullOrEmpty(orderby)
                ? "$orderby=" + orderby
                : "";

        query = !Utils.JsExtantions.String.IsNullOrEmpty(query)
            ? "?" + query
            : "";



        const reqUrl = webApiPath.toLowerCase() + entityNameString.toLowerCase() + query;

        return executeHttpRequest("GET", reqUrl, successCallBack, errorCallback);
    };

    const fetch = function (entityName, fetchXmlStr, successCallBack, errorCallback) {

        const reqUrl = webApiPath + getEntityPluralName(entityName) + "?fetchXml=" + fetchXmlStr;

        return executeHttpRequest("GET", reqUrl, successCallBack, errorCallback);
    };

    const update = function (entityName, recordId, dataObj, successCallBack, errorCallback) {

        const reqUrl = webApiPath + getEntityPluralName(entityName) + "(" + Utils.JsExtantions.String.RemoveBraces(recordId) + ")";

        executeHttpRequest("PATCH", reqUrl, successCallBack, errorCallback, dataObj);
    };

    const updateRecord = function (entityLogicalName, id, data, successCallback, errorCallback) {

        Xrm.WebApi.updateRecord(entityLogicalName, id, data).then(successCallback, errorCallback);

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
                    receivedData = parseResponceData(receivedData);
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

    const getEntityPluralName = function (entityName) {

        return Utils.JsExtantions.Entity.GetEntityPluralName(entityName);
    };

    const parseResponceData = function (responce) {

        let ret;

        if (responce.length) {
            ret = new Array();

            for (let i = 0; i < responce.length; i++) {
                ret.push(parseWebApiJsonObject(responce[i]));
            }
        } else if (responce.length === 0) {
            ret = null;
        } else {
            ret = parseWebApiJsonObject(responce);
        }

        return ret;

    };

    const parseWebApiJsonObject = function (dataObj) {

        const ret = new Object();
        const props = Object.keys(dataObj);

        for (let i = 0; i < props.length; i++) {

            const index = props[i].indexOf('@');

            if (index !== -1) {

                const propNamePrefix = props[i].substring(0, index);
                const propNameSuffix = (props[i].substring(index, props[i].length));
                const suffixesArr = propNameSuffix.split('.');
                const fieldName = propNamePrefix.startsWith('_') && propNamePrefix.endsWith('value') ? propNamePrefix.replace('_', '').replace('_value', '') : propNamePrefix + "_" + suffixesArr[suffixesArr.length - 1];

                switch (propNameSuffix) {

                    case "@OData.Community.Display.V1.FormattedValue": {

                        ret[fieldName] = ret[fieldName] ? ret[fieldName] : {};
                        ret[fieldName].Name = dataObj[props[i]];

                        break;
                    }
                    case "@Microsoft.Dynamics.CRM.lookuplogicalname": {
                        ret[fieldName] = ret[fieldName] ? ret[fieldName] : {};
                        ret[fieldName].LogicalName = dataObj[props[i]];

                        break;
                    }
                    default: {
                        const propName = propNamePrefix + "_" + suffixesArr[suffixesArr.length - 1];
                        ret[propName] = dataObj[props[i]];
                        break;
                    }
                }
            }
            else if (props[i].startsWith('_')) {
                const objName = props[i].replace('_', '').replace('_value', '');
                ret[objName] = ret[objName] ? ret[objName] : {};
                ret[objName].Id = dataObj[props[i]];
            }
            else {

                if (typeof (dataObj[props[i]]) === 'object' && dataObj[props[i]] && Object.keys(dataObj[props[i]]).length >= 1) { //related entity
                    ret[props[i]] = ret[props[i]] ? ret[props[i]] : {};
                    ret[props[i]].Expand = parseWebApiJsonObject(dataObj[props[i]]);
                }
                else {
                    ret[props[i]] = dataObj[props[i]];
                }
            }
        }

        return ret;
    };

    // cache logic
    const localCacheObject = {};
    const storageExpirationTimeInHours = 5;
    const uniqueName = Xrm.Utility.getGlobalContext().organizationSettings.uniqueName;

    const setValueToCacheTreeObject = function (chainingKeys, value) {
        lazyCacheStorageObjectLoad();
        localCacheObject[uniqueName]['data'] = localCacheObject[uniqueName]['data'] || {};
        let storageData = localCacheObject[uniqueName]['data'];
        while (chainingKeys) {
            if (!chainingKeys.innerKey) {//breaking condition
                storageData[chainingKeys.key] = value;
                break;
            }

            // run through the existing nodes in the cache object tree
            if (storageData[chainingKeys.key]) {
                storageData = storageData[chainingKeys.key];
                chainingKeys = chainingKeys.innerKey;
            }
            else {// the rest of the sub tree
                storageData[chainingKeys.key] = new Object();
                storageData = storageData[chainingKeys.key];
                chainingKeys = chainingKeys.innerKey;
            }
        }
        setCacheStorageObject(localCacheObject[uniqueName]);
    };

    const getValueBychainingKeys = function (chainingKeys) {
        lazyCacheStorageObjectLoad();
        if (!isCacheStorageExpired()) {
            localCacheObject[uniqueName]['data'] = localCacheObject[uniqueName]['data'] || {};
            let storageData = localCacheObject[uniqueName]['data'];
            while (chainingKeys) {
                //breaking condition
                if (!chainingKeys.innerKey) {
                    return storageData[chainingKeys.key];
                }
                // run through the existing nodes in the cache object tree
                if (storageData[chainingKeys.key]) {
                    storageData = storageData[chainingKeys.key];
                    chainingKeys = chainingKeys.innerKey;
                }
                else { // not found
                    return null;
                }
            }
        } else {
            initCacheStorageObjectTree();
            return null;
        }
    };

    const isCacheStorageExpired = function () {
        const now = new Date();
        const parsedStorageValueObject = localCacheObject[uniqueName];
        let expirationDate = parsedStorageValueObject["Expiration-Date"] ? new Date(parsedStorageValueObject["Expiration-Date"]) : null;
        return (!expirationDate || expirationDate < now);
    };

    const initCacheStorageObjectTree = function () {
        let nextExpirationDate = new Date();
        nextExpirationDate.setHours(nextExpirationDate.getHours() + storageExpirationTimeInHours);
        setCacheStorageObject({ 'Expiration-Date': nextExpirationDate, 'data': {} });
    };

    //lazy loading
    const lazyCacheStorageObjectLoad = function () {
        if (!localCacheObject[uniqueName]) {
            const clientType = Xrm.Utility.getGlobalContext().client.getClient();
            localCacheObject[uniqueName] = clientType === 'Web' ? JSON.parse(sessionStorage.getItem(uniqueName) || '{}') : {};
        }
    };

    const setCacheStorageObject = function (value) {
        localCacheObject[uniqueName] = value;
        const clientType = Xrm.Utility.getGlobalContext().client.getClient();
        if (clientType === 'Web') {
            sessionStorage.setItem(uniqueName, JSON.stringify(value));
        }
    };

    const messageBuilder = function (message) {

        let messageToLog = [];
        let stack = [];
        let clientUrl = Xrm.Utility.getGlobalContext().getCurrentAppUrl() + getRecordUrl();
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

        let messageBlock = messageBuilder(message);
        let caller = messageBlock[5].split('(');
        let name = getEntityName() + ' ⇒ ' + caller[0];
        let logMessage = [
            { 'key': 'MessageBlock', 'value': messageBlock.join('\n\n'), 'type': CrmDataTypes.String },
            { 'key': 'MessageLevelCode', 'value': messageLevel, 'type': CrmDataTypes.Int },
            { 'key': 'EntryPointTypeCode', 'value': 4, 'type': CrmDataTypes.Int },
            { 'key': 'OverrideCreatedOn', 'value': new Date(), 'type': CrmDataTypes.DateTime },
            { 'key': 'Name', 'value': name, 'type': CrmDataTypes.String },
            { 'key': 'ExecutingSystemUserId', 'value': Xrm.Utility.getGlobalContext().userSettings.userId, 'type': CrmDataTypes.String },
            { 'key': 'CorrelationId', 'value': '', 'type': CrmDataTypes.String },
            { 'key': 'Depth', 'value': 1, 'type': CrmDataTypes.Int },
            { 'key': 'PerformanceExecutionDuration', 'value': 1, 'type': CrmDataTypes.Int },
            { 'key': 'OperationDuration', 'value': 1, 'type': CrmDataTypes.Int },
            { 'key': 'RequestId', 'value': '', 'type': CrmDataTypes.String },
            { 'key': 'TargetLogicalName', 'value': getEntityName(), 'type': CrmDataTypes.String },
            { 'key': 'TargetId', 'value': getEntityId(), 'type': CrmDataTypes.String }
        ];
        // Call action
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

    const getEntityName = function () {

        return isUCI() ? Xrm.Utility.getPageContext().input.entityName
            : Xrm.Page.data.entity.getEntityName();
    };

    const getEntityId = function () {

        return isUCI() ? Xrm.Utility.getPageContext().input.entityId
            : Xrm.Page.data.entity.getId();
    };

    const getRecordUrl = function () {
        const entityName = getEntityName();
        const recordId = getEntityId();
        return isUCI() ? "&pagetype=entityrecord&etn=" + entityName + "&id=" + recordId
            : "/main.aspx?etn=" + entityName + "&id=" + recordId + "&pagetype=entityrecord";
    };

    const isUCI = function () {
        let globalContext = Xrm.Utility.getGlobalContext();
        let currentAppUrl = globalContext.getCurrentAppUrl().toLowerCase();
        return currentAppUrl.indexOf("appid") === -1 ? false : true;
    };

    const exportSSRSReportAsPdf = function (reportName, reportId, fetchParameterXML, successCallBack, errorCallback) {
        var url = Xrm.Page.context.getClientUrl() + "/CRMReports/rsviewer/ReportViewer.aspx";
        var request = new XMLHttpRequest();
        const isAsyncReq = true;
        request.open("POST", url, isAsyncReq);
        request.setRequestHeader("Accept", "*/*");
        request.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        request.onreadystatechange = function () {
            if (request.readyState === 4) {
                if (request.status === 200) {

                    var reportSession = request.responseText.lastIndexOf("ReportSession=");
                    var seessionConfigurationObject = new Array();
                    seessionConfigurationObject[0] = request.responseText.substr(reportSession + 14, 24);
                    seessionConfigurationObject[1] = request.responseText.substr(reportSession + 10, 32);
                    var newPth = Xrm.Page.context.getClientUrl() + "/Reserved.ReportViewerWebControl.axd?ReportSession=" + seessionConfigurationObject[0] + "&Culture=1033&CultureOverrides=True&UICulture=1033&UICultureOverrides=True&ReportStack=1&ControlID=" + seessionConfigurationObject[1] + "&OpType=Export&FileName=public&ContentDisposition=OnlyHtmlInline&Format=PDF";

                    if (successCallBack) {
                        convertResponseToPDF(newPth, successCallBack, errorCallback);
                    }
                }
                else if (request.status === 204) {

                    receivedData = true;
                    if (successCallBack) successCallBack(receivedData);
                }
                else {
                    receivedData = JSON.parse(request.responseText).error;
                    writeHTTPRequestErrorLog("POST", url, receivedData);
                    console.error(receivedData.message);
                    if (errorCallback) errorCallback(receivedData);
                }
            }
        };
        const urlToSend = "id=%7B" + reportId + "%7D&uniquename=" + Xrm.Page.context.getOrgUniqueName() + "&iscustomreport=true&reportnameonsrs=&reportName=" + reportName + "&isScheduledReport=false&" + fetchParameterXML;
        request.send(urlToSend);
    };

    const convertResponseToPDF = function (newPth, successCallBack, errorCallback) {
        var retrieveEntityReq = new XMLHttpRequest();
        retrieveEntityReq.open("GET", newPth, true);
        retrieveEntityReq.setRequestHeader("Accept", "*/*");
        retrieveEntityReq.responseType = "arraybuffer";
        retrieveEntityReq.onreadystatechange = function () {
            if (retrieveEntityReq.readyState === 4) {
                if (retrieveEntityReq.status === 200) {

                    var binary = "";
                    var bytes = new Uint8Array(this.response);
                    for (var i = 0; i < bytes.byteLength; i++) {
                        binary += String.fromCharCode(bytes[i]);
                    }
                    var base64PDFString = btoa(binary);
                    var entityId = Xrm.Page.data.entity.getId();
                    entityId = entityId.replace("{", "").replace("}", "");
                    successCallBack(base64PDFString);

                }
                else if (retrieveEntityReq.status === 204) {

                    receivedData = true;
                    if (successCallBack) successCallBack(receivedData);
                }
                else {
                    receivedData = JSON.parse(retrieveEntityReq.responseText).error;
                    writeHTTPRequestErrorLog("GET", newPth, receivedData);
                    console.error(receivedData.message);
                    if (errorCallback) errorCallback(receivedData);
                }
            }
        };
        retrieveEntityReq.send();
    };

    const createAppNotification = function (title, body, userId, iconType, data, successCallback) {
        var notificationRecord =
        {
            "title": title,
            "body": body,
            "ownerid@odata.bind": "/systemusers(" + userId + ")",
            "icontype": iconType,
            "data": JSON.stringify({
                "body": data
            })
        };
        Xrm.WebApi.createRecord("appnotification", notificationRecord).
            then(
                function success(result) {
                    if (successCallback) {
                        successCallback(result);
                    }
                },
                function (error) {
                    console.log(error.message);
                }
            );
    };

    const retrieveDuplicates = function (record, matchingEntityName, successCallback) {

        var pagingInfo = {
            "PageNumber": 1,
            "Count": 100
        };

        var globalContext = Xrm.Utility.getGlobalContext();

        var requestUrl = "/api/data/v9.1/RetrieveDuplicates(BusinessEntity=@p1,MatchingEntityName=@p2,PagingInfo=@p3)";
        requestUrl += "?@p1=" + encodeURIComponent(JSON.stringify(record));
        requestUrl += "&@p2='" + matchingEntityName + "'";
        requestUrl += "&@p3=" + encodeURIComponent(JSON.stringify(pagingInfo));

        var req = new XMLHttpRequest();
        req.open("GET", globalContext.getClientUrl() + requestUrl, true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;

                if (this.status === 200) {
                    var results = JSON.parse(this.response);
                    console.log(results);
                    if (successCallback) {
                        successCallback(results.value, matchingEntityName);
                    }

                } else {
                    var errorText = this.responseText;
                    console.log(errorText);
                }
            }
        };
        req.send();
    };

    const getObjectTypeCodeByEntityName = function (entityName, successCallback, errorCallback) {
        let request = "EntityDefinitions?$select=LogicalName,ObjectTypeCode&$filter=LogicalName eq '" + entityName + "'";
        let requestUrl = webApiPath + request;
        executeHttpRequest("GET", requestUrl, successCallback, errorCallback);
    }

    return {
        CrmDataTypes: CrmDataTypes,
        CallAction: callAction,
        Retrieve: retrieve,
        RetrieveAndCache: retrieveAndCache,

        RetrieveMultiple: retrieveMultiple,
        RetrieveMultipleAndCache: retrieveMultipleAndCache,
        Fetch: fetch,
        Update: update,
        WriteLog: writeLog,
        MessageLevel: messageLevel,
        ExportSSRSReportAsPdf: exportSSRSReportAsPdf,
        CreateAppNotification: createAppNotification,
        RetrieveDuplicates: retrieveDuplicates,
        GetObjectTypeCodeByEntityName: getObjectTypeCodeByEntityName,
        UpdateRecord: updateRecord
    };

})(window.Utils.Server = window.Utils.Server || {});

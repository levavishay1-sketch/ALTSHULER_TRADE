/// <reference path="../utils/Utils.Global.js" />
/// <reference path="../utils/Utils.Enums.js" />

var DocumentSearchCommonBL = (function () {
    const searchFileFromArchiveTimeout = 2 * 60;
    const documentSearchGlobalConfigName = "DocumentSearchInArchiveWaitingThreshold";
    const documentSearchEntityLogicalName = 'alt_archivedocumentsearch';
    let searchWaitingThreshold = 60;

    const archiveNotificationMessages = {
        SearchingFromArchiveMessage: {
            Message: "מתבצעת טעינת מסמכים מארכיון...",
            Id: "1"
        },
        SearchTimeoutMessage: {
            Message: "חריגה בזמן בטעינת מסמכים מארכיון",
            Id: "2"
        },
        SearchErrorMessage: {
            Message: "שגיאה בטעינת מסמכים מארכיון",
            Id: "3"
        },
        SearchCompleteMessage: {
            Message: "טעינת מסמכים מארכיון הושלמה",
            Id: "4"
        }
    };

    const fieldsToLock = [
        "alt_name",
        "alt_customerid",
        "alt_producttypecode"
    ];

    var lockFieldsInDocumentsGrid = function (executionContext) {
        let oFormContext = executionContext.getFormContext();
        if (oFormContext) {
            let objEntity = oFormContext.data.entity;
            objEntity.attributes.forEach(function (attribute, i) {
                if (fieldsToLock.indexOf(attribute.getName()) > -1) {
                    let attributeToDisable = attribute.controls.get(0);
                    attributeToDisable.setDisabled(true);
                }
            });
        }
    };

    var LoadDocumentGrid = function (entityName, entityIdWithoutBraces, onLoadExecutionContext) {
        let executionContext = onLoadExecutionContext.getFormContext();
        createOrRetrieveDocumentSearchEntity(entityIdWithoutBraces, entityName, executionContext);
    }

    const createOrRetrieveDocumentSearchEntity = function (entityID, entityLogicalName, executionContext) {
        Utils.Global.GetGlobalParamValue(documentSearchGlobalConfigName, function (result) {
            if (result) {
                searchWaitingThreshold = parseInt(result);
            }
        });

        const query = `?$select=alt_lastsearchdate, alt_searchfromarchivestatuscode&$filter=_regardingobjectid_value eq ${entityID}`;
        Xrm.WebApi.retrieveMultipleRecords(documentSearchEntityLogicalName, query).then(
            function success(result) {
                if (result.entities.length > 0) {
                    if (checkIfSearchShouldBePerformed(result)) {
                        callSearchFilesCustomAction(result.entities[0].activityid, documentSearchEntityLogicalName, executionContext);
                    }
                }
                else {
                    createDocumentSearchEntity(entityID, entityLogicalName)
                }
            }
        )
    };

    const callSearchFilesCustomAction = function (entityID, entityLogicalName, executionContext) {
        var documentSearchData = {
            EntityID: entityID,
            EntityLogicalName: entityLogicalName
        };

        var actionInput = [{
            key: 'Data',
            value: JSON.stringify(documentSearchData),
            type: Utils.Server.CrmDataTypes.String
        }];

        executionContext.ui.setFormNotification(
            archiveNotificationMessages.SearchingFromArchiveMessage.Message,
            notificationLevel.Warning,
            archiveNotificationMessages.SearchingFromArchiveMessage.Id);

        Utils.Server.CallAction('alt_SearchFiles', entityLogicalName, null, actionInput,
            function (res) {
                let timer = 0;
                let id = setInterval(check, 500)
                function check() {
                    if (timer >= searchFileFromArchiveTimeout) {
                        let updateObj = { "alt_searchfromarchivestatuscode": 5, "alt_lastsearchdate": new Date() };
                        Utils.Server.UpdateRecord(documentSearchEntityLogicalName, entityID, updateObj, null,
                            function (error) {
                                Utils.Server.WriteLog(error.message, Utils.Server.MessageLevel.Error);
                            }
                        );
                        executionContext.ui.clearFormNotification(archiveNotificationMessages.SearchingFromArchiveMessage.Id);
                        executionContext.ui.setFormNotification(
                            archiveNotificationMessages.SearchTimeoutMessage.Message,
                            notificationLevel.Warning,
                            archiveNotificationMessages.SearchTimeoutMessage.Id);
                        clearInterval(id);
                    }
                    else {
                        Xrm.WebApi.retrieveRecord(entityLogicalName, entityID, "?$select=alt_searchfromarchivestatuscode").then(
                            function success(result) {
                                if (result && result.alt_searchfromarchivestatuscode === 4) {
                                    executionContext.ui.clearFormNotification(archiveNotificationMessages.SearchingFromArchiveMessage.Id);
                                    executionContext.ui.setFormNotification(
                                        archiveNotificationMessages.SearchCompleteMessage.Message,
                                        notificationLevel.Warning,
                                        archiveNotificationMessages.SearchCompleteMessage.Id);
                                    clearInterval(id);
                                    Xrm.Page.data.refresh();
                                }
                                else if (result && result.alt_searchfromarchivestatuscode === 5) {
                                    executionContext.ui.clearFormNotification(archiveNotificationMessages.SearchingFromArchiveMessage.Id);
                                    executionContext.ui.setFormNotification(
                                        archiveNotificationMessages.SearchErrorMessage.Message,
                                        notificationLevel.Warning,
                                        archiveNotificationMessages.SearchErrorMessage.Id);
                                    clearInterval(id);
                                    Xrm.Page.data.refresh();
                                }
                            });
                        timer += 0.5;
                    }
                }
            }, function (error) {
                let updateObj = { "alt_searchfromarchivestatuscode": 5, "alt_lastsearchdate": new Date() };
                Utils.Server.Update(documentSearchEntityLogicalName, entityID, updateObj, null, null);
                executionContext.ui.clearFormNotification(archiveNotificationMessages.SearchingFromArchiveMessage.Id);
                executionContext.ui.setFormNotification(
                    archiveNotificationMessages.SearchErrorMessage.Message,
                    notificationLevel.Warning,
                    archiveNotificationMessages.SearchErrorMessage.Id);
                clearInterval(id);
            });
    };

    const createDocumentSearchEntity = function (entityId, entityName) {
        let data = {
            "subject": `חיפוש מסמכים מארכיון עבור ישות ${entityName}, מזהה ${entityId}`
        }
        data[`regardingobjectid_${entityName}@odata.bind`] = `/${entityName}s(${entityId})`;

        Xrm.WebApi.createRecord(documentSearchEntityLogicalName, data).then(
            function success(result) {
                console.log("new search created - " + result.id);
                callSearchFilesCustomAction(result.id, result.entityType);
            },
            function (error) {
                console.log(error.message);
            }
        );
    };

    const checkIfSearchShouldBePerformed = function (data) {
        const retrievedTime = new Date(data.entities[0].alt_lastsearchdate);
        const currentTime = new Date();
        const timeMargin = ((currentTime - retrievedTime) / 1000) / 60;
        return data.entities[0].alt_searchfromarchivestatuscode === 5 || timeMargin > searchWaitingThreshold;
    };

    return {
        LoadDocumentGrid: LoadDocumentGrid,
        LockFieldsInDocumentsGrid: lockFieldsInDocumentsGrid
    }
})();
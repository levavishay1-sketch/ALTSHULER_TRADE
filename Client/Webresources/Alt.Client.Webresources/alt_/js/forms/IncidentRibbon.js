var IncidentRibbon = (function () {
    // "use strict";

    let formContext;
    var ALERT_MESSAGE;
    const CANCEL_INCIDENT_CONFIRM_TITLE_MESSAGE = 'שים לב - האירוע יבוטל';
    const CANCEL_INCIDENTS_CONFIRM_SUBTITLE_MESSAGE = 'פעולה זו תביא לביטול של כל האירועים הצאצאים תחת אירוע זה, פעולה זו הינה בלתי הפיכה. האם ברצונך להמשיך?';
    const CANCEL_INCIDENT_CONFIRM_SUBTITLE_MESSAGE = 'האם ברצונך להמשיך?';

    let incidentsToCreateCount;
    let createdIncidentIds = [];


    const incidentsBulkCreateOnClick = function (primaryControl) {

        formContext = primaryControl ? primaryControl : null;
        createdIncidentIds = [];
        ALERT_MESSAGE = 'הפעולה הסתיימה.';
        formContext.data.save().then(function () {
            const select = 'alt_subject1id';
            let filter = "alt_bulkincidentcreatedisplaybit eq true";
            Utils.Server.RetrieveMultiple("alt_subject1", select, filter, null, null, function (result) {
                if (result) {
                    var lookupOptions = {};
                    lookupOptions.allowMultiSelect = true;
                    lookupOptions.disableMru = true;
                    lookupOptions.defaultEntityType = "alt_subject1";
                    lookupOptions.entityTypes = ["alt_subject1"];
                    let filterConditions = [];
                    result.forEach(function (subject) {
                        filterConditions.push('<condition attribute="alt_subject1id" operator="eq" uitype="alt_subject1" value="{' + subject.alt_subject1id + '}"/>');
                    });

                    lookupOptions.filters = [{
                        filterXml: '<filter type="or">' + filterConditions.join('') + ' </filter>',
                        entityLogicalName: "alt_subject1"
                    }];
                    Xrm.Utility.lookupObjects(lookupOptions).
                        then(function (selectedTemplates) {
                            if (selectedTemplates !== null && selectedTemplates.length > 0) {
                                Xrm.Utility.showProgressIndicator("מעבד...");
                                getTradeAutomaticIncidentTemplates(selectedTemplates, showTradeIncidentTemplates, createIncidentSuccessCallback);
                            }
                            else {
                                Xrm.Navigation.openAlertDialog({ text: 'לא נבחרה אף קטגורית אירוע.' });
                            }
                        }, function (e) {
                            console.log(e.error.message);
                        });
                }
                else {
                    Xrm.Navigation.openAlertDialog({ text: "לא קיימים נושאים ליצירה אוטומטי לפי סוג שנבחר." });
                    Xrm.Utility.closeProgressIndicator();
                }
            }, function (error) {
                Xrm.Navigation.openAlertDialog({ text: 'לא הייתה אפשרות לבצע את הפעולה, אנא פנה למנהל מערכת.' });
                Xrm.Utility.closeProgressIndicator();
                console.log(error);
            });       
        }, function (err) {
            console.error(err);
        });
    };

    const changeIncidentStatusOnClick = function (primaryControl) {
        formContext = primaryControl ? primaryControl : Xrm.Page;

        var formParameters = null;

        openIncidentStatusLogQuickCreateForm(formParameters);
    };

    const cancelIncidentOnClick = function (primaryControl) {
        formContext = primaryControl ? primaryControl : Xrm.Page;
        const subtitle = formContext.getAttribute('alt_bulkincidentsparentbit').getValue() == true ?
            CANCEL_INCIDENTS_CONFIRM_SUBTITLE_MESSAGE : CANCEL_INCIDENT_CONFIRM_SUBTITLE_MESSAGE;
        const confirmStrings = {
            title: CANCEL_INCIDENT_CONFIRM_TITLE_MESSAGE,
            subtitle: subtitle
        };

        Xrm.Navigation.openConfirmDialog(confirmStrings).then(
            function (success) {
                if (success.confirmed) {
                    Xrm.Utility.showProgressIndicator('מבטל אירוע...');

                    Utils.Global.GetGlobalParamValue('TradeIncidentStatusCancelCode', function (systemParam) {
                        if (systemParam) {
                            const userId = Xrm.Utility.getGlobalContext().userSettings.userId;

                            Utils.Global.GetSystemUserDefaultTeam(formContext, userId, function (result) {
                                const team = result.alt_defaultteamid;
                                if (team.Id) {
                                    const crmDataTypes = Utils.Server.CrmDataTypes;
                                    const changeStatusData = [
                                        {
                                            key: 'ToIncidentStatusCode',
                                            value: parseInt(systemParam),
                                            type: crmDataTypes.Int
                                        },
                                        {
                                            key: 'FromTeamId',
                                            value: team.Id,
                                            type: crmDataTypes.String
                                        },
                                        {
                                            key: 'ToTeamId',
                                            value: team.Id,
                                            type: crmDataTypes.String
                                        }];

                                    const incidentId = Utils.JsExtantions.String.RemoveBraces(formContext.data.entity.getId());
                                    Utils.Server.CallAction('alt_IncidentStatusChange', 'incident', incidentId, changeStatusData, function (res) {
                                        formContext.data.refresh(false).then(function () {
                                            Xrm.Utility.closeProgressIndicator();
                                        });
                                    }, function () {
                                        Xrm.Utility.closeProgressIndicator();
                                        Xrm.Navigation.openAlertDialog({ text: Utils.CrmPage.CommonRequestFailedMessage });
                                    });
                                } else {
                                    Xrm.Utility.closeProgressIndicator();
                                    Xrm.Navigation.openAlertDialog({ text: Utils.CrmPage.CommonRequestFailedMessage });
                                }
                            });
                        } else {
                            Xrm.Utility.closeProgressIndicator();
                            Xrm.Navigation.openAlertDialog({ text: Utils.CrmPage.CommonRequestFailedMessage });
                        }
                    }, null);
                }
            });
    };

    const openIncidentStatusLogQuickCreateForm = function (formParameters) {

        formContext.data.save().then(function () {

            const entityFormOptions = {};
            entityFormOptions["entityName"] = "alt_incidentstatuslog";
            entityFormOptions["createFromEntity"] = {
                id: formContext.data.entity.getId(),
                name: formContext.getAttribute('title').getValue(),
                entityType: "incident"
            };
            entityFormOptions["useQuickCreateForm"] = true;

            // Open the form.
            Xrm.Navigation.openForm(entityFormOptions, formParameters).then(
                function (success) {
                    formContext.data.refresh(false);
                },
                function (error) {
                    console.log(error);
                });
        }, function (err) {
            console.error(err);
        });
    };

    var showIncidentTemplateOptions = function (result) {
        var lookupOptions = {};
        lookupOptions.allowMultiSelect = true;
        lookupOptions.disableMru = true;
        lookupOptions.defaultEntityType = "alt_automaticincidenttemplate";
        lookupOptions.entityTypes = ["alt_automaticincidenttemplate"];
        let filterConditions = [];
        result.forEach(function (incidentTemplate) {
            filterConditions.push('<condition attribute="alt_automaticincidenttemplateid" operator="eq" uitype="alt_automaticincidenttemplate" value="{' + incidentTemplate.alt_automaticincidenttemplateid + '}"/>');
        });

        lookupOptions.filters = [{
            filterXml: '<filter type="or">' + filterConditions.join('') + ' </filter>',
            entityLogicalName: "alt_automaticincidenttemplate"
        }];

        return lookupOptions;
    };

    var createIncident = function (data, name, successCallback) {
        Xrm.Utility.closeProgressIndicator();
        var CREATING_INCIDENT_ALERT_MESSAGE = "מתבצעת יצירת אירוע ";
        Xrm.Utility.showProgressIndicator(CREATING_INCIDENT_ALERT_MESSAGE + name);

        Xrm.WebApi.createRecord("incident", data).then(
            function success(result) {
                incidentsToCreateCount = incidentsToCreateCount - 1;
                successCallback(result.id, name);
            },
            function (error) {
                incidentsToCreateCount = incidentsToCreateCount - 1;
                ALERT_MESSAGE += '\n';
                ALERT_MESSAGE += 'לא הייתה אפשרות ליצור אירוע ';
                ALERT_MESSAGE += name;
                ALERT_MESSAGE += '\n';
                ALERT_MESSAGE += 'אנא פנה למנהל מערכת';
                if (incidentsToCreateCount == 0 && createdIncidentIds.length == 0) {
                    Xrm.Utility.closeProgressIndicator();
                    Xrm.Navigation.openAlertDialog({ text: ALERT_MESSAGE });
                }
                console.log(error.message);
                Utils.Server.WriteLog(error.message, Utils.Server.MessageLevel.Critical);
            }
        );
    };

    var createIncidentSuccessCallback = function (createdIncidentId, name) {
        createdIncidentIds.push(createdIncidentId);
        ALERT_MESSAGE += '\n';
        ALERT_MESSAGE += 'נוצר אירוע ';
        ALERT_MESSAGE += name;

        if (incidentsToCreateCount == 0) {
            Xrm.Utility.closeProgressIndicator();
            Xrm.Navigation.openAlertDialog({ text: ALERT_MESSAGE }).then(function () {
                var gridContext = formContext.getControl('GeneralTabChildIncidentsGrid');
                gridContext.refresh();
                if (createdIncidentIds.length == 1) {
                    openIncident(createdIncidentId);
                }
            });
        }
    };

    var generateLookupObjectPropertyName = function (entityName, complexEntityType) {

        return complexEntityType ? complexEntityType + entityName + "@odata.bind" : entityName + "@odata.bind";
    };

    var generateLookupObjectValue = function (entityName, entityId) {

        let id = Utils.JsExtantions.String.RemoveBraces(entityId);
        return "/" + Utils.JsExtantions.Entity.GetEntityPluralName(entityName) + "(" + id + ")";
    };

    var openIncident = function (incidentId) {

        var pageInput = {
            pageType: "entityrecord",
            entityName: "incident",
            entityId: incidentId
        };
        var navigationOptions = {
            target: 2,
            height: { value: 850, unit: "%" },
            width: { value: 850, unit: "%" },
            position: 1
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions);
    };

    var getTradeAutomaticIncidentTemplates = function (selectedTemplates, successCallback, createIncidentSuccessCallback) {

        let id;
        let filter = '';
        selectedTemplates.forEach(function (template, index) {

            id = Utils.JsExtantions.String.RemoveBraces(template.id).toLowerCase();
            if (index == 0) {
                filter += "alt_Subject1Id/alt_subject1id eq " + id;
            }
            else {
                filter += " or alt_Subject1Id/alt_subject1id eq " + id;
            }
        });

        const select = 'alt_key';

        Utils.Server.RetrieveMultiple("alt_automaticincidenttemplate", select, filter, null, null, function (result) {
            Xrm.Utility.closeProgressIndicator();
            if (result) {

                successCallback(result, createIncidentSuccessCallback);
            }
            else {
                Xrm.Navigation.openAlertDialog({ text: "איורע שגיאה. אנא פנה למנהל מערכת." });
            }
        }, function (error) {
            Xrm.Utility.closeProgressIndicator();
            Xrm.Navigation.openAlertDialog({ text: 'לא הייתה אפשרות לבצע את הפעולה, אנא פנה למנהל מערכת' });
            console.log(error);
        });
    };

    var showTradeIncidentTemplates = function (result, createIncidentSuccessCallback) {

        var lookupOptions = showIncidentTemplateOptions(result);
        Xrm.Utility.lookupObjects(lookupOptions).
            then(function (selectedTemplates) {
                if (selectedTemplates !== null && selectedTemplates.length > 0) {
                    Xrm.Utility.showProgressIndicator("מעבד...");
                    handleTradeIncidentsCreate(selectedTemplates, createIncidentSuccessCallback);
                }
                else {
                    Xrm.Navigation.openAlertDialog({ text: 'לא נבחר אף תבנית ליצירת אירוע.' });
                }
            }, function (e) {
                console.log(e.error.message);
            });
    };

    var handleTradeIncidentsCreate = function (selectedTemplates, createIncidentSuccessCallback) {
        incidentsToCreateCount = selectedTemplates.length;
        selectedTemplates.forEach(function (selectedTemplate) {
            const select = 'alt_key';
            let filter = "alt_automaticincidenttemplateid eq " + Utils.JsExtantions.String.RemoveBraces(selectedTemplate.id).toLowerCase();

            Utils.Server.RetrieveMultiple("alt_automaticincidenttemplate", select, filter, null, null, function (data) {
                if (data) {

                    createTradeIncidentByIncidentTemplateOnSelect(data[0].alt_key, selectedTemplate.name, createIncidentSuccessCallback);
                }
                else {
                    Xrm.Navigation.openAlertDialog({ text: "לא קיימים נושאים ליצירה אוטומטי לפי סוג שנבחר." });
                    Xrm.Utility.closeProgressIndicator();
                }
            }, function (error) {
                Xrm.Navigation.openAlertDialog({ text: 'לא הייתה אפשרות לבצע את הפעולה, אנא פנה למנהל מערכת.' });
                Xrm.Utility.closeProgressIndicator();
                console.log(error);
            });
        });
    };

    var createTradeIncidentByIncidentTemplateOnSelect = function (automaticIncidentTemplateKey, name, successCallback) {

        let incidentToCreate = {};
        incidentToCreate["alt_automaticincidenttemplatekey"] = automaticIncidentTemplateKey;
        incidentToCreate['caseorigincode'] = formContext.getAttribute('caseorigincode').getValue();        

        const customerid = formContext.getAttribute("customerid").getValue()
            && formContext.getAttribute("customerid").getValue()[0];
        if (customerid) {
            const customeridId = Utils.JsExtantions.String.RemoveBraces(customerid.id);
            const customerPropertyName = generateLookupObjectPropertyName(customerid.entityType, "customerid_");
            incidentToCreate[customerPropertyName] = generateLookupObjectValue(customerid.entityType, customeridId);
        }
      
        const primaryEntityName = formContext.data.entity.getEntityName();
        const primaryEntityId = Utils.JsExtantions.String.RemoveBraces(formContext.data.entity.getId());
        const parentCasePropertyName = generateLookupObjectPropertyName("parentcaseid");
        incidentToCreate[parentCasePropertyName] = generateLookupObjectValue(primaryEntityName, primaryEntityId);

        const portfolioId = formContext.getAttribute('alt_portfolioid').getValue()
            && formContext.getAttribute('alt_portfolioid').getValue()[0];
        if (portfolioId) {
            const portfolioPropertyName = generateLookupObjectPropertyName('alt_PortfolioId');
            incidentToCreate[portfolioPropertyName] = generateLookupObjectValue(portfolioId.entityType, Utils.JsExtantions.String.RemoveBraces(portfolioId.id));
        }

        createIncident(incidentToCreate, name, successCallback);
    };

    return {
        IncidentsBulkCreateOnClick: incidentsBulkCreateOnClick,
        ChangeIncidentStatusOnClick: changeIncidentStatusOnClick,
        CancelIncidentOnClick: cancelIncidentOnClick
    };
})();
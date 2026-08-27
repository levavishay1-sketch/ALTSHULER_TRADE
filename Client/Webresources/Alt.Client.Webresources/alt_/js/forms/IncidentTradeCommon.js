/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Global.js" />

var IncidentTradeCommonBL = (function () {

    const RESTRICT_OPEN_INCIDENT_FORM_ALERT_TEXT = 'לא ניתן ליצור אירוע שלא מתוך חשבון ';
    const ACCOUNTHOLDERTYE_IS_NOT_DEFINED_ALERT_TEXT = "סוגי בעל חשבון לא הוגדרו. נא לפנות למנהל מערכת.";
    const guidEmpty = '{00000000-0000-0000-0000-000000000000}';
    const formAttributes = {
        alt_incidentstatusid: 'alt_incidentstatusid',
        alt_subject1id: 'alt_subject1id',
        alt_subject2id: 'alt_subject2id',
        ownerid: 'ownerid',
        alt_bulkincidentsparentbit: 'alt_bulkincidentsparentbit',
        alt_automaticincidenttemplatekey: 'alt_automaticincidenttemplatekey',
        customerid: 'customerid',
        alt_portfolioid: 'alt_portfolioid',
        caseorigincode: 'caseorigincode',
        parentcaseid: 'parentcaseid',
        alt_responsiblesystemuserid: 'alt_responsiblesystemuserid',
        alt_bpfstagesjson: 'alt_bpfstagesjson'
    };
    const quickCreateTabName = 'tab_1';
    const quickCreateFormClassifictionSectionName = 'tab_1_column_2_section_1';
    const mainFormIncidentClassificationSectionName = 'IncidentClassificationSection';
    const mainFormGeneralTabName = 'GeneralTab';

    const attributesAffectedByBulkIncidentsParentBit = [
        formAttributes.alt_subject1id,
        formAttributes.alt_subject2id,
        formAttributes.alt_incidentstatusid
    ];

    const teamDirectionCode = {
        recieve: 1,
        assign: 2
    };

    let formContext;
    let crmFormTypes;
    let automaicIncidentTemplateCode;
    let teamIds = [];
    let handlingTeamIds = [];
    let systemUserIds = [];
    let userSettings;

    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();
        userSettings = Xrm.Utility.getGlobalContext().userSettings;

        const legalityCreationSetting =
            [
                {
                    conditionCallback: validateConditionCallback,
                    errorMessage: RESTRICT_OPEN_INCIDENT_FORM_ALERT_TEXT,
                }
            ];

        const formType = formContext.ui.getFormType();
        crmFormTypes = Utils.CrmPage.FormType;

        switch (formType) {
            case crmFormTypes.Create: {

                Utils.CrmPage.HandleRecordLegalityCreation(formContext, legalityCreationSetting, function () {
                    initOnChange();
                    initFormData();
                    initUI();
                    setCustomFiltersByAccountHolder();
                });
                break;
            }
            default:
                break;
        }
    };

    const initOnChange = function () {

        formContext.getAttribute(formAttributes.alt_bulkincidentsparentbit).addOnChange(bulkIncidentsParentBitOnChange);
        formContext.getAttribute(formAttributes.alt_subject1id).addOnChange(subject1onChange);
        formContext.getAttribute(formAttributes.alt_subject2id).addOnChange(subject2onChange);
        formContext.getAttribute(formAttributes.alt_incidentstatusid).addOnChange(incidentStatusOnChange);
        formContext.getAttribute(formAttributes.ownerid).addOnChange(ownerOnChange);
    };

    const initFormData = function () {

        handleReponsibleUserAndOwner();
        Utils.Global.GetAllTeamsByUserId(formContext, userSettings.userId, getAllTeamsSuccessCallback, null);

        formContext.getControl(formAttributes.ownerid).addPreSearch(ownerPreSearch);
        formContext.getControl(formAttributes.alt_responsiblesystemuserid).addPreSearch(responsibleSystemUserPreSearch);
    };

    const initUI = function () {

        disableAttributesWithValue();
        handleFormUIByBulkIncidentsParentBit();
        Utils.Global.RemoveOptionsetValuesByGlobalParams(formContext, "TradeOptionSetsValuesToRemove", "incident", formAttributes.caseorigincode);
    };

    const bulkIncidentsParentBitOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();

        var isBulkIncidents = formContext.getAttribute(formAttributes.alt_bulkincidentsparentbit).getValue();
        handleFormUIByBulkIncidentsParentBit(isBulkIncidents);
        setAutomaicIncidentTemplateCode(isBulkIncidents);
    };

    const subject1onChange = function (executionContext) {

        if (executionContext) {
            formContext = executionContext.getFormContext();
        }
        if (formContext.ui.getFormType() == crmFormTypes.Create) {
            const subject1Id = formContext.getAttribute(formAttributes.alt_subject1id).getValue()
                && formContext.getAttribute(formAttributes.alt_subject1id).getValue()[0];
            if (subject1Id) {
                setSubject2CustomView(subject1Id.id);
            }
            anySubjectOnChange(executionContext);
        }
    };

    const subject2onChange = function (executionContext) {

        if (executionContext) {
            formContext = executionContext.getFormContext();
        }

        if (formContext.ui.getFormType() === Utils.CrmPage.FormType.Create) {
            setIncidentStatusCustomView();
            subject2OnChangeOnCreate();
        }
        anySubjectOnChange(executionContext, formAttributes.alt_subject2id);
        ownerOnChange(executionContext);
    };

    const subject2OnChangeOnCreate = function () {

        Utils.CrmPage.HandleControlsVisibleMode(formContext, [formAttributes.ownerid], false);

        const subject2Value = formContext.getAttribute(formAttributes.alt_subject2id).getValue();
        const subject2ValueId = subject2Value && subject2Value[0] && subject2Value[0].id;

        if (subject2ValueId) {

            //handleOwner(subject2ValueId);
            handleDefaultIncidentStatusBySubject2(subject2ValueId);

        } else {
            Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.alt_incidentstatusid, true);
            formContext.getAttribute(formAttributes.alt_incidentstatusid).setValue(null);
        }
    };

    const anySubjectOnChange = function (executionContext, attributeName) {

        if (executionContext) {
            formContext = executionContext.getFormContext();
        }
        handleSubjectVisibility(attributeName || executionContext.getEventSource().getName(), true);
    };

    const incidentStatusOnChange = function () {

        if (formContext.ui.getFormType() === Utils.CrmPage.FormType.Create) {

            incidentStatusOnChangeOnCreate();
        }
    };

    const incidentStatusOnChangeOnCreate = function () {

        const incidentStatusValue = formContext.getAttribute(formAttributes.alt_incidentstatusid).getValue();
        const incidentStatusId = incidentStatusValue && incidentStatusValue[0] && incidentStatusValue[0].id;

        if (incidentStatusId) {
            handleOwnerByIncidentStatus(incidentStatusId);
        }
        else {
            formContext.getAttribute(formAttributes.ownerid).setValue(null);
        }
    };

    const ownerOnChange = function (executionContext) {

        if (executionContext) {
            formContext = executionContext.getFormContext();
        }

        const ownerValue = formContext.getAttribute(formAttributes.ownerid).getValue();
        const ownerValueId = ownerValue && ownerValue[0] && ownerValue[0].id;

        systemUserIds.length = 0;
        if (ownerValueId) {
            getAllUsersByTeamId(ownerValueId);
        }

        const responsibleSystemUser = formContext.getAttribute(formAttributes.alt_responsiblesystemuserid);
        if (responsibleSystemUser) {
            const responsibleSystemUserValue = responsibleSystemUser.getValue();
            const responsibleSystemUserValueId = responsibleSystemUserValue && responsibleSystemUserValue[0] && responsibleSystemUserValue[0].id;

            if (responsibleSystemUserValueId
                && responsibleSystemUserValueId.toLowerCase() !== userSettings.userId.toLowerCase()) {

                formContext.getAttribute(formAttributes.alt_responsiblesystemuserid).setValue(null);
            }
            Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.alt_responsiblesystemuserid, false)
            formContext.getControl(formAttributes.alt_responsiblesystemuserid).setDisabled(false);
        }
    };

    const handleDefaultIncidentStatusBySubject2 = function (subject2Id) {

        const select = '_alt_defaultincidentstatusid_value';

        Utils.Server.Retrieve('alt_subject2', subject2Id, select, null, function (result) {
            const incidentStatus = result.alt_defaultincidentstatusid;
            if (incidentStatus && incidentStatus.Id) {
                Utils.CrmPage.SetLookup(formContext, formAttributes.alt_incidentstatusid, incidentStatus.Id, incidentStatus.Name, incidentStatus.LogicalName);
            } else {
                formContext.getAttribute(formAttributes.alt_incidentstatusid).setValue(null);
            }
            Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.alt_incidentstatusid, false);
        }, null);

    };

    const handleReponsibleUserAndOwner = function () {

        let responsibleUser = formContext.getAttribute(formAttributes.ownerid).getValue();
        if (!responsibleUser) {
            Utils.CrmPage.SetLookup(formContext, formAttributes.alt_responsiblesystemuserid, userSettings.userId, userSettings.userName, 'systemuser');
        }
        formContext.getAttribute(formAttributes.ownerid).setValue(null);
        ownerOnChange();
    };

    const getAllUsersByTeamId = function (teamId) {
        Utils.Global.GetAllUsersByTeamId(formContext, teamId, function (result) {
            for (let i in result) {
                systemUserIds.push(result[i].systemuserid);
            }
        });
    };

    const handleSubjectVisibility = function (subjectName, isOnChange) {

        let subjectNumber = subjectName.replace(/[^0-9]/g, '');
        if (!isNaN(subjectNumber)) {

            let subjectSerialNumber = parseInt(subjectNumber);
            if (subjectSerialNumber < 2) {

                const nextSubjectName = subjectName.replace(subjectNumber, parseInt(subjectNumber) + 1);
                const nextSubjectDisabled = formContext.getAttribute(subjectName).getValue() ? false : true;
                Utils.CrmPage.SetControlDisabledMode(formContext, nextSubjectName, nextSubjectDisabled);
                if (isOnChange) {
                    formContext.getAttribute(nextSubjectName).setValue(null);
                }

                if (subjectSerialNumber === 1) {
                    subject2onChange();
                }
                handleSubjectVisibility(nextSubjectName, isOnChange);
            }
        }
    };

    const handleFormUIByBulkIncidentsParentBit = function (isBulkIncidents) {

        toggleCaseClassifictionSection(isBulkIncidents);

        const requiredLevel = isBulkIncidents ? Utils.CrmPage.RequirementLevel.None : Utils.CrmPage.RequirementLevel.Required;
        attributesAffectedByBulkIncidentsParentBit.forEach(function (attributeName) {
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, attributeName, requiredLevel);
        });
    };

    const toggleCaseClassifictionSection = function (isBulkIncidents) {

        const isMainForm = formContext.ui.tabs.get(quickCreateTabName) ? false : true;
        const tabName = isMainForm ? mainFormGeneralTabName : quickCreateTabName;
        const caseClassificationSectionName = isMainForm ? mainFormIncidentClassificationSectionName : quickCreateFormClassifictionSectionName;
        Utils.CrmPage.SetSectionVisibleMode(formContext, tabName, caseClassificationSectionName, !isBulkIncidents);
    };

    const disableAttributesWithValue = function () {

        const attributes = [
            formAttributes.customerid,
            formAttributes.alt_portfolioid
        ];
        attributes.forEach(function (attributeName) {
            let isDisabled = formContext.getAttribute(attributeName).getValue() ? true : false;
            Utils.CrmPage.SetControlDisabledMode(formContext, attributeName, isDisabled);
        });
    };

    const setAutomaicIncidentTemplateCode = function (isBulkIncidents) {

        if (isBulkIncidents) {
            if (automaicIncidentTemplateCode) {
                formContext.getAttribute(formAttributes.alt_automaticincidenttemplatekey).setValue(automaicIncidentTemplateCode);
            }
            Utils.Global.GetGlobalParamValue('TradeParentIncidentAutomaticTemplateCode', function (globalParam) {
                if (globalParam) {
                    automaicIncidentTemplateCode = globalParam;
                    formContext.getAttribute(formAttributes.alt_automaticincidenttemplatekey).setValue(automaicIncidentTemplateCode);
                    Utils.CrmPage.SetHiddenFieldsToUnRequired(formContext);
                }
                else {
                    Xrm.Navigation.openAlertDialog({ text: Utils.CrmPage.CommonRequestFailedMessage });
                }
            });
            const subject1 = formContext.getAttribute(formAttributes.alt_subject1id);
            subject1.setValue(null);
            subject1.fireOnChange();
        }
        else {
            formContext.getAttribute(formAttributes.alt_automaticincidenttemplatekey).setValue(null);
        }
    };

    //const handleOwner = function (subject2Id) {
    //    const select = '_alt_teamid_value';
    //    const filter = '_alt_subject2id_value eq ' + subject2Id;

    //    Utils.Server.RetrieveMultiple('alt_subjecthandlingteam', select, filter, null, null, handleOwnerSuccessCallback, null);
    //};

    const handleOwnerByIncidentStatus = function (incidentIdStatusId) {
        const select = '_alt_teamid_value';
        const filter = '_alt_incidentstatusid_value eq ' + incidentIdStatusId + ' and alt_directioncode eq ' + teamDirectionCode.recieve + ' and statecode eq ' + customEntityStateCode.Active;

        Utils.Server.RetrieveMultiple('alt_incidentstatushandlingteam', select, filter, null, null, handleOwnerSuccessCallback, null);
    };

    const handleOwnerSuccessCallback = function (result) {

        if (result) {
            let team = null;
            handlingTeamIds.length = 0;
            result.forEach(function (handlingTeam) {
                //if (teamIds.indexOf(handlingTeam.alt_teamid.Id) !== -1) {
                team = handlingTeam.alt_teamid;
                handlingTeamIds.push(team.Id);
                // }
            });
            const manualChooseOwner = handlingTeamIds.length !== 1;
            if (manualChooseOwner) {
                Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.ownerid, Utils.CrmPage.RequirementLevel.Required);
            }
            else {

                Utils.CrmPage.SetLookup(formContext, 'ownerid', team.Id, team.Name, team.LogicalName);
                ownerOnChange();
            }
            Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.ownerid, manualChooseOwner);
        }
    };

    const validateConditionCallback = function () {

        return formContext.getAttribute(formAttributes.customerid).getValue()
            || formContext.getAttribute(formAttributes.alt_portfolioid).getValue();
    };

    const getAllTeamsSuccessCallback = function (result) {

        for (let i in result) {
            teamIds.push(result[i].teamid);
        }

        formContext.getControl(formAttributes.ownerid).setEntityTypes(['team']);

        setSubject1CustomView();
        //setIncidentStatusCustomView();
    };

    const setSubject1CustomView = function () {

        const entityName = "alt_subject1";
        const viewDisplayName = "נושא 1";

        const filter = Utils.Global.CreateLookupCustomFilter(teamIds, 'alt_teamid');
        const fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
            "<entity name='alt_subject1'>" +
            "<attribute name='alt_subject1id' />" +
            "<attribute name='alt_name' />" +
            "<filter type='and'>" +
            "<condition attribute='statecode' operator='eq' value='0' />" +
            "<condition attribute='alt_userdisplaybit' operator='eq' value='1' />" +
            "</filter>" +
            "<link-entity name='alt_subject2' from='alt_subject1id' to='alt_subject1id' link-type='inner' alias='ai'>" +
            "<filter type='and'>" +
            "<condition attribute='statecode' operator='eq' value='0' />" +
            "</filter>" +
            "<link-entity name='alt_subjecthandlingteam' from='alt_subject2id' to='alt_subject2id' link-type='inner' alias='aj'>" +
            "<filter type='and'>" +
            "<condition attribute='statecode' operator='eq' value='0' />" +
            filter +
            "</filter>" +
            "</link-entity>" +
            "</link-entity>" +
            "</entity>" +
            "</fetch>";

        const layoutXml = "<grid name='resultset' jump='alt_name' select='1' icon='1' preview='0'>" +
            "<row name='result' id='alt_subject1id'>" +
            "<cell name='alt_name' width='300' />" +
            "</row>" +
            "</grid>";

        formContext.getControl(formAttributes.alt_subject1id).addCustomView('{00000000-0000-0000-0000-000000000001}', entityName, viewDisplayName, fetchXml, layoutXml, true);
    };

    const setSubject2CustomView = function (subject1Id) {

        const entityName = "alt_subject2";
        const viewDisplayName = "נושא 2";

        const filter = Utils.Global.CreateLookupCustomFilter(teamIds, 'alt_teamid');

        const fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
            "<entity name='alt_subject2'>" +
            "<attribute name='alt_name' />" +
            "<attribute name='alt_subject2id' />" +
            "<filter type='and'>" +
            "<condition attribute='statecode' operator='eq' value='0' />" +
            "</filter>" +
            "<link-entity name='alt_subjecthandlingteam' from='alt_subject2id' to='alt_subject2id' link-type='inner' alias='al'>" +
            "<filter type='and'>" +
            "<condition attribute='statecode' operator='eq' value='0' />" +
            filter +
            "</filter>" +
            "</link-entity>" +
            "</entity>" +
            "</fetch>";

        const layoutXml = "<grid name='resultset' jump='alt_name' select='1' icon='1' preview='0'>" +
            "<row name='result' id='alt_subject2id'>" +
            "<cell name='alt_name' width='300' />" +
            "</row>" +
            "</grid>";

        formContext.getControl(formAttributes.alt_subject2id).addCustomView('{00000000-0000-0000-0000-000000000002}', entityName, viewDisplayName, fetchXml, layoutXml, true);
    };

    const setIncidentStatusCustomViewOld = function () {

        const entityName = "alt_incidentstatus";
        const viewDisplayName = "מצבי אירוע";

        const fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
            "<entity name='alt_incidentstatus'>" +
            "<attribute name='alt_incidentstatusid' />" +
            "<attribute name='alt_name' />" +
            "<filter type='and'>" +
            "<condition attribute='alt_enableforcreatebit' value='1' operator='eq' /> " +
            "<condition attribute='statecode' operator='eq' value='0' />" +
            "</filter>" +
            "</entity>" +
            "</fetch>";

        const layoutXml = "<grid name='resultset' jump='alt_name' select='1' icon='1' preview='0'>" +
            "<row name='result' id='alt_incidentstatusid'>" +
            "<cell name='alt_name' width='150' />" +
            "</row>" +
            "</grid>";

        formContext.getControl(formAttributes.alt_incidentstatusid).addCustomView('{00000000-0000-0000-0000-000000000003}', entityName, viewDisplayName, fetchXml, layoutXml, true);
    };

    const setPortfolioCustomView = function (customerId, globalParam) {

        const viewId = '{00000000-0000-0000-0000-000000000004}';
        const entityName = 'alt_portfolio';
        const viewDisplayName = 'חשבונות בעלי חשבון';
        const layoutXml = "<grid name='resultset' jump='alt_name' select='1' icon='1' preview='0'>" +
            "<row name='result' id='alt_portfolioid'>" +
            "<cell name='alt_name' width='200' />" +
            "</row>" +
            "</grid>";
        const fetchXml = generatePortfolioFilterByCustomerFetchXml(customerId, globalParam);
        formContext.getControl(formAttributes.alt_portfolioid).addCustomView(viewId, entityName, viewDisplayName, fetchXml, layoutXml, true);
    };

    const setCustomFiltersByAccountHolder = function () {

        if (formContext.getAttribute(formAttributes.alt_portfolioid).getValue()) {

            const portfolioId = formContext.getAttribute(formAttributes.alt_portfolioid).getValue()[0];
            handleCustomerCustomView(portfolioId);

        } else if (formContext.getAttribute(formAttributes.customerid).getValue()) {

            const customerId = formContext.getAttribute(formAttributes.customerid).getValue()[0];
            handlePortfolioCustomView(customerId);
        }
    };

    const handlePortfolioCustomView = function (customerId) {

        if (customerId) {
            Utils.Global.GetGlobalParamValue('AccountHolderTypeServiceAllowed', function (globalParam) {
                if (globalParam) {
                    setPortfolioCustomView(customerId.id, globalParam);
                }
                else {
                    handleaccountHolderTypeError(formAttributes.alt_portfolioid);
                }
            }, function (error) { handleaccountHolderTypeError(formAttributes.alt_portfolioid); });
        }
        else {
            removeLookupOptions(formAttributes.alt_portfolioid);
        }
    };

    const handleCustomerCustomView = function (portfolioId) {

        if (portfolioId) {
            Utils.Global.GetGlobalParamValue('AccountHolderTypeServiceAllowed', function (globalParam) {
                if (globalParam) {

                    const fetchXml = generateCustomerFilterByPortfolioFetchXml(portfolioId.id, globalParam);
                    Utils.Global.FilterCustomers(formContext, formAttributes.customerid, 'alt_accountholder', fetchXml, 'alt_customerid', true);
                }
                else {
                    handleaccountHolderTypeError(formAttributes.customerid);
                }
            }, function (error) { handleaccountHolderTypeError(formAttributes.customerid) });
        }
        else {
            Utils.Global.FilterCustomers(formContext, formAttributes.customerid, null);
        }
    };

    const ownerPreSearch = function () {
        const teamsFilter = Utils.Global.CreateLookupCustomFilter(handlingTeamIds, 'teamid');
        const filter = "<filter type='and'>" +
            teamsFilter +
            "</filter>";
        formContext.getControl('ownerid').addCustomFilter(filter);
    };

    const responsibleSystemUserPreSearch = function () {
        const usersFilter = Utils.Global.CreateLookupCustomFilter(systemUserIds, 'systemuserid');
        const filter = "<filter type='and'>" +
            usersFilter +
            "</filter>";

        formContext.getControl('alt_responsiblesystemuserid').addCustomFilter(filter);
    };

    const generatePortfolioFilterByCustomerFetchXml = function (customerid, accountHolderTypes) {

        const accountHolderCondition = accountHolderTypes ? generateAccountHolerTypeCondition(accountHolderTypes) : null;

        const fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
            "<entity name='alt_portfolio'>" +
            "<attribute name='alt_name' />" +
            "<attribute name='alt_portfolioid' />" +
            '<link-entity name="alt_accountholder" from="alt_portfolioid" to="alt_portfolioid" link-type="inner" alias="ab">' +
            "<filter type='and'>" +
            accountHolderCondition +
            "<condition attribute='alt_customerid' value='" + customerid + "' operator='eq' /> " +
            "<condition attribute='statecode' operator='eq' value='0' />" +
            "</filter>" +
            '</link-entity>' +
            "</entity>" +
            "</fetch>";
        return fetchXml;
    };

    const generateAccountHolerTypeCondition = function (accountHolderTypes) {

        let condition = null;
        if (accountHolderTypes) {

            let types = accountHolderTypes.split(",");
            if (Array.isArray(types)) {

                condition = "<condition attribute='alt_accountholdertypecode' operator='in'>";
                types.forEach(function (type) {
                    if (type) {
                        condition += "<value>" + type.trim() + "</value>";
                    }
                });
                condition += "</condition>";
            }
        }
        return condition;
    };

    const generateCustomerFilterByPortfolioFetchXml = function (portfolioId, accountHolderTypes) {

        const accountHolderCondition = accountHolderTypes ? generateAccountHolerTypeCondition(accountHolderTypes) : null;

        const fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
            "<entity name='alt_accountholder'>" +
            "<attribute name='alt_name' />" +
            "<attribute name='alt_customerid' />" +
            "<filter type='and'>" +
            accountHolderCondition +
            "<condition attribute='alt_portfolioid' value='" + portfolioId + "' operator='eq' /> " +
            "<condition attribute='statecode' operator='eq' value='0' />" +
            "</filter>" +
            "</entity>" +
            "</fetch>";
        return fetchXml;
    };

    const handleaccountHolderTypeError = function (attributeName) {

        Xrm.Navigation.openAlertDialog({ text: ACCOUNTHOLDERTYE_IS_NOT_DEFINED_ALERT_TEXT }).then(function () {
            removeLookupOptions(attributeName);
        });
    };

    const removeLookupOptions = function (attributeName) {
        switch (attributeName) {
            case formAttributes.customerid: {

                Utils.Global.FilterCustomers(formContext, formAttributes.customerid, null);
                break;
            }
            case formAttributes.alt_portfolioid: {

                setPortfolioCustomView(guidEmpty);
                break;
            }
            default:
        }
    };

    const setIncidentStatusCustomView = function () {
        const subject2Value = formContext.getAttribute(formAttributes.alt_subject2id).getValue();

        const subject2Id = subject2Value && subject2Value[0] && subject2Value[0].id;

        if (!subject2Id) {
            return;
        }

        const entityName = "alt_incidentstatus";
        const viewDisplayName = "מצבי אירוע";

        const teamFilter = Utils.Global.CreateLookupCustomFilter(teamIds, 'alt_teamid');

        const fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
            "<entity name='alt_incidentstatus'>" +
            "<attribute name='alt_incidentstatusid' />" +
            "<attribute name='alt_name' />" +

            "<filter type='and'>" +
            "<condition attribute='alt_enableforcreatebit' value='1' operator='eq' />" +
            "<condition attribute='statecode' value='0' operator='eq' />" +
            "<condition attribute='alt_subject2id' operator='eq' value='" +
            subject2Id.replace(/[{}]/g, "") +
            "' />" +
            "</filter>" +

            "<link-entity name='alt_incidentstatushandlingteam' " +
            "from='alt_incidentstatusid' " +
            "to='alt_incidentstatusid' " +
            "link-type='inner' " +
            "alias='handlingteam'>" +

            "<filter type='and'>" +
            "<condition attribute='statecode' value='0' operator='eq' />" +
            "<condition attribute='alt_directioncode' value='" +
            teamDirectionCode.assign +
            "' operator='eq' />" +
            teamFilter +
            "</filter>" +

            "</link-entity>" +

            "</entity>" +
            "</fetch>";

        const layoutXml = "<grid name='resultset' jump='alt_name' select='1' icon='1' preview='0'>" +
            "<row name='result' id='alt_incidentstatusid'>" +
            "<cell name='alt_name' width='150' />" +
            "</row>" +
            "</grid>";

        formContext.getControl(formAttributes.alt_incidentstatusid).addCustomView(
            '{00000000-0000-0000-0000-000000000003}',
            entityName,
            viewDisplayName,
            fetchXml,
            layoutXml,
            true
        );
    };

    return {
        OnLoad: onLoad,
        formAttributes: formAttributes,
        HandleFormUIByBulkIncidentsParentBit: handleFormUIByBulkIncidentsParentBit
    };
})();
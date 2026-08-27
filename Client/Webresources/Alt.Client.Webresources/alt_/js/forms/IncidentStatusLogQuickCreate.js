/// <reference path="../utils/Utils.Server.js" />
/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Enums.js" />
/// <reference path="IncidentStatusLogCommon.js" />

var IncidentStatusLogQuickCreate = (function () {

    let formContext;
    let openedChildCases = false;
    let openedActivities = [];
    let teamIds = [];
    let statusCodeCloseIncident;
    let hasBlockingActivities = false; 
    let activitiesLoaded = false; 

    const formAttributes = {
        alt_toincidentstatusid: 'alt_toincidentstatusid',
        alt_incidentid: 'alt_incidentid',
        alt_fromincidentstatusid: 'alt_fromincidentstatusid',
        alt_assigningteamid: 'alt_assigningteamid',
        alt_teamid: 'alt_teamid'
    };

    const teamDirectionCode = {
        recieve: 1,
        assign: 2
    };

    const defaultViewId = {
        alt_toincidentstatusid: '{00000000-0000-0000-0000-000000000123}',
        alt_teamid: '{00000000-0000-0000-0000-000000222222}',
        alt_assigningteamid: '{00000000-0000-0000-0000-000000000111}'
    };

    const UNCLOSED_CHILD_CASES_ALERT_TITLE = 'לאירוע משויכים אירועים בנים, לא ניתן לסגור את האירוע!';
    const UNCLOSED_ACTIVITIES_ALERT_TITLE = 'לאירוע יש פעילויות פתוחות, לא ניתן לסגור את האירוע!';
    const UNLOADED_ACTIVITIES_ALERT_TITLE = 'המערכת עדיין טוענת נתונים. נסה שוב בעוד רגע.';


    const activitiesToIgnore = new Set([
        "alt_archivedocumentsearch"
    ]);

    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();

        IncidentStatusLogCommonBL.OnLoad(executionContext);
        initFormProperties();
        initOnChange();
    };

    const initOnChange = function () {
        formContext.getAttribute(formAttributes.alt_toincidentstatusid).addOnChange(toIncidentStatusOnChange);
    };

    const initFormProperties = function () {

        getAllTeamsOfSystemUser();

        const incidentValue = formContext.getAttribute(formAttributes.alt_incidentid).getValue();
        const incidentValueId = incidentValue && incidentValue[0] && Utils.JsExtantions.String.RemoveBraces(incidentValue[0].id);

        if (incidentValueId) {
            setOpenedActivities(incidentValueId);
            setOpenedChildCases(incidentValueId);
        }
    };

    const toIncidentStatusOnChange = function (executionContext) {
        formContext = executionContext.getFormContext();

        if (activitiesLoaded)
        {
            const toStatusValue = formContext.getAttribute(formAttributes.alt_toincidentstatusid).getValue();
            const toStatusValueId = toStatusValue && toStatusValue.length > 0 && Utils.JsExtantions.String.RemoveBraces(toStatusValue[0].id);

            if (toStatusValueId)
            {
                if (hasBlockingActivities)
                {
                    displayAlertDialog(UNCLOSED_ACTIVITIES_ALERT_TITLE);
                }
                else
                {
                    getDataFromIncidentStatus(toStatusValueId);
                }
            } else
            {
                disableAndClearTeamFields();
            }
        }
        else
        {
            displayAlertDialog(UNLOADED_ACTIVITIES_ALERT_TITLE);
        }    
    };

    const getDataFromIncidentStatus = function (incidentStatusId) {
        const select = 'alt_incidentstatuscode';
        Utils.Server.Retrieve('alt_incidentstatus', incidentStatusId, select, null, function (result) {
            if ((ifStatusCodeCloseIncident(result.alt_incidentstatuscode) && openedChildCases)) {

                displayAlertDialog(UNCLOSED_CHILD_CASES_ALERT_TITLE);
            }
            else {
                handleTeamsByIncidentStatus(incidentStatusId);
            }
        }, null);
    };

    const getAllTeamsOfSystemUser = function () {
        let userSettings = Xrm.Utility.getGlobalContext().userSettings;
        const userId = userSettings.userId;
        Utils.Global.GetAllTeamsByUserId(formContext, userId, getAllTeamsSuccessCallback, null);
    };


    const getAllTeamsSuccessCallback = function (result) {

        const fromStatusValue = formContext.getAttribute(formAttributes.alt_fromincidentstatusid).getValue();
        const fromStatusValueId = fromStatusValue && fromStatusValue.length > 0 && Utils.JsExtantions.String.RemoveBraces(fromStatusValue[0].id);

        for (let i in result) {
            teamIds.push(result[i].teamid);
        }
        setToIncidentStatusIdCustomView(fromStatusValueId, teamIds);
    };


    const setOpenedChildCases = function (incidentValueId) {

        const select = 'statecode';
        const filter = '_parentcaseid_value eq ' + incidentValueId + ' and statecode eq ' + incidentStateCodes.Active;

        Utils.Server.RetrieveMultiple('incident', select, filter, null, null, function (result) {
            openedChildCases = result;
        }, null);
    };

    const setOpenedActivities = function (incidentValueId) {

        Utils.Global.GetAllActiveActivitiesByRegardingObject(incidentValueId, function (result)
        {
            activitiesLoaded = true;
            if (!Array.isArray(result) || result.length === 0)
            {
                openedActivities = [];
                hasBlockingActivities = false;
            }
            else
            {
                openedActivities = result.filter(function (activity)
                {
                    return activity && !activitiesToIgnore.has(activity.activitytypecode);
                });

                hasBlockingActivities = openedActivities.length > 0;
            }
        });
    };

    const setToIncidentStatusIdCustomView = function (fromStatusId, teamIds) {

        const entityName = "alt_incidentstatus";
        const viewDisplayName = "מצבי אירוע";

        const filter = createTeamsFilter(teamIds, 'alt_teamid');

        const fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
            "<entity name='alt_incidentstatus'>" +
            "<attribute name='alt_incidentstatusid' />" +
            "<attribute name='alt_name' />" +
            "<order attribute='alt_name' descending='false' />" +
            "<filter type='and'>" +
            "<condition attribute='statecode' operator='eq' value='0' />" +
            "<condition attribute='alt_incidentstatusid' operator='ne' value='{" + fromStatusId + "}' /> " +
            "</filter>" +
            "<link-entity name='alt_incidentstatushandlingteam' from='alt_incidentstatusid' to='alt_incidentstatusid' link-type='inner' alias='aj'>" +
            "<filter type='and'>" +
            "<condition attribute='statecode' operator='eq' value='0' />" +
            "<condition attribute='alt_directioncode' operator='eq' value='2' />" +
            filter +
            "</filter>" +
            "</link-entity>" +
            "</entity>" +
            "</fetch>";

        const layoutXml = "<grid name='resultset' jump='alt_name' select='1' icon='1' preview='0'>" +
            "<row name='result' id='alt_incidentstatusid'>" +
            "<cell name='alt_name' width='150' />" +
            "</row>" +
            "</grid>";

        formContext.getControl(formAttributes.alt_toincidentstatusid).addCustomView(defaultViewId[formAttributes.alt_toincidentstatusid], entityName, viewDisplayName, fetchXml, layoutXml, true);
    };

    const handleTeamsByIncidentStatus = function (incidentStatusId) {
        const select = '_alt_teamid_value,alt_directioncode';
        const filter = 'statecode eq 0 and _alt_incidentstatusid_value eq ' + incidentStatusId;
        Utils.Server.RetrieveMultiple('alt_incidentstatushandlingteam', select, filter, null, null, handleTeamsByIncidentStatusSuccessCallback, null);
    };

    const handleTeamsByIncidentStatusSuccessCallback = function (result) {

        const assignTeams = [];
        const recieveTeams = [];
        let defaultAssignTeam = null;
        let defaultRecieveTeam = null;

        if (result) {
            result.forEach(function (handlingTeam, index) {

                if (handlingTeam.alt_directioncode === teamDirectionCode.assign
                    && teamIds.indexOf(handlingTeam.alt_teamid.Id) !== -1) {

                    defaultAssignTeam = handlingTeam.alt_teamid;
                    assignTeams.push(handlingTeam.alt_teamid.Id);

                }  else if (handlingTeam.alt_directioncode === teamDirectionCode.recieve) {

                    defaultRecieveTeam = handlingTeam.alt_teamid;
                    recieveTeams.push(handlingTeam.alt_teamid.Id);
                }
            });
        }
        if (statusCodeCloseIncident
            && recieveTeams.length == 0
            && assignTeams.length == 1) {

            defaultRecieveTeam = defaultAssignTeam;
            recieveTeams.push(defaultRecieveTeam.Id);
        }
        handleTeamAttributes(formAttributes.alt_assigningteamid, assignTeams, defaultAssignTeam);
        handleTeamAttributes(formAttributes.alt_teamid, recieveTeams, defaultRecieveTeam);
    };

    const handleTeamAttributes = function (attributeName, teams, defaultTeam) {
        if (teams && teams.length == 1) {
            Utils.CrmPage.SetLookup(formContext, attributeName, defaultTeam.Id, defaultTeam.Name, defaultTeam.LogicalName);
        } else if (formContext.getAttribute(attributeName).getValue()){
            formContext.getAttribute(attributeName).setValue(null);
        }
        formContext.getControl(attributeName).setDisabled(false);
        setTeamIdCustomView(attributeName, teams);
    };

    const setTeamIdCustomView = function (teamFieldName, teamIds) {
        const entityName = "team";
        const viewDisplayName = "צוותים מקושרים";

        const filter = createTeamsFilter(teamIds, 'teamid');

        const fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
            "<entity name='team'>" +
            "<attribute name='name' />" +
            "<attribute name='teamid' />" +
            "<attribute name='teamtype' />" +
            "<filter type='and'>" +
            filter +
            "</filter>" +
            "</entity>" +
            "</fetch>";

        const layoutXml = "<grid name='resultset' object='9' jump='teamid' select='1' icon='1' preview='0'>" +
            "<row name='result' id='teamid'>" +
            "<cell name='name' width='150' />" +
            "<cell name='teamtype' width='150' />" +
            "</row>" +
            "</grid>";

        formContext.getControl(teamFieldName).addCustomView(defaultViewId[teamFieldName], entityName, viewDisplayName, fetchXml, layoutXml, true);
    };

    const createTeamsFilter = function (teams, teamFieldName) {
        let filter = "<filter type='or'>";
        if (!teams || teams.length === 0) {
            filter += "<condition attribute='" + teamFieldName + "' operator='eq' value='{00000000-0000-0000-0000-000000000000}' />";
        } else {
            teams.forEach(function (teamId) {
                filter += "<condition attribute='" + teamFieldName + "' operator='eq' value='{" + teamId + "}' />";
            });
        }

        filter += "</filter>";
        return filter;
    };

    const ifStatusCodeCloseIncident = function (statusCode) {

        statusCodeCloseIncident = false;
        switch (statusCode) {

            case incidentStatusCodes.Solved:
            case incidentStatusCodes.Cancelled:
            case incidentStatusCodes.InformationProvided:
            case incidentStatusCodes.Merged: {

                statusCodeCloseIncident = true;
                break;
            }

            default:
                break;
        }

        return statusCodeCloseIncident;
    };

    const disableAndClearTeamFields = function () {
        [formAttributes.alt_teamid, formAttributes.alt_assigningteamid].forEach(function (attributeName) {
            formContext.getAttribute(attributeName).setValue(null);
            Utils.CrmPage.SetControlDisabledMode(formContext, attributeName, true);
        });
    };

    const displayAlertDialog = function (message) {
        Xrm.Navigation.openAlertDialog({ text: message }).then(
            function success() {
                formContext.getAttribute('alt_toincidentstatusid').setValue(null);
                disableAndClearTeamFields();
            }
        );
    };

    return {
        OnLoad: onLoad
    };
})();

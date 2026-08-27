/// <reference path="../utils/Utils.Global.js" />
/// <reference path="../utils/Utils.Enums.js" />

var LeadRibbonBL = (function () {

    const STATUS_NOT_SELECTED_ALERT_MESSAGE = "לתשומת לבך, לא נבחר סטטוס.";
    const NO_EXISTING_STATUSES_FOR_DISQUALIFY_ALERT_MESSAGE = "לא קיימים סטטוסים לפסילה.";

    const leadStatusCodesGlobalParamName = "LeadDisqualifyStatusCodes";

    const formAttributes = {

        statecode: 'statecode',
        statuscode: 'statuscode',
        alt_treatmentstatusid: 'alt_treatmentstatusid',
        ownerid: 'ownerid',
        alt_senttoivrbit: 'alt_senttoivrbit',
        alt_ivrcampaigncode: 'alt_ivrcampaigncode',
        alt_assigntomebit: 'alt_assigntomebit'
    };

    const entityToDisplayCode = {

        Lead: 1,
        Opportunity: 2
    };

    const ivrCampaignCodes = {
        NewCallBacks: 1
    };

    let disqualifyStatuCodes;

    let formContext;

    const startManualProcess = function (primaryControl) {
        formContext = primaryControl;

        formContext.getAttribute(formAttributes.statuscode).setValue(2);
        formContext.data.save().then(
            function () { },
            function (error) {
                formContext.getAttribute(formAttributes.statuscode).setValue(1);
                console.log(error);
            });
    };

    const disqualifyLeadOnClick = function (primaryControl) {

        formContext = primaryControl;

        if (formContext.data.entity.getIsDirty()) {

            formContext.data.save().then(() => handleTreatmentStatusLookupSelectionSideForm());
        } else {

            handleTreatmentStatusLookupSelectionSideForm();
        }
    };

    const handleTreatmentStatusLookupSelectionSideForm = function () {

        Utils.Global.GetEntityStatusCodesByStateCode("lead", leadStateCode.Disqualified)
            .then((leadStatusCodes) => Utils.Global.GetTreatmentStatusesByRelatedEntityStatusCodes(leadStatusCodes, entityToDisplayCode.Lead, "alt_leadstatuscode"))
            .then((retrievedTreatmentStatuses) => openTreatmentStatusLookupSelectionSideForm(retrievedTreatmentStatuses))
            .then(() => Xrm.Utility.closeProgressIndicator());
    };

    const openTreatmentStatusLookupSelectionSideForm = function (treatmentStatuses) {

        if (treatmentStatuses) {
            let lookupOptions = getLookupOptions("alt_treatmentstatus", treatmentStatuses);

            Xrm.Utility.lookupObjects(lookupOptions).
                then(function (selectedStatuses) {
                    if (selectedStatuses !== null && selectedStatuses.length > 0) {
                        const selectedStatus = selectedStatuses[0];
                        Utils.CrmPage.SetLookup(formContext, formAttributes.alt_treatmentstatusid,
                            selectedStatus.id, selectedStatus.name, selectedStatus.entityType);

                        formContext.data.save();
                    }
                    else {
                        Xrm.Navigation.openAlertDialog({ text: STATUS_NOT_SELECTED_ALERT_MESSAGE });
                    }
                }, function (e) {
                    console.log(e.error.message);
                });
        }
        else {
            Xrm.Navigation.openAlertDialog({ text: NO_EXISTING_STATUSES_FOR_DISQUALIFY_ALERT_MESSAGE });
        }
    };

    const getLookupOptions = function (entityName, entitiesArray) {

        let primaryEntityAttribute = entityName + "id";
        let lookupOptions = {};

        lookupOptions.allowMultiSelect = false;
        lookupOptions.disableMru = true;
        lookupOptions.defaultEntityType = entityName;
        lookupOptions.entityTypes = [entityName];

        let filterConditions = [];
        entitiesArray.forEach(function (entity) {

            filterConditions.push('<condition attribute="' + primaryEntityAttribute + '" operator="eq" uitype="' + entityName + '" value="{' + entity[primaryEntityAttribute] + '}"/>');
        });

        lookupOptions.filters = [{
            filterXml: '<filter type="or">' + filterConditions.join('') + ' </filter>',
            entityLogicalName: entityName
        }];

        return lookupOptions;
    };

    const isDisqualifyLeadEnabled = function (primaryControl) {

        //formContext = primaryControl ? primaryControl : Xrm.Page;
        //let isFormTypeUpdate = formContext.ui.getFormType() == Utils.CrmPage.FormType.Update;
        //return isFormTypeUpdate;
        return false;
    };

    const populateLeadDisqualifyDynamicMenu = async function (commandProperties, primaryControl) {

        formContext = primaryControl ? primaryControl : Xrm.Page;
        getLeadStatusCodesGlobalParam(function () {
            createAndAddPopulationXMLToCommandProperties(commandProperties);
        });
    };

    const getLeadStatusCodesGlobalParam = function (successCallBack) {

        let webApiQuery = Xrm.Page.context.getClientUrl() + `/api/data/v9.2/alt_globalparameters?$filter=alt_name eq '${leadStatusCodesGlobalParamName}'`;

        let req = new XMLHttpRequest();
        req.open('GET', webApiQuery, false);
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.send();
        if (req.readyState == 4) {
            if (req.status == 200) {
                let results = JSON.parse(req.response);
                data = results.value[0].alt_value;
                disqualifyStatuCodes = JSON.parse(data).disqualifyStatuCodes;
                successCallBack();
            }
            else {
                let error = JSON.parse(req.response).error;
                console.log(error.message);
            }
        }
    };

    const createAndAddPopulationXMLToCommandProperties = function (commandProperties) {

        let command = "alt.Form.Lead.Disqualify.FlyoutItem.Command";
        if (commandProperties.SourceControlId) {
            let source = commandProperties.SourceControlId.split('|');
            if (source.length > 3) {
                command = source[0] + "|" + source[1] + "|" + source[2] + "|" + command;
            }
        }
        createPopulationXML(commandProperties, command);
    };

    const createPopulationXML = function (commandProperties, command) {

        let ribbonXml = `<MenuSection Id="alt.Form.Lead.Disqualify.Flyout.MenuSection" Sequence="10">` +
            `<Controls Id="alt.Form.Lead.Disqualify.Flyout.Control">`;

        let buttons = setFlyoutButtons(disqualifyStatuCodes);

        for (let i = 0; i < buttons.length; i++) {
            let name = buttons[i].name;
            let value = buttons[i].id;

            ribbonXml +=
                `<Button Id="` + value +
                `" Command="` + command +
                `" Sequence="` + (i + 1) * 10 +
                `" LabelText="` + name.replace(`"`, `&quot;`) + `" />`;
        }

        ribbonXml += `</Controls></MenuSection>`;
        commandProperties["PopulationXML"] = `<Menu Id="alt.Form.Lead.Disqualify.Flyout.Menu">` + ribbonXml + `</Menu>`;
    };

    const disqualifyLeadFlyoutOnClick = function (commandProperties, primaryControl) {

        formContext = primaryControl ? primaryControl : Xrm.Page;

        const statusCode = formContext.getAttribute(formAttributes.statuscode);
        const prevStatusCodeValue = statusCode.getValue();
        const disqualifyStatusCodeToUpdate = parseInt(commandProperties.SourceControlId);

        statusCode.setValue(disqualifyStatusCodeToUpdate);
        formContext.data.save().then(null, function () {
            statusCode.setValue(prevStatusCodeValue);
        });
    };

    const setFlyoutButtons = function (disqualifyStatuCodes) {

        let buttons = [];

        disqualifyStatuCodes.forEach(function (disqualifyStatuCode) {

            let button = {};
            button.name = disqualifyStatuCode.label;
            button.id = disqualifyStatuCode.value;
            buttons.push(button);
        });

        return buttons;
    };

    const isDisqualifyLeadFlyoutEnabled = function (primaryControl) {

        formContext = primaryControl ? primaryControl : Xrm.Page;
        let isFormTypeUpdate = formContext.ui.getFormType() == Utils.CrmPage.FormType.Update;
        let isActive = formContext.getAttribute(formAttributes.statecode).getValue() == leadStateCode.Active;
        return isFormTypeUpdate && isActive;
    };

    const isAssignToMeEnabled = function (primaryControl) {

        formContext = primaryControl ? primaryControl : Xrm.Page;
        const isFormTypeUpdate = formContext.ui.getFormType() == Utils.CrmPage.FormType.Update;
        const isActive = formContext.getAttribute(formAttributes.statecode).getValue() == leadStateCode.Active;
        const owner = formContext.getAttribute(formAttributes.ownerid).getValue();
        const sentToIVR = formContext.getAttribute(formAttributes.alt_senttoivrbit).getValue();
        const ivrCampaign = formContext.getAttribute(formAttributes.alt_ivrcampaigncode).getValue();

        if (isFormTypeUpdate && isActive
            && owner[0].name === 'חבר בורסה כללי'
            && sentToIVR === true
            && ivrCampaign === ivrCampaignCodes.NewCallBacks) {
            return true;
        }

        return false;
    };

    const assignToMeOnClick = function (primaryControl) {

        formContext = primaryControl ? primaryControl : Xrm.Page;

        formContext.getAttribute(formAttributes.alt_assigntomebit).setValue(true);

        formContext.data.save().then(
            function () {
                //openPhoneCall(formContext);
                openPhoneCallQuickCreate(formContext);
            },
            function (error) {
                Xrm.Navigation.openErrorDialog({ message: error.message });
            }
        );
    };

    const openPhoneCall = function (formContext) {

        const leadId = Utils.JsExtantions.String.RemoveBraces(formContext.data.entity.getId());

        const pageInput = {
            pageType: "entityrecord",
            entityName: "phonecall",
            createFromEntity: {
                entityType: "lead",
                id: leadId
            }
        };

        const navigationOptions = {
            target: 2
        };

        Xrm.Navigation.navigateTo(pageInput, navigationOptions);
    };

    const openPhoneCallQuickCreate = function (formContext) {

        const leadId = Utils.JsExtantions.String.RemoveBraces(formContext.data.entity.getId());

        const entityFormOptions = {
            entityName: "phonecall",
            useQuickCreateForm: true,
            createFromEntity: {
                entityType: "lead",
                id: leadId
            }
        };

        const formParameters = {
            OpenedFromLeadButton: "true"
        };

        Xrm.Navigation.openForm(entityFormOptions, formParameters)
            .then(function (result) {
                if (result.savedEntityReference && result.savedEntityReference.length > 0) {
                    console.log(result.savedEntityReference[0].id);
                }
            })
            .catch(function (error) {
                console.error(error.message);
            });
    };

    return {
        StartManualProcess: startManualProcess,
        DisqualifyLeadOnClick: disqualifyLeadOnClick,
        IsDisqualifyLeadEnabled: isDisqualifyLeadEnabled,
        IsDisqualifyLeadFlyoutEnabled: isDisqualifyLeadFlyoutEnabled,
        PopulateLeadDisqualifyDynamicMenu: populateLeadDisqualifyDynamicMenu,
        DisqualifyLeadFlyoutOnClick: disqualifyLeadFlyoutOnClick,
        IsAssignToMeEnabled: isAssignToMeEnabled,
        AssignToMeOnClick: assignToMeOnClick
    };
}());
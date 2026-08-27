/// <reference path="../utils/Utils.Validators.js" />
/// <reference path="../utils/Utils.JsExtantions.js" />
/// <reference path="../utils/Utils.CrmPage.js" />

var TreatmentStatusMainBL = (function () {

    const formAttributes = {

        alt_entitytodisplaycode: 'alt_entitytodisplaycode',
        alt_leadstatuscode: 'alt_leadstatuscode'
    };

    const formTabs = {

        GeneralTab: 'GeneralTab'
    };

    const formSections = {

        LeadSettingsSection: 'LeadSettingsSection',
        OpportunitySettingsSection: 'OpportunitySettingsSection'
    };

    const entityToDisplayCode = {

        Lead: 1,
        Opportunity: 2
    };

    let formContext;

    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();
        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;

            switch (formType) {
                case crmFormTypes.Create: {

                    initOnChange();
                    break;
                }
                case crmFormTypes.Update: {

                    initOnChange();
                    break;
                }
                default:
                    break;
            }

            initFormUI();
        }
    };

    const initFormUI = function () {

        // not used
        /*handleSectionsVisibilityByEntityToDisplay();*/
    };

    const initOnChange = function () {

        // not used
        //formContext.getAttribute(formAttributes.alt_entitytodisplaycode).addOnChange(entityToDisplayCodeOnChange);
    };

    const entityToDisplayCodeOnChange = function () {

        handleSectionsVisibilityByEntityToDisplay();
    };

    const handleSectionsVisibilityByEntityToDisplay = function () {

        let entityToDisplay = formContext.getAttribute(formAttributes.alt_entitytodisplaycode).getValue();
        let isLeadVisible = entityToDisplay == entityToDisplayCode.Lead;
        let isOpportunityVisible = entityToDisplay == entityToDisplayCode.Opportunity;

        Utils.CrmPage.SetSectionVisibleMode(formContext, formTabs.GeneralTab, formSections.LeadSettingsSection, isLeadVisible);
        Utils.CrmPage.SetSectionVisibleMode(formContext, formTabs.GeneralTab, formSections.OpportunitySettingsSection, isOpportunityVisible);

        if (isLeadVisible) {

            setSectionAttributesRequirement(formTabs.GeneralTab, formSections.LeadSettingsSection, true);
        }
    };

    const setSectionAttributesRequirement = function (tabName, sectionName, isRequired) {

        let requirementLevel = isRequired == true ? Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.None;
        let section = Xrm.Page.ui.tabs.get(tabName).sections.get(sectionName);
        let controls = section?.controls?.getAll();

        if (controls) {

            controls.forEach(function (control) {

                Utils.CrmPage.SetAttributeRequiredLevel(formContext, control.getName(), requirementLevel);
            });
        }
    };

    return {
        OnLoad: onLoad
    };
})();
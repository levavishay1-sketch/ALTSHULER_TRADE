/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.JsExtantions.js" />
/// <reference path="../utils/Utils.Server.js" />
/// <reference path="../forms/IncidentTradeCommon.js" />

var IncidentTradeMain = (function () {

    let formContext;
    let formAttributes;
    const bpfSectionName = 'BpfSection';
    const operationalProcessDetailsSectionName = 'OperationalProcessDetailsSection';
    const dynamicFormConfigurationSectionName = 'DynamicFormConfigurationSection';
    const preservationSectionName = 'PreservationSection'
    const generalTabName = 'GeneralTab';
    const operationalProcessDetailsTabName = 'OperationalProcessDetailsTab';

    const preservationStatusReasonValues = {
        Preserved: 1,
        PartiallyPreserved: 2,
        NotPreserved: 3,
        PreservationNotDone: 4
    };

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();
        initFormAttributes();
        if (Utils.CrmPage.IsFirstLoad()) {

            IncidentTradeCommonBL.OnLoad(executionContext);
            initFormAttributes();

            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;

            switch (formType) {
                case crmFormTypes.Create: {
                    initOnChange();
                    enableAttributesOnCreate();
                    break;
                }
                case crmFormTypes.Update: {
                    initUI();
                    initOnChange();
                    break;
                }
                case crmFormTypes.Disable: {
                    initUI();
                    break;
                }
                default:
                    break;
            }
        }
        else {
            reLoad();
        }
    };

    const reLoad = function () {
        Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.ownerid, false);
        disableAttributes();
        initUI();
    };

    const initFormAttributes = function () {

        formAttributes = IncidentTradeCommonBL.formAttributes;
        formAttributes.alt_bpfstagesjson = 'alt_bpfstagesjson';
        formAttributes.alt_operationalprocessid = 'alt_operationalprocessid';
        formAttributes.alt_preservationstatuscode = 'alt_preservationstatuscode';
        formAttributes.alt_preservationstatusreasoncode = 'alt_preservationstatusreasoncode';
    };

    const initOnChange = function () {

        formContext.getAttribute(formAttributes.alt_bpfstagesjson).addOnChange(bpfStagesJsonOnChange);
        //formContext.getAttribute(formAttributes.alt_subject2id).addOnChange(handlePreservationSectionVisibility);
        formContext.getAttribute(formAttributes.alt_preservationstatuscode).addOnChange(handleUIByPreservationStatusReason);
    };

    const initUI = function () {

        displayAttributesWithValue();
        handleChildCasesVisibility();
        handleBpfSectionVisibility();
        handleOperationalProcessTabVisibility();
        handlePreservationSectionVisibility();
    }

    const bpfStagesJsonOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        handleBpfSectionVisibility();
    };

    const handleChildCasesVisibility = function () {

        const isBulkIncidentsParent = formContext.getAttribute(formAttributes.alt_bulkincidentsparentbit).getValue();
        if (isBulkIncidentsParent) {
            showGeneralTabChildCasesSection();
        }
        else {
            showChildCasesTab();
        }
    };

    const showChildCasesTab = function () {
        getChildCases(function (result) {
            if (result && result.length > 0) {
                Utils.CrmPage.SetTabVisibilityMode(formContext, 'ChildCasesTab', true);
            }
        });
    };

    const showGeneralTabChildCasesSection = function () {

        Utils.CrmPage.SetSectionVisibleMode(formContext, 'GeneralTab', 'ChildCasesSection', true);
    };

    const displayAttributesWithValue = function () {

        const attributesWithValueToDisplay = [
            formAttributes.parentcaseid
        ];

        Utils.CrmPage.DisplayAttributesWithValue(formContext, attributesWithValueToDisplay);
    };

    const enableAttributesOnCreate = function () {

        const attributes = [
            formAttributes.alt_subject1id,
            formAttributes.caseorigincode,
            formAttributes.alt_bulkincidentsparentbit
        ];

        Utils.CrmPage.DisableAttributes(formContext, attributes, false);
    };

    const disableAttributes = function () {

        const attributes = [
            formAttributes.alt_subject1id,
            formAttributes.caseorigincode,
            formAttributes.customerid,
            formAttributes.alt_bulkincidentsparentbit,
            formAttributes.alt_subject2id,
            formAttributes.alt_incidentstatusid,
            formAttributes.alt_portfolioid
        ];
        Utils.CrmPage.DisableAttributes(formContext, attributes, true);
    };

    const getChildCases = function (successCallback) {

        const entityId = Utils.JsExtantions.String.RemoveBraces(formContext.data.entity.getId());
        const filter = '_parentcaseid_value eq ' + entityId;

        Utils.Server.RetrieveMultiple("incident", "title", filter, null, null, function (result) {
            if (successCallback) {
                successCallback(result)
            };
        });
    };

    const handleBpfSectionVisibility = function () {

        const isSectionVisible = formContext.getAttribute(formAttributes.alt_bpfstagesjson).getValue() ? true : false;
        Utils.CrmPage.SetSectionVisibleMode(formContext, generalTabName, bpfSectionName, isSectionVisible);
    };


    const handleOperationalProcessTabVisibility = function () {

        const isTabVisible = formContext.getAttribute(formAttributes.alt_operationalprocessid).getValue() ? true : false;
        Utils.CrmPage.SetTabVisibilityMode(formContext, operationalProcessDetailsTabName, isTabVisible);
    };

    const handlePreservationSectionVisibility = function () {

        const subject2 = formContext.getAttribute(formAttributes.alt_subject2id).getValue();
        if (subject2) {
            const subject2Id = Utils.JsExtantions.String.RemoveBraces(subject2[0].id);
            Xrm.WebApi.retrieveRecord("alt_subject2", subject2Id, "?$select=alt_codeint").then(
                function (result) {
                    const code = result.alt_codeint;
                    Utils.Global.GetGlobalParamValue("Subject2CodesForWithdrawals",
                        (result) => {
                            const withdrawalCodes = result.split(",").map(Number);
                            Utils.CrmPage.SetSectionVisibleMode(formContext, generalTabName, preservationSectionName, withdrawalCodes.includes(code));
                        },
                        (error) => {
                            console.log(error);
                        }
                    )
                },
                function (error) {
                    console.log(error);
                }
            );
        }
    };

    const handleUIByPreservationStatusReason = function () {

        formContext.getAttribute(formAttributes.alt_preservationstatusreasoncode).setValue(null);
        const preservationStatusReasonValue = formContext.getAttribute(formAttributes.alt_preservationstatuscode).getValue();
        if (preservationStatusReasonValue === preservationStatusReasonValues.PartiallyPreserved
            || preservationStatusReasonValue === preservationStatusReasonValues.NotPreserved) {
            Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_preservationstatusreasoncode, true);
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_preservationstatusreasoncode, Utils.CrmPage.RequirementLevel.Required);
        }
        else {
            Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_preservationstatusreasoncode, false);
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_preservationstatusreasoncode, Utils.CrmPage.RequirementLevel.None);
        }
    };

    return {
        OnLoad: onLoad
    };
})();
/// <reference path="../utils/Utils.Validators.js" />
/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.JsExtantions.js" />
/// <reference path="../utils/Utils.Server.js" />

var RepresentativeRewardMain = (function () {

    const formAttributes = {
        alt_manualcreateexplanation: 'alt_manualcreateexplanation',
        alt_creationmethodcode: 'alt_creationmethodcode',
        alt_representativerewardtypecode: 'alt_representativerewardtypecode',
        alt_relatedrecordid: 'alt_relatedrecordid',
        alt_treatmentstatusid: 'alt_treatmentstatusid',
        alt_joiningprocessnumber: 'alt_joiningprocessnumber',
        alt_portfolioid: 'alt_portfolioid',
        alt_representativerewardsystemuserid: 'alt_representativerewardsystemuserid'
    };

    const creationMethodCode = {
        Manual: 1
    };

    const representativeRewardTypeCode = {
        SalesProcess: 1,
        EncouragingDeposit: 2
    };

    let formContext;

    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();
        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;

            switch (formType) {
                case crmFormTypes.Create: {

                    initDefaultValues();
                    initFormUI();
                    initOnChange();
                    break;
                }
                case crmFormTypes.Update: {
                    initFormUI();
                    break;
                }
                default:
                    break;
            }
        }
        handleDisableRepresentativeRewardSystemUserId();
    };

    const initOnChange = function () {

        formContext.getAttribute(formAttributes.alt_relatedrecordid).addOnChange(relatedRecordIdOnChange);
    };

    const initFormUI = function () {

        handleDisableRepresentativeRewardSystemUserId();
        handleManualCreateExplanation();
    };

    const relatedRecordIdOnChange = function () {

        handleRelatedRecord();
    };

    const initDefaultValues = function () {

        formContext.getAttribute(formAttributes.alt_creationmethodcode).setValue(creationMethodCode.Manual);
        handleRelatedRecord();
    };

    const handleRelatedRecord = function () {

        let relatedRecord = formContext.getAttribute(formAttributes.alt_relatedrecordid).getValue();
        if (relatedRecord) {

            let relatedRecordLogicalName = relatedRecord[0].entityType;
            let selectColumns = {
                alt_digitalformverification: "alt_digitalformnumber, _alt_portfolioid_value",
                lead: "_alt_treatmentstatusid_value, alt_leadidentitynumber",
                opportunity: "_alt_treatmentstatusid_value, alt_opportunityidentitynumber"
            };
            
            setRepresentativeRewardTypeCodeByRelatedRecord(relatedRecordLogicalName);
            setColumnsByRelatedRecord(relatedRecord[0], selectColumns[relatedRecordLogicalName]);
        }
    };

    const setRepresentativeRewardTypeCodeByRelatedRecord = function (relatedRecordLogicalName) {

        let repRewardTypeCodeToSet = relatedRecordLogicalName == "alt_digitalformverification" ?
            representativeRewardTypeCode.EncouragingDeposit
            : representativeRewardTypeCode.SalesProcess;

        formContext.getAttribute(formAttributes.alt_representativerewardtypecode).setValue(repRewardTypeCodeToSet);
    };

    const setColumnsByRelatedRecord = function (relatedRecord, selectColumns) {

        let relatedRecordLogicalName = relatedRecord.entityType;
        Utils.Server.Retrieve(relatedRecordLogicalName, relatedRecord.id, selectColumns, null, function (retrievedRelatedRecord) {

            if (retrievedRelatedRecord) {

                if (relatedRecordLogicalName != "alt_digitalformverification") {

                    let treatmentStatus = retrievedRelatedRecord.alt_treatmentstatusid;
                    Utils.CrmPage.SetLookup(formContext, formAttributes.alt_treatmentstatusid,
                        treatmentStatus.Id,
                        treatmentStatus.Name,
                        treatmentStatus.LogicalName);
                } else {

                    let portfolio = retrievedRelatedRecord.alt_portfolioid;
                    Utils.CrmPage.SetLookup(formContext, formAttributes.alt_portfolioid,
                        portfolio.Id,
                        portfolio.Name,
                        portfolio.LogicalName);

                    formContext.getAttribute(formAttributes.alt_joiningprocessnumber).setValue(retrievedRelatedRecord.alt_digitalformnumber);
                }

                if (relatedRecordLogicalName == "lead") {

                    formContext.getAttribute(formAttributes.alt_joiningprocessnumber).setValue(retrievedRelatedRecord.alt_leadidentitynumber);
                } else if (relatedRecordLogicalName == "opportunity") {

                    formContext.getAttribute(formAttributes.alt_joiningprocessnumber).setValue(retrievedRelatedRecord.alt_opportunityidentitynumber);
                }
            }
        });
    };

    const handleManualCreateExplanation = function () {

        let creationMethod = formContext.getAttribute(formAttributes.alt_creationmethodcode).getValue();
        let isManualCreation = creationMethod == creationMethodCode.Manual ? true : false;
        let requiredLevel = isManualCreation ? Utils.CrmPage.RequirementLevel.Required
                                             : Utils.CrmPage.RequirementLevel.None;

        Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_manualcreateexplanation, isManualCreation);
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_manualcreateexplanation, requiredLevel);
    };

    const handleDisableRepresentativeRewardSystemUserId = function () {

        let currentFormType = formContext.ui.getFormType();
        let isNotCreate = currentFormType != Utils.CrmPage.FormType.Create
        Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.alt_representativerewardsystemuserid, isNotCreate);

    };

    return {
        OnLoad: onLoad
    };
})();
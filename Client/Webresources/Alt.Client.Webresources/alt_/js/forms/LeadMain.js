/// <reference path="../utils/Utils.Enums.js" />
/// <reference path="../utils/Utils.JsExtantions.js" />
/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Server.js" />
/// <reference path="../utils/Utils.Global.js" />

var LeadMain = (function () {

    const DUPLICATE_LEAD_WITH_SAME_MOBILE_WARNING = "קיימת הפניה מספר {0} עם נייד זהה.";
    const DUPLICATE_OPPORTUNITY_WITH_SAME_MOBILE_WARNING = "קיימת הזדמנות מספר {0} עם נייד זהה";
    const CANT_QUALIFY_WITHOUT_CUSTOMER_IDENTIFIER = "לא ניתן לאשר הפניה ללא מספר מזהה לקוח.";

    const attributsWithValueToDisplay = ['alt_digitalformlink', 'qualifyingopportunityid'];
    const formAttributes = {
        mobilephone: 'mobilephone',
        alt_identitynumber: 'alt_identitynumber',
        emailaddress1: 'emailaddress1',
        telephone1: 'telephone1',
        alt_identitytypecode: 'alt_identitytypecode',
        companyname: 'companyname',
        leadsourcecode: 'leadsourcecode',
        alt_digitalformlink: 'alt_digitalformlink',
        alt_leadidentitynumber: 'alt_leadidentitynumber',
        alt_marketingsource: 'alt_marketingsource',
        alt_referralsourceid: 'alt_referralsourceid',
        alt_treatmentstatusid: 'alt_treatmentstatusid',
    };

    const defaultViewId = '{00000000-0000-0000-0000-000000000001}';
    const customViewDisplayName = 'סטטוסי טיפול';

    const identityTypeCode =
    {
        ID: 100000000,
        CompanyNumber: 100000001
    };

    var leadSourceCode = {
        Else: 1,
        MarketingSite: 2,
        Management: 3,
        DigitalForm: 4,
        StockExchangeEmployee: 5,
        CompanyEmployee: 6,
        MiniSite: 7,
        Agents: 8,
        Advertising: 9,
        Internet: 10,
        MarketingHomeWebsite: 15,
        Mivtza1: 12
    };

    const entityToDisplayCode = {
        Lead: 1,
        Opportunity: 2
    };

    const leadSourceOptionsToRemove =
        [
            leadSourceCode.DigitalForm,
            leadSourceCode.MarketingSite,
            leadSourceCode.Management,
            leadSourceCode.MiniSite,
            leadSourceCode.Advertising,
            leadSourceCode.Internet,
            leadSourceCode.MarketingHomeWebsite,
            leadSourceCode.Mivtza1
        ];

    let formContext;

    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();

        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;

            switch (formType) {
                case crmFormTypes.Create:
                    {
                        initOnChange();
                        initFormUI();
                        break;
                    }
                case crmFormTypes.Update: {
                    initOnChange();
                    initFormUI();
                    initFormUIUpdate();
                    addToOnPostSave();
                    showDuplicatesByMobilePhone();
                    validate();
                    break;
                }
                default: {
                    break;
                }
            }
            Utils.CrmPage.DisplayAttributesWithValue(formContext, attributsWithValueToDisplay);
            addDigitalFormUrlQueryStringParameters();
            handlePCFDuplicatesSectionVisibility();
        }
    };

    const onSave = function (executionContext) {
        formContext = executionContext.getFormContext();
        const eventArgs = executionContext.getEventArgs();

        if (formContext.ui.getFormType() === Utils.CrmPage.FormType.Update) {
            const saveMode = eventArgs.getSaveMode();
            if (saveMode === Utils.CrmPage.SaveModes.Qualify && !isValidCustomer()) {
                eventArgs.preventDefault();
            }
        }

        handleMobilePhoneLock(true);
    };

    const initOnChange = function () {
        formContext.getAttribute(formAttributes.mobilephone).addOnChange(mobilePhoneOnChange);
        formContext.getAttribute(formAttributes.alt_identitynumber).addOnChange(identityNumberOnChange);
        formContext.getAttribute(formAttributes.emailaddress1).addOnChange(emailAddressOnChange);
        formContext.getAttribute(formAttributes.telephone1).addOnChange(telephoneOnChange);
        formContext.getAttribute(formAttributes.alt_identitytypecode).addOnChange(identityTypeOnChange);
        formContext.getAttribute(formAttributes.alt_marketingsource).addOnChange(marketingSourceOnChange);
        //formContext.getControl(formAttributes.alt_treatmentstatusid).addPreSearch(setTreatmentStatusPreSearch);
    };

    const addToOnPostSave = function () {

        formContext.data.entity.addOnPostSave(addDigitalFormUrlQueryStringParameters);
    }

    const initFormUI = function () {

        handleUIByIdentityType();
        removeLeadSourceOptions();
        disableReferralSourceIdByMarketingSource();
        setTreatmentStatusCustomView();
    };

    const initFormUIUpdate = function () {
        handleMobilePhoneLock(true);
    };

    const emailAddressOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        Utils.CrmPage.HandleEmailAddressAttributeChange(executionContext);
    };

    const telephoneOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        Utils.CrmPage.HandleTelephoneAttributeChange(executionContext);
    };

    const identityTypeOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        formContext.getAttribute(formAttributes.alt_identitynumber).setValue(null);
        let identityNumberControl = formContext.getControl(formAttributes.alt_identitynumber);
        identityNumberControl.clearNotification();

        handleUIByIdentityType();
    };

    const mobilePhoneOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        Utils.CrmPage.HandleMobilePhoneAttributeChange(executionContext);
    };

    const identityNumberOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        let identityType = formContext.getAttribute(formAttributes.alt_identitytypecode).getValue();

        if (identityType == identityTypeCode.ID) {
            Utils.CrmPage.HandleGovIdAttributeChange(executionContext);
        } else if (identityType == identityTypeCode.CompanyNumber) {
            Utils.CrmPage.HandleAccountNumberAttributeChange(executionContext);
        }
    };

    const marketingSourceOnChange = function () {
        disableReferralSourceIdByMarketingSource();
    };

    const validate = function () {
        Utils.CrmPage.HandleEmailAddressAttribute(formContext, formAttributes.emailaddress1);
        Utils.CrmPage.HandleMobilePhoneAttribute(formContext, formAttributes.mobilephone);
        Utils.CrmPage.HandleTelephoneAttribute(formContext, formAttributes.telephone1);
        validateIdentityNumber();
    };

    const validateIdentityNumber = function () {
        let identityType = formContext.getAttribute(formAttributes.alt_identitytypecode).getValue();
        switch (identityType) {
            case identityTypeCode.ID: {
                Utils.CrmPage.HandleGovIdAttribute(formContext, formAttributes.alt_identitynumber);
                break;
            }
            case identityTypeCode.CompanyNumber: {
                Utils.CrmPage.HandleAccountNumberAttribute(formContext, formAttributes.alt_identitynumber);
                break;
            }
            default:
                break;
        }
    };

    const handleMobilePhoneLock = function (isLocked) {

        Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.mobilephone, isLocked);
    };

    const handleUIByIdentityType = function () {

        let identityType = formContext.getAttribute(formAttributes.alt_identitytypecode).getValue();
        let companyNameRequired = identityType == identityTypeCode.CompanyNumber ?
            Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.Recommended;
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.companyname, companyNameRequired);
    };

    const removeLeadSourceOptions = function () {

        let leadSourceValue = formContext.getAttribute(formAttributes.leadsourcecode).getValue();

        leadSourceOptionsToRemove.forEach(function (value) {
            if (leadSourceValue != value) {
                formContext.getControl(formAttributes.leadsourcecode).removeOption(value);
            }
            else {
                Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.leadsourcecode, true);
            }
        });
    };

    const showDuplicatesByMobilePhone = function () {
        formContext.ui.clearFormNotification();
        showLeadDuplicates();
        showOpportunityDuplicates();
    };

    const showLeadDuplicates = function () {
        let matchingEntityName = 'lead';
        retrieveDuplicatesByMobilePhone(matchingEntityName);
    };

    const showOpportunityDuplicates = function () {
        let matchingEntityName = 'opportunity';
        retrieveDuplicatesByMobilePhone(matchingEntityName);
    };

    const retrieveDuplicatesByMobilePhone = function (matchingEntityName) {

        let mobilePhone = formContext.getAttribute(formAttributes.mobilephone).getValue();
        var record = {
            "@odata.type": "Microsoft.Dynamics.CRM.lead",
            "mobilephone": mobilePhone
        };
        Utils.Server.RetrieveDuplicates(record, matchingEntityName, showFormNotification);
    };

    const showFormNotification = function (result, matchingEntityName) {

        if (result && result.length) {

            var entityId = Utils.JsExtantions.String.RemoveBraces(formContext.data.entity.getId());
            result.forEach(function (record) {
                let recordId = record[matchingEntityName + "id"];
                if (recordId.toLowerCase() != entityId.toLowerCase()) {

                    let message = matchingEntityName == entityName.Lead ?
                        DUPLICATE_LEAD_WITH_SAME_MOBILE_WARNING : DUPLICATE_OPPORTUNITY_WITH_SAME_MOBILE_WARNING;
                    let identityNumberAttributeName = matchingEntityName == entityName.Lead ?
                        formAttributes.alt_leadidentitynumber : 'alt_opportunityidentitynumber';
                    message = Utils.JsExtantions.String.Format(message, record[identityNumberAttributeName]);
                    formContext.ui.setFormNotification(message, notificationLevel.Warning, recordId);
                }
            });
        }
    };

    const isValidCustomer = function () {

        let isValid = true;
        const customerIdentityNumber = formContext.getAttribute('alt_identitynumber').getValue();
        if (!customerIdentityNumber) {
            isValid = false;
            Xrm.Navigation.openAlertDialog({ text: CANT_QUALIFY_WITHOUT_CUSTOMER_IDENTIFIER });
        }
        return isValid;
    };

    const addDigitalFormUrlQueryStringParameters = function () {

        Utils.Global.AddQueryStringParamsToJoiningFormURL(formContext, formAttributes.alt_digitalformlink);
    };

    const disableReferralSourceIdByMarketingSource = function () {

        let isMarketingSourceEmpty = formContext.getAttribute(formAttributes.alt_marketingsource).getValue() ? false : true;
        Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.alt_referralsourceid, !isMarketingSourceEmpty);
    };

    const handlePCFDuplicatesSectionVisibility = function () {

        let isVisible = formContext.ui.getFormType() != Utils.CrmPage.FormType.Create ? true : false;
        Utils.CrmPage.SetSectionVisibleMode(formContext, "Summary", "PCFDuplicatesSection", isVisible);
    };

    const setTreatmentStatusCustomView = function () {

        const fetchXML = createTreatmentStatusLookupFetchXml();
        setTreatmentStatusLookupCustomView(fetchXML);
    };

    const createTreatmentStatusLookupFetchXml = function () {

        const fetchxml =
            "<fetch version='1.0' mapping='logical'>" +
            "<entity name='alt_treatmentstatus'>" +
            "<attribute name='alt_name' />" +
            "<attribute name='alt_codeint' />" +
            "<attribute name='alt_treatmentstatusid' />" +
            "<order attribute='alt_name' descending='false' />" +
            "<filter type='and'>" +
            "<condition attribute='statecode' operator='eq' value='" + customEntityStateCode.Active + "' />" +
            "<condition attribute='alt_userdisplaybit' operator='eq' value='1' />" +
            "</filter>" +
            "</entity>" +
            "</fetch>";

        return fetchxml;
    }

    const setTreatmentStatusLookupCustomView = function (fetchXml) {

        const treatmentStatusIdControl = formContext.getControl(formAttributes.alt_treatmentstatusid);
        const layoutXml = '<grid name="resultset" jump="alt_name" select="1" preview="1" icon="1"><row name="result" id="alt_treatmentstatusid">' +
            '<cell name="alt_name" width="100"/>' +
            '<cell name="alt_codeint" width="100"/>' +
            '</row></grid>';

        treatmentStatusIdControl.addCustomView(defaultViewId, "alt_treatmentstatus", customViewDisplayName, fetchXml, layoutXml, true);
    };

    return {
        OnLoad: onLoad,
        OnSave: onSave
    };
})();
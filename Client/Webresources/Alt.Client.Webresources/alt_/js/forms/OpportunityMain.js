var OpportunityMain = (function () {

    const formAttributes = {
        mobilephone: 'alt_mobilephone',
        alt_digitalformlink: 'alt_digitalformlink',
        currentsituation: 'currentsituation',
        alt_telephone1: 'alt_telephone1',
        emailaddress: 'emailaddress',
        alt_opportunityidentitynumber: 'alt_opportunityidentitynumber',
        originatingleadid: 'originatingleadid',
        alt_treatmentstatusid: 'alt_treatmentstatusid'
    };

    const attributsWithValueToDisplay = [
        formAttributes.alt_digitalformlink,
        formAttributes.currentsituation
    ];

    const entityToDisplayCode = {
        Lead: 1,
        Opportunity: 2
    };

    const defaultViewId = '{00000000-0000-0000-0000-000000000001}';
    const customViewDisplayName = 'סטטוסי טיפול';

    let formContext;

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();

        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;

            switch (formType) {
                case crmFormTypes.Update: {
                    initOnChange();
                    showDuplicatesByMobilePhone();
                    validate();
                    addToOnPostSave();
                    break;
                }
                default: {
                    break;
                }
            }
            Utils.CrmPage.DisplayAttributesWithValue(formContext, attributsWithValueToDisplay);
            addDigitalFormUrlQueryStringParameters();
            handlePCFDuplicatesSectionVisibility();
            setTreatmentStatusCustomView();
        }
    };

    const initOnChange = function () {
        formContext.getAttribute(formAttributes.mobilephone).addOnChange(mobilePhoneOnChange);
        formContext.getAttribute(formAttributes.alt_telephone1).addOnChange(telephone1OnChange);
        formContext.getAttribute(formAttributes.emailaddress).addOnChange(emailAddressOnChange);
    };

    const addToOnPostSave = function () {

        formContext.data.entity.addOnPostSave(addDigitalFormUrlQueryStringParameters);
    }

    const mobilePhoneOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        Utils.CrmPage.HandleMobilePhoneAttribute(formContext, formAttributes.mobilephone);
    };

    const telephone1OnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        Utils.CrmPage.HandleTelephoneAttribute(formContext, formAttributes.alt_telephone1);
    };

    const emailAddressOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        Utils.CrmPage.HandleEmailAddressAttribute(formContext, formAttributes.emailaddress);
    };

    const validate = function () {
        Utils.CrmPage.HandleMobilePhoneAttribute(formContext, formAttributes.mobilephone);
        Utils.CrmPage.HandleEmailAddressAttribute(formContext, formAttributes.emailaddress);
        Utils.CrmPage.HandleTelephoneAttribute(formContext, formAttributes.alt_telephone1);
    };

    const showDuplicatesByMobilePhone = function () {
        formContext.ui.clearFormNotification();
        showOpportunityDuplicates();
    };

    const showOpportunityDuplicates = function () {
        retrieveDuplicatesByMobilePhone('opportunity');
    };

    const retrieveDuplicatesByMobilePhone = function (matchingEntityName) {

        let mobilePhone = formContext.getAttribute(formAttributes.mobilephone).getValue();
        var record = {
            "@odata.type": "Microsoft.Dynamics.CRM.opportunity",
            "alt_mobilephone": mobilePhone
        };
        Utils.Server.RetrieveDuplicates(record, matchingEntityName, showFormNotification);
    };

    const showFormNotification = function (result, matchingEntityName) {

        if (result && result.length) {

            var entityId = Utils.JsExtantions.String.RemoveBraces(formContext.data.entity.getId());
            result.forEach(function (record) {
                let recordId = record[matchingEntityName + "id"];
                if (recordId.toLowerCase() != entityId.toLowerCase()) {

                    let message = "קיימת הזדמנות מספר {0} עם נייד זהה";
                    message = Utils.JsExtantions.String.Format(message, record.alt_opportunityidentitynumber);
                    formContext.ui.setFormNotification(message, notificationLevel.Warning, recordId);
                }
            });
        }
    };

    const addDigitalFormUrlQueryStringParameters = function () {

        Utils.Global.AddQueryStringParamsToJoiningFormURL(formContext, formAttributes.alt_digitalformlink);
    };

    const handlePCFDuplicatesSectionVisibility = function () {

        let isVisible = formContext.ui.getFormType() != Utils.CrmPage.FormType.Create ? true : false;
        Utils.CrmPage.SetSectionVisibleMode(formContext, "Summary", "PCFDuplicatesSection", isVisible);
    };

    const setTreatmentStatusCustomView = function () {
        createTreatmentStatusLookupFetchXml();
    };

    const createTreatmentStatusLookupFetchXml = function () {

        const fetchXml =
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

        setTreatmentStatusLookupCustomView(fetchXml);
    };

    const setTreatmentStatusLookupCustomView = function (fetchXml) {

        const treatmentStatusIdControl = formContext.getControl(formAttributes.alt_treatmentstatusid);
        const layoutXml = '<grid name="resultset" jump="alt_name" select="1" preview="1" icon="1"><row name="result" id="alt_treatmentstatusid">' +
            '<cell name="alt_name" width="100"/>' +
            '<cell name="alt_codeint" width="100"/>' +
            '</row></grid>';

        treatmentStatusIdControl.addCustomView(defaultViewId, "alt_treatmentstatus", customViewDisplayName, fetchXml, layoutXml, true);
    };

    return {
        OnLoad: onLoad
    };
})();

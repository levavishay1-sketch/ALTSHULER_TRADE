var AppointmentMain = (function () {

    let formContext;
    let accountHolders = [];
    const formAttributes = {
        alt_activitysubjectid: 'alt_activitysubjectid',
        regardingobjectid: 'regardingobjectid',
        requiredattendees: 'requiredattendees',
        optionalattendees: 'optionalattendees',
        alt_sendsmsbit: 'alt_sendsmsbit',
        alt_sendemailbit: 'alt_sendemailbit',
        location: 'location',
        description: 'description'
    };

    const activityTypeCode = {
        Appointment: 1
    };

    const attributesToDisable = [
        formAttributes.alt_activitysubjectid,
        formAttributes.alt_sendemailbit,
        formAttributes.alt_sendsmsbit
    ];

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();

        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;

            switch (formType) {
                case crmFormTypes.Create: {

                    initFormUI();
                    initOnChange();
                    break;
                }
                case crmFormTypes.Update: {
                    initFormUI();
                    Utils.CrmPage.DisableAttributes(formContext, attributesToDisable, true);
                    break;
                }
                default:
                    break;
            }
        }
        else {
            reload();
        }
    };

    const reload = function () {

        Utils.CrmPage.DisableAttributes(formContext, attributesToDisable, true);
    };

    const initFormUI = function () {

        handleUIByRegardingObject();
        setActivitySubjectCustomView();
        getActivitySubject(false, handleUIByActivitySubject);
        formContext.getControl(formAttributes.requiredattendees).addPreSearch(setRequiredAttendeesCustomerFilter);
    };

    const initOnChange = function () {

        formContext.getAttribute(formAttributes.alt_activitysubjectid).addOnChange(activitySubjectOnChanged);
    };

    const activitySubjectOnChanged = function (executionContext) {

        formContext = executionContext.getFormContext();
        getActivitySubject(true);
    };

    const handleMailingAttributesUIByActivitySubject = function (activitySubject, isSetDefaultValue) {

        let isShowSendEmail = activitySubject && activitySubject.alt_emailtemplateid
            && activitySubject.alt_emailtemplateid.Id ? true : false;
        let isShowSendSms = activitySubject && activitySubject.alt_smstemplateid
            && activitySubject.alt_smstemplateid.Id ? true : false;

        Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_sendemailbit, isShowSendEmail);
        Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_sendsmsbit, isShowSendSms);
        if (isSetDefaultValue) {
            if (formContext.getAttribute(formAttributes.alt_sendemailbit).getValue() != isShowSendEmail) {

                formContext.getAttribute(formAttributes.alt_sendemailbit).setValue(isShowSendEmail);
            }
            if (formContext.getAttribute(formAttributes.alt_sendsmsbit).getValue() != isShowSendSms) {

                formContext.getAttribute(formAttributes.alt_sendsmsbit).setValue(isShowSendSms);
            }
        }
    };

    const handleUIByRegardingObject = function () {

        const regardingObject = formContext.getAttribute(formAttributes.regardingobjectid).getValue();
        if (regardingObject && regardingObject[0]) {

            switch (regardingObject[0].entityType) {
                case entityName.Lead: {

                    Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.requiredattendees, true);
                    break;
                }
                case entityName.Opportunity: {

                    handleRequiredAttendeesByOpportunityRegardingObject(regardingObject[0]);
                    Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.requiredattendees, true);
                    break;
                }
                case entityName.DigitalFormVerification:
                case entityName.Portfolio: {

                    getAccountHoldersByRegardingObject(regardingObject[0]);
                    formContext.getControl(formAttributes.requiredattendees).setEntityTypes([entityName.Contact, entityName.Account]);
                    break;
                }
                default:
                    break;
            }
        }
    };

    const handleRequiredAttendeesByOpportunityRegardingObject = function (opportunity) {

        const select = '_customerid_value';
        var opportunityId = Utils.JsExtantions.String.RemoveBraces(opportunity.id);
        Utils.Server.Retrieve(opportunity.entityType, opportunityId, select, null,
            function (result) {
                const customer = result.customerid;
                if (customer) {
                    Utils.CrmPage.SetLookup(formContext, formAttributes.requiredattendees, customer.Id, customer.Name, customer.LogicalName);
                }
            }, null);
    };

    const handleUIByActivitySubject = function (activitySubject) {

        if (activitySubject) {
            const activitySubjectCode = activitySubject.alt_codeint;
            Utils.Global.GetGlobalParamValue('AppointmentUISettingsByActivitySubjectId', function (globalParam) {
                if (globalParam) {
                    const parsedJson = JSON.parse(globalParam);
                    const settingsByCode = parsedJson && parsedJson.codes[activitySubjectCode];
                    if (settingsByCode && settingsByCode.controls) {

                        handleUIBySettings(settingsByCode.controls);
                    }
                    else {
                        handleUIBySettings([]);
                    }
                } else {
                    Xrm.Navigation.openAlertDialog({ text: Utils.CrmPage.CommonRequestFailedMessage });
                }
            }, null);
        }
        else {
            handleUIBySettings([]);
        }
    };

    const handleUIBySettings = function (controls) {

        handleLocationUI(controls);
        handleDescriptionUI(controls);
    };

    const handleLocationUI = function (controls) {

        const locationSettings = getControlByName(controls, formAttributes.location);
        const locationRequirementLevel = locationSettings && locationSettings.required ?
            locationSettings.required : Utils.CrmPage.RequirementLevel.None;
        const locationVisibleMode = locationSettings && locationSettings.visible ?
            locationSettings.visible : false;
        Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.location, locationVisibleMode);
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.location, locationRequirementLevel);
    };

    const handleDescriptionUI = function (controls) {

        const descriptionSettings = getControlByName(controls, formAttributes.description);
        const descriptionRequirementLevel = descriptionSettings && descriptionSettings.required ?
            descriptionSettings.required : Utils.CrmPage.RequirementLevel.None;
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.description, descriptionRequirementLevel);
    };

    const getActivitySubject = function (isSetDefaultValue) {

        const activitySubjectValue = formContext.getAttribute(formAttributes.alt_activitysubjectid).getValue();

        if (activitySubjectValue) {
            const select = 'alt_codeint, _alt_emailtemplateid_value, _alt_smstemplateid_value';
            var activitySubjectId = Utils.JsExtantions.String.RemoveBraces(activitySubjectValue[0].id);
            Utils.Server.Retrieve('alt_activitysubject', activitySubjectId, select, null,
                function (result) {
                    handleMailingAttributesUIByActivitySubject(result, isSetDefaultValue);
                    handleUIByActivitySubject(result);

                }, function (error) {
                    Xrm.Navigation.openAlertDialog({ text: INTERNAL_SERVER_ERROR });
                });
        }
        else {
            handleMailingAttributesUIByActivitySubject(null, isSetDefaultValue);
            handleUIByActivitySubject(null);
        }
    };

    const getControlByName = function (controls, controlName) {

        const filteredControls = controls.filter(function (value) {
            return value.name == controlName;
        });

        return filteredControls && filteredControls[0] ? filteredControls[0] : null;
    };

    const getAccountHoldersByRegardingObject = function (regardingObjectId) {

        Utils.Global.GetActiveAccountHoldersByRelatedEntity(regardingObjectId, function (result) {
            accountHolders = result;
        });
    };

    const setRequiredAttendeesCustomerFilter = function () {

        formContext.getControl(formAttributes.requiredattendees).addCustomFilter(Utils.Global.CreateAccountHoldersCustomerFilter(accountHolders, entityName.Contact), entityName.Contact);
        formContext.getControl(formAttributes.requiredattendees).addCustomFilter(Utils.Global.CreateAccountHoldersCustomerFilter(accountHolders, entityName.Account), entityName.Account);
    };

    const setActivitySubjectCustomView = function () {

        if (formContext.getAttribute(formAttributes.alt_activitysubjectid)) {

            const regardingObject = formContext.getAttribute(formAttributes.regardingobjectid).getValue();
            if (regardingObject && regardingObject[0]) {
                const entityType = regardingObject[0].entityType;
                const viewDisplayName = "נושא לפעילות";

                const fetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">' +
                    '<entity name="alt_activitysubject">' +
                    '<attribute name="alt_activitysubjectid" />' +
                    '<attribute name="alt_name" />' +
                    '<filter type="and">' +
                    '<condition attribute="alt_activityregardingobjectschemaname" operator="eq" value="' + entityType + '" />' +
                    '<condition attribute="alt_activitytypecode" operator="eq" value="' + activityTypeCode.Appointment + '" />' +
                    '</filter>' +
                    '</entity>' +
                    '</fetch>';

                const layoutXml = "<grid name='resultset' jump='alt_name' select='1' icon='1' preview='0'>" +
                    '<row name="result" id="alt_activitysubjectid">' +
                    '<cell name="alt_name" width="150" />' +
                    '</row>' +
                    '</grid>';

                const lookupControl = formContext.getControl('alt_activitysubjectid');
                const viewId = '{00000000-0000-0000-0000-000000000001}';

                lookupControl.addCustomView(viewId, "alt_activitysubject", viewDisplayName, fetchXml, layoutXml, true);
            }
        }
    };

    return {
        OnLoad: onLoad
    };
})();


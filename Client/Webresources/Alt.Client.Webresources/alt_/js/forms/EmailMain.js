/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Server.js" />
/// <reference path="../utils/Utils.Global.js" />
/// <reference path="ActivityTemplateCommonBL.js" />

var EmailMain = (function () {

    let formContext;
    let accountHolders = [];

    const sendFromCode = {
        Queue: 100000000,
        Team: 100000001,
        User: 100000002
    };

    const formAttributes = {
        to: 'to',
        regardingobjectid: 'regardingobjectid',
        alt_parsercustomentrypoint: 'alt_parsercustomentrypoint',
        related: 'related',
        alt_emailtemplateid: 'alt_emailtemplateid',
        from: 'from'
    };

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();

        const emailTemplateDtoObject =
        {
            templateEntityName: 'alt_emailtemplate',
            templateType: activityTemplateType.Email,
            templateAttributeName: 'alt_emailtemplateid',
            effectedTemplateAttributesNamesArray: ['subject', 'description', 'to', 'alt_emailtemplateid'],
            regardingOnChangeCallBacks: [],
            initDefaultValuesCallBacks: []
        };

        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;
            switch (formType) {
                case crmFormTypes.Create:
                    {
                        ActivityTemplateCommonBL.OnLoad(executionContext, emailTemplateDtoObject);
                        hanldeEmailRecipientsByRegardingObject();
                        initFormUI();
                        initRelated();
                        initOnChange();
                        break;
                    }
                case crmFormTypes.Update:
                    {
                        ActivityTemplateCommonBL.OnLoad(executionContext, emailTemplateDtoObject);
                        initFormUI();
                        initRelated();
                        initOnChange();
                        break;
                    }
                default:
                    break;
            }
        }
    };

    const initOnChange = function () {

        formContext.getAttribute(formAttributes.alt_emailtemplateid).addOnChange(emailTemplateOnChange);
    };

    const initRelated = function () {

        if (formContext.getAttribute(formAttributes.related)) {

            const regardingObjectId = formContext.getAttribute(formAttributes.regardingobjectid).getValue()
                && formContext.getAttribute(formAttributes.regardingobjectid).getValue()[0];
            if (regardingObjectId.entityType == entityName.Portfolio
                || regardingObjectId.entityType == entityName.DigitalFormVerification) {

                formContext.getControl(formAttributes.related).setEntityTypes([entityName.Contact, entityName.Account]);
                formContext.getAttribute(formAttributes.related).addOnChange(relatedOnChange);
                formContext.getControl(formAttributes.related).addPreSearch(setRelatedCustomFilter);
                Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.related, true);
            }        
        }
    };

    const initFormUI = function () {

        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_emailtemplateid, Utils.CrmPage.RequirementLevel.Required);
    };

    const emailTemplateOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        handleUIByEmailTemplate();
    };

    const relatedOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        setUnresolvedAddressByRelated();
    };

    const hanldeEmailRecipientsByRegardingObject = function () {

        const regardingObject = formContext.getAttribute(formAttributes.regardingobjectid).getValue();
        if (regardingObject && regardingObject[0]) {

            const regardingObjectId = regardingObject[0];
            switch (regardingObjectId.entityType) {
                case entityName.Lead: {
                    break;
                }
                case entityName.Opportunity: {

                    setRecipientFromOpportunity(regardingObjectId);
                    break;
                }
                case entityName.Portfolio:
                case entityName.DigitalFormVerification: {

                    getAccountHolders(regardingObjectId);
                    break;
                }
                case entityName.Incident: {

                    getIncidentDetails(regardingObjectId, handleRelatedAccountHolderByIncident);
                    break;
                }
                default: {
                    Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.to, false);
                    break;
                }

            }
        }
    };

    const handleRelatedAccountHolderByIncident = function (incident) {

        getAccountHolders({ id: incident.alt_portfolioid.Id, entityType: entityName.Portfolio }, function (result) {

            const accountHolder = getAccountHolerByCustomerId(incident.customerid.Id);
            setValuesByAccountHolder(accountHolder);
        });
    };

    const handleUIByEmailTemplate = function () {

        const emailTemplateValue = formContext.getAttribute(formAttributes.alt_emailtemplateid).getValue();
        if (emailTemplateValue && emailTemplateValue[0]) {

            const select = "alt_sendfromcode, _alt_fromqueueid_value, _alt_fromteamid_value, alt_parsercustomentrypointschemaname";
            Utils.Server.Retrieve("alt_emailtemplate", emailTemplateValue[0].id, select, null, function (result) {

                setFromByEmailTemplate(result);
                parseTemplateMessage(result);
            });
        }
        else {
            setFromByEmailTemplate(null);
            ActivityTemplateCommonBL.ParseTemplateMessagesHandler();
        }
    };

    const parseTemplateMessage = function (result) {

        if (result) {
            if (result.alt_parsercustomentrypointschemaname) {
                const parserEntryPoint = formContext.getAttribute(formAttributes.alt_parsercustomentrypoint).getValue();
                if (parserEntryPoint) {

                    ActivityTemplateCommonBL.ParseTemplateMessagesHandler(parserEntryPoint);
                }
                else {
                    const errorMessage = 'הגדרת תבנית דואר אלקטרוני שגויה. נא פנה למנהל מערכת.';
                    Xrm.Navigation.openAlertDialog({ text: errorMessage });
                    Utils.Server.WriteLog(errorMessage + 'Id: ' + smsTemplateId[0].id, Utils.Server.MessageLevel.Warning);
                }
            }
            else {
                ActivityTemplateCommonBL.ParseTemplateMessagesHandler();
            }
        }
        else {

            ActivityTemplateCommonBL.ParseTemplateMessagesHandler();
        }
    };

    const getIncidentDetails = function (regardingObjectId, successCallback) {

        const select = '_customerid_value, _alt_portfolioid_value';
        let id = Utils.JsExtantions.String.RemoveBraces(regardingObjectId.id).toLowerCase();
        Utils.Server.Retrieve(regardingObjectId.entityType, id, select, null, function (result) {
            if (result && result.alt_portfolioid && result.customerid) {

                if (successCallback) {
                    successCallback(result);
                }
            }
        }, null);
    };

    const getAccountHolders = function (regardingObjectId, successCallback) {

        Utils.Global.GetActiveAccountHoldersByRelatedEntity(regardingObjectId, function (result) {
            accountHolders = result;
            if (successCallback) {
                successCallback(result);
            }
        });
    };

    const getAccountHolerByCustomerId = function (customerId) {

        let filteredAccountHolders;

        if (accountHolders && customerId) {
            filteredAccountHolders = accountHolders.filter(function (value) {

                return value.alt_customerid.Id.toLowerCase() == Utils.JsExtantions.String.RemoveBraces(customerId).toLowerCase();
            });
        }

        return filteredAccountHolders && filteredAccountHolders[0];
    };

    const setValuesByAccountHolder = function (accountHolder) {

        if (accountHolder) {
            setParserCustomEntryPoint('alt_accountholder', accountHolder.alt_accountholderid);
            setUnresolvedAddress(accountHolder.alt_email);
            if (accountHolder.alt_email) {
                setRelatedValueByCustomer(accountHolder.alt_customerid);
            }
        }
    };

    const setFromByEmailTemplate = function (result) {

        if (result) {
            switch (result.alt_sendfromcode) {
                case sendFromCode.Team: {
                    const team = result.alt_fromteamid;
                    if (team) {
                        let select = '_queueid_value';
                        Utils.Server.Retrieve(team.LogicalName, team.Id, select, null, function (retrievedTeam) {

                            if (retrievedTeam && retrievedTeam.queueid) {
                                Utils.CrmPage.SetLookup(formContext, formAttributes.from, retrievedTeam.queueid.Id, retrievedTeam.queueid.Name, retrievedTeam.queueid.LogicalName);
                            }
                        });
                    }
                    break;
                }
                case sendFromCode.Queue: {
                    const queue = result.alt_fromqueueid;
                    if (queue) {
                        Utils.CrmPage.SetLookup(formContext, formAttributes.from, queue.Id, queue.Name, queue.LogicalName);
                    }
                    break;
                }
                case sendFromCode.User: {
                    const userSettings = Xrm.Utility.getGlobalContext().userSettings;
                    const currentUserId = userSettings.userId;
                    const currentUserName = userSettings.userName;
                    Utils.CrmPage.SetLookup(formContext, formAttributes.from, currentUserId, currentUserName, "systemuser");
                    break;
                }
                default: {
                    console.error("Invalid send code value");
                    break;
                }
            };
        }
        else {
            formContext.getAttribute(formAttributes.from).setValue(null);
        }
    };

    const setRelatedCustomFilter = function () {

        formContext.getControl(formAttributes.related).addCustomFilter(Utils.Global.CreateAccountHoldersCustomerFilter(accountHolders, entityName.Contact), entityName.Contact);
        formContext.getControl(formAttributes.related).addCustomFilter(Utils.Global.CreateAccountHoldersCustomerFilter(accountHolders, entityName.Account), entityName.Account);
    };

    const setUnresolvedAddressByRelated = function () {

        let relatedValue = formContext.getAttribute(formAttributes.related).getValue();
        let isSetNewValue = false;
        if (relatedValue !== null && relatedValue.length > 0) {
            let recipients = [];
            let newValue = [];
            relatedValue.forEach(function (selectedCustomer, index) {

                const accountHolder = getAccountHolerByCustomerId(selectedCustomer.id);
                if (accountHolder.alt_email) {

                    recipients.push(generateUnresolvedAddress(accountHolder.alt_email, index));
                    newValue.push(selectedCustomer);
                }
                else {
                    isSetNewValue = true;
                    Xrm.Navigation.openAlertDialog({ text: 'שים לב, ל' + selectedCustomer.name + ' לא קיימת כתובת דואר אלקטרוני.' }).then(function () {
                    });
                }
            });
            formContext.getAttribute(formAttributes.to).setValue(recipients);
            if (isSetNewValue) {
                formContext.getAttribute(formAttributes.related).setValue(newValue);
            }
        }
        else {
            formContext.getAttribute(formAttributes.to).setValue(null);
        }
    };

    const setRecipientFromOpportunity = function (regardingObjectId) {

        const select = 'emailaddress, _customerid_value';
        let id = Utils.JsExtantions.String.RemoveBraces(regardingObjectId.id).toLowerCase();
        Utils.Server.Retrieve(regardingObjectId.entityType, id, select, null, function (result) {
            if (result && result.emailaddress) {

                setUnresolvedAddress(result.emailaddress);
                const customer = result.customerid;
                Utils.CrmPage.SetLookup(formContext, formAttributes.related, customer.Id, customer.Name, customer.LogicalName);
            }
        }, null);
    };

    const setParserCustomEntryPoint = function (logicalName, id) {

        const value = Utils.Global.GenerateParserCustomEntryPointEntityReference(logicalName, id);
        formContext.getAttribute(formAttributes.alt_parsercustomentrypoint).setValue(value);
    };

    const setRelatedValueByCustomer = function (customer) {

        if (formContext.getAttribute(formAttributes.related)
            && customer) {
            Utils.CrmPage.SetLookup(formContext, formAttributes.related, customer.Id, customer.Name, customer.LogicalName);
        }
    };

    const setUnresolvedAddress = function (emailAddress) {

        if (emailAddress) {

            let recipients = [];
            recipients.push(generateUnresolvedAddress(emailAddress));
            formContext.getAttribute('to').setValue(recipients);
        }
        else {
            Xrm.Navigation.openAlertDialog({ text: 'לבעל חשבון לא קיימת כתובת דואר אלקטרוני.' });
        }
    };

    const generateUnresolvedAddress = function (emailAddress, index) {

        return {
            name: emailAddress,
            entityType: 'unresolvedaddress',
            id: index ? '{00000000-0000-0000-0000-00000000000' + index + '}' : '{00000000-0000-0000-0000-000000000000}'
        };
    };

    return {
        OnLoad: onLoad
    };
})();
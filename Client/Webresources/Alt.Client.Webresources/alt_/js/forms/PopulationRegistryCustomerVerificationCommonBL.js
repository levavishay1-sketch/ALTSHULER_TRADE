/// <reference path="../utils/Utils.Server.js" />
/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Enums.js" />

var PopulationRegistryCustomerVerificationCommonBL = (function () {

    let formContext;

    const formAttributes = {
        alt_identitynumber: 'alt_identitynumber',
        alt_contactid: 'alt_contactid',
        alt_birthdate: 'alt_birthdate',
        alt_idissuancedate: 'alt_idissuancedate',
        alt_companycodeint: 'alt_companycodeint',
        alt_populationtypecode: 'alt_populationtypecode',
        alt_transferstatuscode: 'alt_transferstatuscode'
    };

    const attributesToEnableWithoutValues = [
        formAttributes.alt_identitynumber,
        formAttributes.alt_birthdate,
        formAttributes.alt_idissuancedate
    ];

    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();

        initOnChange();
        initFormUI();
    };

    const initOnChange = function () {

        formContext.getAttribute(formAttributes.alt_identitynumber).addOnChange(identityNumberOnChange);
    };

    const initFormUI = function () {
        setAttributeDisabledModeByValue();
        setCompanyCodeByCurrentUser();
        setIdentityNumberByContactId();
    };

    const identityNumberOnChange = function (executionContext) {

        formContext = executionContext ? executionContext.getFormContext() : formContext;
        Utils.CrmPage.HandleGovIdAttributeChange(executionContext);
    };

    const setCompanyCodeByCurrentUser = function () {
        const userSettings = Xrm.Utility.getGlobalContext().userSettings;
        const currentUserId = Utils.JsExtantions.String.RemoveBraces(userSettings.userId);
        const expand = 'alt_CompanyId($select=alt_codeint)';
        const select = "_alt_companyid_value";
        Utils.Server.Retrieve("systemuser", currentUserId, select, expand, function (result) {
            if (result && result.alt_CompanyId) {
                const companyCode = result.alt_CompanyId.Expand
                    && result.alt_CompanyId.Expand.alt_codeint;
                if (companyCode) {
                    formContext.getAttribute(formAttributes.alt_companycodeint).setValue(companyCode);
                }
            }
        });
    };

    const setIdentityNumberByContactId = function () {

        const identityNumber = formContext.getAttribute(formAttributes.alt_identitynumber).getValue();
        if (!identityNumber) {
            const contactId = formContext.getAttribute(formAttributes.alt_contactid).getValue()
                && formContext.getAttribute(formAttributes.alt_contactid).getValue()[0];
            if (contactId) {
                const select = "governmentid";
                Utils.Server.Retrieve("contact", contactId.id, select, null, function (result) {
                    if (result && result.governmentid) {
                        formContext.getAttribute(formAttributes.alt_identitynumber).setValue(result.governmentid);
                        formContext.getAttribute(formAttributes.alt_identitynumber).fireOnChange();
                        Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.alt_identitynumber, true);
                    }
                });
            }
        }
        else {
            Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.alt_identitynumber, true);
        }
    };

    const setAttributeDisabledModeByValue = function () {
        attributesToEnableWithoutValues.forEach(function (attributeName) {
            if (!formContext.getAttribute(attributeName).getValue()) {
                Utils.CrmPage.SetControlDisabledMode(formContext, attributeName, false);
            }
        });
       // Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.alt_populationtypecode, false);
    };

    return {
        OnLoad: onLoad
    }

})();
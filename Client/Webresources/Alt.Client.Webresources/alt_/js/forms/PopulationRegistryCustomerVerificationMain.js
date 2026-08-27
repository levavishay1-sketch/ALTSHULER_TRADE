var PopulationRegistryCustomerVerificationBL = (function () {

    const POPULATION_REGISTER_VERIFICATION_REQUEST_NOTIFICATION_MESSAGE = 'מתבצעת פניה למרשם האוכלוסין...';
    let formContext;

    const formAttributes = {
        alt_identitynumber: 'alt_identitynumber',
        alt_contactid: 'alt_contactid',
        alt_birthdate: 'alt_birthdate',
        alt_idissuancedate: 'alt_idissuancedate',
        alt_companycodeint: 'alt_companycodeint',
        alt_populationtypecode: 'alt_populationtypecode',
        alt_transferstatuscode: 'alt_transferstatuscode',
        alt_relatedrecordid: 'alt_relatedrecordid'
    };

    const attributesToDisable = [
        formAttributes.alt_identitynumber,
        formAttributes.alt_birthdate,
        formAttributes.alt_idissuancedate,
        formAttributes.alt_populationtypecode
    ];

    const attributesWithValueToDisplay = [
        formAttributes.alt_contactid,
        formAttributes.alt_relatedrecordid
    ];

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();
        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;
            switch (formType) {
                case crmFormTypes.Create:
                    {
                        PopulationRegistryCustomerVerificationCommonBL.OnLoad(executionContext)
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            Utils.CrmPage.DisplayAttributesWithValue(formContext, attributesWithValueToDisplay);
        }
        else {
            Utils.CrmPage.DisableAttributes(formContext, attributesToDisable, true);
            Utils.CrmPage.DisplayAttributesWithValue(formContext, attributesWithValueToDisplay);
            refreshFormData();
        }
    };

    const onSave = function (executionContext) {

        formContext = executionContext.getFormContext();
        if (formContext.ui.getFormType() == Utils.CrmPage.FormType.Create
            && formContext.getAttribute(formAttributes.alt_transferstatuscode).getValue() == transferStatusCode.Send) {
            Xrm.Utility.showProgressIndicator(POPULATION_REGISTER_VERIFICATION_REQUEST_NOTIFICATION_MESSAGE);
        }
    };

    const refreshFormData = function () {
        formContext.data.refresh(false);
        var statusCodeValue = formContext.getAttribute(formAttributes.alt_transferstatuscode).getValue();
        if (statusCodeValue !== transferStatusCode.Sending) {
            Xrm.Utility.closeProgressIndicator();
            return;
        }
        setTimeout(refreshFormData, 2000);
    };

    return {
        OnLoad: onLoad,
        OnSave: onSave
    }

})();
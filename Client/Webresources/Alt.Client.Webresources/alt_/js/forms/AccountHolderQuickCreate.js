/// <reference path="../utils/Utils.Validators.js" />
/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Enums.js" />
/// <reference path="AccountHolderCommonBL.js" />

var AccountHolderQuickCreate = (function () {

    let formContext;

    const formAttributes = {
        alt_identificationnumber: 'alt_identificationnumber',
        alt_identificationtypecode: 'alt_identificationtypecode',
        alt_creationmethodcode: 'alt_creationmethodcode',
        alt_receivedamendedbeneficiarydeclarationcode: 'alt_receivedamendedbeneficiarydeclarationcode',
        alt_accountholdertypecode: 'alt_accountholdertypecode',
    };

    const creationMethodCode = {
        Manual: 1,
        WebAPI: 2
    };

    const accountHolderTypeCode = {
        Beneficiary: 3
    };

    const identificationTypeCode = {
        ID: 1
    };

    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();

        AccountHolderCommonBL.OnLoad(executionContext);
        initFormUI();
        initOnChange();
    };

    const initFormUI = function () {

        handleBeneficiaryDeclarationCode();
        formContext.getAttribute(formAttributes.alt_creationmethodcode).setValue(creationMethodCode.Manual)
        formContext.getAttribute(formAttributes.alt_identificationtypecode).setValue(identificationTypeCode.ID)
    };

    const initOnChange = function () {

        formContext.getAttribute(formAttributes.alt_accountholdertypecode).addOnChange(handleBeneficiaryDeclarationCode);
    };

    const handleBeneficiaryDeclarationCode = function () {

        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_receivedamendedbeneficiarydeclarationcode, Utils.CrmPage.RequirementLevel.None);
        Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_receivedamendedbeneficiarydeclarationcode, false);

        if (formContext.getAttribute(formAttributes.alt_accountholdertypecode).getValue() != creationMethodCode.WebAPI
            && formContext.getAttribute(formAttributes.alt_accountholdertypecode).getValue() == accountHolderTypeCode.Beneficiary) {

            Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_receivedamendedbeneficiarydeclarationcode, Utils.CrmPage.RequirementLevel.Required);
            Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_receivedamendedbeneficiarydeclarationcode, true);
        }
    };

    return {
        OnLoad: onLoad
    };
})();
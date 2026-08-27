/// <reference path="../utils/Utils.Validators.js" />
/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Server.js" />
/// <reference path="../utils/Utils.Enums.js" />

var AccountHolderMain = (function () {

    const COMPARISON_NOTITIFCATION_MESSAGE_WARNING = "אין התאמה בין מספר הזיהוי שהוקלד ע\"י הלקוח למספר זיהוי המקוון";
    const NoticeMessage = "שים לב!";

    const formAttributes = {
        alt_accountholdertypecode: 'alt_accountholdertypecode',
        alt_identificationnumberinitialcomparisoncode: 'alt_identificationnumberinitialcomparisoncode',
        alt_identificationnumbercontrolcomparisoncode: 'alt_identificationnumbercontrolcomparisoncode',
        alt_onlineidentificationnumber: 'alt_onlineidentificationnumber',
        alt_cityid: 'alt_cityid',
        alt_streetid: 'alt_streetid',
        alt_housenumber: 'alt_housenumber',
        alt_flatnumber: 'alt_flatnumber',
        alt_countryid: 'alt_countryid',
        alt_identificationnumber: 'alt_identificationnumber',
        alt_identificationtypecode: 'alt_identificationtypecode',
        alt_receivedamendedbeneficiarydeclarationcode: 'alt_receivedamendedbeneficiarydeclarationcode',
        alt_digitalformverificationid: 'alt_digitalformverificationid',
        alt_idissuedate: 'alt_idissuedate',
        alt_customersignals: 'alt_customersignals'
    };

    const identificationComparisonFields = [
        formAttributes.alt_onlineidentificationnumber,
        formAttributes.alt_identificationnumberinitialcomparisoncode,
        formAttributes.alt_identificationnumbercontrolcomparisoncode
    ];

    const controlsToEnableForAccountHolderTypeBeneficiary = [
        formAttributes.alt_cityid,
        formAttributes.alt_streetid,
        formAttributes.alt_housenumber,
        formAttributes.alt_flatnumber,
        formAttributes.alt_countryid
    ];

    const comparisonCode = {
        Identical: 1,
        NotIdentical: 2
    };

    const BeneficiarySigningDeclarationCode = {
        OnlineSignature: 1,
        VisualSignature: 2,
        FaceToFace: 3,
        Other: 4,
    };
    const AccountHolderTypeCode = {
        Owner: 1,
        Beneficiary: 3,
        AccountRelated: 2,
        Shareholder: 4,
        AppointedByOrder: 5,
        BeneficiaryShareholder: 6,
        RelatedCorporationShareholder: 7,
        Custodian: 8,
        AuthorizedToReceiveInfomation: 9,
        AuthorizedForOperations: 10,
        Guardian: 11,
        CEO: 12
    };
    const StatusCode = {
        Active: 1,
        Processing: 455710001,
    };
    const DigitalVisualRecognitionCode = {
        Valid: 1,
    };
    const identificationTypeCode = {
        GovernmentId: 1,
        DrivingLicense: 4,
    };
    const creationMethodCode = {
        Manual: 1,
        WebAPI: 2
    };
    const formPageType = {
        EntityRecord: "entityrecord",
        QuickCreate: "quickcreate"
    };
    let formContext;

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();
        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;

            switch (formType) {
                case crmFormTypes.Create: {
                    AccountHolderCommonBL.OnLoad(executionContext);
                    break;
                }
                case crmFormTypes.Update: {
                    AccountHolderCommonBL.OnLoad(executionContext);
                    initOnChange();
                    initFormUI();
                    break;
                }
                default:
                    break;
            }
        }
    };

    const initOnChange = function () {
        formContext.getAttribute("alt_beneficiarydeclarationrequiredbit").addOnChange(onChangeByBeneficiaryDeclarationRequiredBit);
        formContext.getAttribute("alt_birthdate").addOnChange(birthDateOnChanged);
        formContext.getAttribute("alt_deceasedbit").addOnChange(handleUIByDeceasedBit);
        formContext.getAttribute("alt_performadditionalverificationcode").addOnChange(handleUIByPerformAdditionalVerificationCode);
        formContext.getAttribute(formAttributes.alt_cityid).addOnChange(emptyStreetIdOnChangeCityId);
    };

    const initFormUI = function () {

        if (formContext.getAttribute('statuscode').getValue() != StatusCode.Processing) {
            Utils.CrmPage.DisableAllFormFields(formContext);
        } else {
            handleUIByBeneficiaryDeclarationRequiredBit();
            handleUIByDeceasedBit();
            handleUIByPerformAdditionalVerificationCode();
            handleUIByAccountHolderTypeCode();
            handleUIByStatusCode();
            handleUIByIdentificationNumberInitialComparisonCode();
            handleBeneficiaryDeclarationCode();
            handlePopupByCustomerSignals();
            AccountHolderCommonBL.ValidateIdentificationNumber();
        }
    };

    const emptyStreetIdOnChangeCityId = function () {
        formContext.getAttribute(formAttributes.alt_streetid).setValue(null);
    };

    const birthDateOnChanged = function (executionContext) {
        Utils.CrmPage.HandleDateTimeAttributeForPastDateChange(executionContext);
    };

    const handleUIByIdentificationNumberInitialComparisonCode = function () {
        if (formContext.getAttribute(formAttributes.alt_identificationnumbercontrolcomparisoncode)) {

            if (formContext.getAttribute(formAttributes.alt_accountholdertypecode).getValue() == AccountHolderTypeCode.Owner) {
                let comparisonControlCode = formContext.getAttribute(formAttributes.alt_identificationnumbercontrolcomparisoncode).getValue();
                switch (comparisonControlCode) {
                    case comparisonCode.Identical:
                        formContext.getControl(formAttributes.alt_onlineidentificationnumber).setDisabled(true);
                        break;
                    case comparisonCode.NotIdentical:
                        formContext.ui.setFormNotification(COMPARISON_NOTITIFCATION_MESSAGE_WARNING, notificationLevel.Warning, "comparisonNotitifcationMessage");
                        break;
                    default:
                        break;
                }
            }
        }
    };

    const handleUIByAccountHolderTypeCode = function () {

        let attributesRequiredLevel = [];
        const accoutHolderType = formContext.getAttribute('alt_accountholdertypecode').getValue();
        switch (accoutHolderType) {
            case AccountHolderTypeCode.Owner:
                {
                    handleUIByBeneficiarySigningDeclarationCode();
                    attributesRequiredLevel.push('alt_email', 'alt_mobilephone', 'alt_postalcode');
                    handleUIIdentificationComparison();
                    break;
                }
            case AccountHolderTypeCode.AccountRelated:
            case AccountHolderTypeCode.Beneficiary:
                {
                    controlsToEnableForAccountHolderTypeBeneficiary.forEach(controlName =>
                        Utils.CrmPage.SetControlDisabledMode(formContext, controlName, false)
                    );

                    break;
                }
            case AccountHolderTypeCode.Shareholder:
            case AccountHolderTypeCode.AppointedByOrder:
            case AccountHolderTypeCode.BeneficiaryShareholder:
            case AccountHolderTypeCode.RelatedCorporationShareholder:
            case AccountHolderTypeCode.Custodian:
            case AccountHolderTypeCode.AuthorizedToReceiveInfomation:
            case AccountHolderTypeCode.AuthorizedForOperations:
            case AccountHolderTypeCode.Guardian:
            case AccountHolderTypeCode.CEO:
                {
                    handleUIByDigitalVisualRecognitionCode(attributesRequiredLevel);
                }
                break;
            default:
                break;
        }
        if (accoutHolderType != AccountHolderTypeCode.Beneficiary) {

            attributesRequiredLevel.push('alt_manualcontrolverificationidappliedcode', 'alt_manualcontrolverificationiddescription');
        }
        if (attributesRequiredLevel.length > 0) {

            attributesRequiredLevel.forEach(attributeName => Utils.CrmPage.SetAttributeRequiredLevel(formContext, attributeName, Utils.CrmPage.RequirementLevel.Required));
        }
    };

    const handleUIIdentificationComparison = function () {
        if (formContext.getAttribute(formAttributes.alt_accountholdertypecode)) {

            if (formContext.getAttribute(formAttributes.alt_accountholdertypecode).getValue() == AccountHolderTypeCode.Owner) {
                Utils.CrmPage.HandleControlsVisibleMode(formContext, identificationComparisonFields, true);
            }
        }
    };

    const handleUIByDigitalVisualRecognitionCode = function (attributesRequiredLevel) {
        if (formContext.getAttribute('alt_digitalvisualrecognitioncode').getValue() != DigitalVisualRecognitionCode.Valid) {
            attributesRequiredLevel.push('alt_performadditionalverificationcode');
        }
    };

    const handleUIByStatusCode = function () {
        switch (formContext.getAttribute('statuscode').getValue()) {
            case StatusCode.Processing:
                {
                    let attributes = ['alt_secondidentificationtypecode', 'alt_secondaryidentificationnumber', 'alt_secondaryidissueddate', 'alt_secondaryidentificationissuingcountryid'];
                    attributes.forEach(attributeName => Utils.CrmPage.SetControlDisabledMode(formContext, attributeName, false));
                }
                break;
            default:
                break;
        }
    };

    const handleUIByBeneficiarySigningDeclarationCode = function () {
        if (formContext.getAttribute('alt_beneficiarysigningdeclarationcode').getValue() == BeneficiarySigningDeclarationCode.Other) {
            formContext.getControl("alt_beneficiarysigningdeclarationcode").setDisabled(false);
            formContext.getControl('alt_beneficiarysigningdeclarationcode').removeOption(BeneficiarySigningDeclarationCode.OnlineSignature);
            formContext.getControl('alt_beneficiarysigningdeclarationcode').removeOption(BeneficiarySigningDeclarationCode.VisualSignature);
        } else {
            formContext.getControl("alt_beneficiarysigningdeclarationcode").setDisabled(true);
        }
    };

    const handleUIByBeneficiaryDeclarationRequiredBit = function () {
        if (formContext.getAttribute('alt_beneficiarydeclarationrequiredbit').getValue() == true) {
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_beneficiarydeclarationcontrolcode', Utils.CrmPage.RequirementLevel.Required);
            formContext.getControl("alt_beneficiarydeclarationrequiredbit").setDisabled(true);
        }
        else {
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_beneficiarydeclarationcontrolcode', Utils.CrmPage.RequirementLevel.None);
            formContext.getControl("alt_beneficiarydeclarationrequiredbit").setDisabled(false);
        }
    }

    const onChangeByBeneficiaryDeclarationRequiredBit = function () {
        if (formContext.getAttribute('alt_beneficiarydeclarationrequiredbit').getValue() == true) {
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_beneficiarydeclarationcontrolcode', Utils.CrmPage.RequirementLevel.Required);
        }
        else {
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_beneficiarydeclarationcontrolcode', Utils.CrmPage.RequirementLevel.None);
        }
    }

    const handleUIByDeceasedBit = function () {
        if (formContext.getAttribute('alt_deceasedbit').getValue() == true) {
            Utils.CrmPage.SetControlVisibleMode(formContext, 'alt_deceaseddate', true);
        } else {
            Utils.CrmPage.SetControlVisibleMode(formContext, 'alt_deceaseddate', false);
        }
    };

    const handleUIByPerformAdditionalVerificationCode = function () {
        if (formContext.getAttribute('alt_performadditionalverificationcode').getValue() != null) {
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_performadditionalverificationdate', Utils.CrmPage.RequirementLevel.Required);
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_performadditionalverificationsystemuserid', Utils.CrmPage.RequirementLevel.Required);
        } else {
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_performadditionalverificationdate', Utils.CrmPage.RequirementLevel.None);
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_performadditionalverificationsystemuserid', Utils.CrmPage.RequirementLevel.None);
        }
    };

    const handleBeneficiaryDeclarationCode = function () {
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_receivedamendedbeneficiarydeclarationcode, Utils.CrmPage.RequirementLevel.None);
        Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_receivedamendedbeneficiarydeclarationcode, false);

        if (formContext.getAttribute(formAttributes.alt_accountholdertypecode).getValue() != creationMethodCode.WebAPI
            && formContext.getAttribute(formAttributes.alt_accountholdertypecode).getValue() == AccountHolderTypeCode.Beneficiary) {

            Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_receivedamendedbeneficiarydeclarationcode, Utils.CrmPage.RequirementLevel.Required);
            Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_receivedamendedbeneficiarydeclarationcode, true);
        }
    };

    const handlePopupByCustomerSignals = function () {

        const customerSignals = formContext.getAttribute(formAttributes.alt_customersignals).getValue();
        if (customerSignals != null && customerSignals.trim() !== "") {
            const message = NoticeMessage + "\n" + customerSignals;
            Xrm.Navigation.openAlertDialog({ text: message });
        }
    };

    return {
        OnLoad: onLoad
    };
})();
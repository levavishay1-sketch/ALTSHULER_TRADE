/// <reference path="../utils/Utils.JsExtantions.js" />
/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Validators.js" />
/// <reference path="../utils/Utils.Server.js" />
/// <reference path="../utils/Utils.Global.js" />

var AuthorizationManagementMain = (function () {

    const formAttributes = {
        alt_controlstagestatuscode: "alt_controlstagestatuscode",
        alt_digitalformverificationid: "alt_digitalformverificationid",
        alt_backconrolreason: "alt_backconrolreason"
    };

    const ControlStageTeamId = {
        JoiningControl: "בקרת הצטרפות",
        ManagementControl: "בקרת מנהל/ת",
        MoneyLaunderingControl: "בקרת הלבנת הון",
        OperationalControl: "בקרה תפעולית"
    };
    const ControlStageStatusCode = {
        Approval: 1,
        BackControl: 2,
        FormCancellation: 3,
        SentBackManagementControl: 4,
    };
    const CreditRequestCode = {
        No: 1,
        Yes: 2
    };
    const InitialDepositCode = {
        AwaitinglDeposit: 2,
        AcceptedDepositForApproval: 4
    };
    const CreditRequestExistsCode = {
        Yes: 1,
        No: 2
    };
    const ShortSaleRequestApprovaiExistsCode = {
        Yes: 1,
        No: 2
    };
    const OptionExerciseRequestApprovalExistsCode = {
        No: 1,
        OnlyBuySell: 2,
        IncludOptions: 3
    };
    const CapitalRiskLeveAccountCode = {
        Low: 1,
        Medium: 2,
        High: 3
    };
    const formStatusCode = {
        ApprovalProcess: 2
    };

    const alertErrorCallbackRemoteServer = 'תקלה בקבלת הנתונים לחיצה על אישור תרענן את הדף, במידה והתקלה חוזרת על עצמה יש לפנות לתמיכה.';
    const alertFormCancellation = 'פעולה זו תבטל את תהליך ההצטרפות. ולא יהיה ניתן לחדשו';
    var notificationPopup = true;
    let formContext;

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();
        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;
            switch (formType) {
                case crmFormTypes.Create:
                case crmFormTypes.Update:
                default:
                    break;
            }
            initFormUI();
        }
    };

    const onSave = function (executionContext) {
        formContext = executionContext.getFormContext();
        if (notificationPopup) {
            handleUIByControlStageStatusCode(executionContext);
        }
        else {
            notificationPopup = true;
        }
    };


    const initFormUI = function () {
        handleUIByControlStageTeamId();
        handleUIBackConrolReason();
        initOnChangeControlStageStatusCode();
        handleUIControlStageStatusCode();
    };

    const handleUIByControlStageTeamId = function () {
        switch (formContext.getAttribute('alt_controlstageteamid').getValue() != null && formContext.getAttribute('alt_controlstageteamid').getValue()[0].name) {
            case ControlStageTeamId.JoiningControl:
                joiningControlStatus();
                break;
            case ControlStageTeamId.ManagementControl:
                handleUIByJoiningManagementControl();
                initOnChangeManagementControl();
                handleUIByCreditRequestCode();
                handleUIByChangesAfterManagerApproval();
                managementControlStatus();
                break;
            case ControlStageTeamId.MoneyLaunderingControl:
                formContext.getControl('alt_controlstagestatuscode').removeOption(ControlStageStatusCode.FormCancellation);
                break;
            case ControlStageTeamId.OperationalControl:
                operationalControlStatus();
                break;
            default: break;
        }
    };

    const initOnChangeManagementControl = function () {
        formContext.getAttribute("alt_creditrequestcode").addOnChange(handleUIByCreditRequestCode);
    };

    const initOnChangeControlStageStatusCode = function () {
        formContext.getAttribute(formAttributes.alt_controlstagestatuscode).addOnChange(handleUIBackConrolReason);
    };

    const handleUIByJoiningManagementControl = function () {
        Utils.CrmPage.SetControlVisibleMode(formContext, 'alt_creditrequestcode', true);
        Utils.CrmPage.SetControlVisibleMode(formContext, 'alt_creditamountnismny', true);
        Utils.CrmPage.SetControlVisibleMode(formContext, 'alt_linewriteoptionsmny', true);
        Utils.CrmPage.SetControlVisibleMode(formContext, 'alt_linestockshortmny', true);
        Utils.CrmPage.SetControlVisibleMode(formContext, 'alt_lineaggregatecreditlimitmny', true);
        Utils.CrmPage.SetControlVisibleMode(formContext, 'alt_lineaggregatecreditlimitpercentint', true);
        Utils.CrmPage.SetControlVisibleMode(formContext, 'alt_shortsalerequestapprovalbit', true);
        Utils.CrmPage.SetControlVisibleMode(formContext, 'alt_optinexerciserequestapprovalcode', true);
        Utils.CrmPage.SetControlVisibleMode(formContext, 'alt_creditrequestremarks', true);
    };

    const joiningControlStatus = function () {
        // TO DO: Natasha need to replacement to Xrm.WebApi.retrieveRecord       
        const entityLogicalName = formContext.getAttribute('alt_digitalformverificationid').getValue()[0].entityType;
        const select = 'alt_beneficiarydeclarationcontrolexistsbit';
        const filter = 'alt_digitalformverificationid eq ' + formContext.getAttribute('alt_digitalformverificationid').getValue()[0].id +
            'and alt_initialdepositcode ne null and alt_verifiedaccountholdersforstagejoiningbit eq true and alt_verifiedkycforstagejoiningcontrolbit eq true';
        Utils.Server.RetrieveMultiple(entityLogicalName, select, filter, null, null, function (receivedData) {
            if (receivedData && (formContext.getAttribute('alt_capitalrisklevelaccountcode').getValue() != CapitalRiskLeveAccountCode.High || receivedData[0].alt_beneficiarydeclarationcontrolexistsbit == true)) {
                formContext.getControl('alt_controlstagestatuscode').removeOption(ControlStageStatusCode.BackControl);
                formContext.getControl('alt_controlstagestatuscode').removeOption(ControlStageStatusCode.FormCancellation);
            } else {
                formContext.getControl("alt_controlstagestatuscode").setDisabled(true);
            }
        }, errorCallbackRemoteServer);
    };

    const managementControlStatus = function () {
        // TO DO: Natasha need to replacement to Xrm.WebApi.retrieveRecord
        const entityLogicalName = formContext.getAttribute('alt_digitalformverificationid').getValue()[0].entityType;
        const select = 'alt_creditrequestexistscode,alt_shortsalerequestapprovaiexistscode,alt_optionexerciserequestapprovalexistscode';
        const filter = 'alt_digitalformverificationid eq ' + formContext.getAttribute('alt_digitalformverificationid').getValue()[0].id + 'and alt_verifiedaccountholdersstagemanagementbit eq true';
        Utils.Server.RetrieveMultiple(entityLogicalName, select, filter, null, null, function (receivedData) {
            if (receivedData) {
                if (receivedData[0].alt_creditrequestexistscode == CreditRequestExistsCode.Yes) {
                    Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_creditrequestcode', Utils.CrmPage.RequirementLevel.Required);
                }
                if (receivedData[0].alt_shortsalerequestapprovaiexistscode == ShortSaleRequestApprovaiExistsCode.Yes) {
                    Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_shortsalerequestapprovalbit', Utils.CrmPage.RequirementLevel.Required);
                }
                if (receivedData[0].alt_optionexerciserequestapprovalexistscode != OptionExerciseRequestApprovalExistsCode.No) {
                    Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_optinexerciserequestapprovalcode', Utils.CrmPage.RequirementLevel.Required);
                }
            }
            else {
                formContext.getControl('alt_controlstagestatuscode').removeOption(ControlStageStatusCode.Approval);
            }
        }, errorCallbackRemoteServer);
    };

    const operationalControlStatus = function () {
        // TO DO: Natasha do replacement to Xrm.WebApi.retrieveRecord
        const entityLogicalName = formContext.getAttribute('alt_digitalformverificationid').getValue()[0].entityType;
        const select = 'alt_initialdepositcode, alt_formstatuscode';
        const filter = 'alt_digitalformverificationid eq ' + formContext.getAttribute('alt_digitalformverificationid').getValue()[0].id;// + 'and alt_initialdepositcode eq ' + InitialDepositCode.AwaitinglDeposit;
        Utils.Server.RetrieveMultiple(entityLogicalName, select, filter, null, null, function (receivedData) {
            if (receivedData) {
                let retrievedDigitalFormVerification = receivedData[0];
                if (retrievedDigitalFormVerification.alt_initialdepositcode == InitialDepositCode.AwaitinglDeposit) {
                    formContext.getControl('alt_controlstagestatuscode').removeOption(ControlStageStatusCode.Approval);
                    formContext.getControl('alt_controlstagestatuscode').removeOption(ControlStageStatusCode.BackControl);
                } else
                    if (retrievedDigitalFormVerification.alt_initialdepositcode != InitialDepositCode.AcceptedDepositForApproval
                        || retrievedDigitalFormVerification.alt_formstatuscode != formStatusCode.ApprovalProcess) {

                        formContext.getControl('alt_controlstagestatuscode').removeOption(ControlStageStatusCode.FormCancellation);
                    }
            } else {
                formContext.getControl("alt_controlstagestatuscode").setDisabled(true);
            }
        }, errorCallbackRemoteServer);
    };

    const handleUIByCreditRequestCode = function () {
        if (formContext.getAttribute('alt_creditrequestcode').getValue() == CreditRequestCode.Yes) {
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_creditamountnismny', Utils.CrmPage.RequirementLevel.Required);
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_linewriteoptionsmny', Utils.CrmPage.RequirementLevel.Required);
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_linestockshortmny', Utils.CrmPage.RequirementLevel.Required);
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_lineaggregatecreditlimitmny', Utils.CrmPage.RequirementLevel.Required);
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_lineaggregatecreditlimitpercentint', Utils.CrmPage.RequirementLevel.Required);
        } else {
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_creditamountnismny', Utils.CrmPage.RequirementLevel.None);
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_linewriteoptionsmny', Utils.CrmPage.RequirementLevel.None);
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_linestockshortmny', Utils.CrmPage.RequirementLevel.None);
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_lineaggregatecreditlimitmny', Utils.CrmPage.RequirementLevel.None);
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, 'alt_lineaggregatecreditlimitpercentint', Utils.CrmPage.RequirementLevel.None);
        }
    };

    const errorCallbackRemoteServer = function (receivedData) {
        const alertStrings = { confirmButtonLabel: 'אישור', text: alertErrorCallbackRemoteServer, title: 'לתשומת ליבך' };
        Xrm.Navigation.openAlertDialog(alertStrings).then(
            function (success) {
                Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), Xrm.Page.data.entity.getId());
            }
        );
    };

    const handleUIByControlStageStatusCode = function (executionContext) {
        if (formContext.getAttribute('alt_controlstagestatuscode').getValue() == ControlStageStatusCode.FormCancellation && formContext.getAttribute('alt_controlstageteamid').getValue()[0].name == ControlStageTeamId.ManagementControl) {
            const alertStrings = { text: alertFormCancellation, title: 'לתשומת ליבך' };
            executionContext.getEventArgs().preventDefault();
            Xrm.Navigation.openConfirmDialog(alertStrings).then(
                function (success) {
                    if (success.confirmed) {
                        notificationPopup = false;
                        formContext.data.entity.save();
                    }
                }
            );
        }
    };

    const handleUIByChangesAfterManagerApproval = function () {

        const digitalFormVerification = formContext.getAttribute(formAttributes.alt_digitalformverificationid);
        if (digitalFormVerification != null && digitalFormVerification.getValue() != null) {
            const digitalFormVerificationId = digitalFormVerification.getValue();
            const id = Utils.JsExtantions.String.RemoveBraces(digitalFormVerificationId[0].id);
            Xrm.WebApi.retrieveRecord(digitalFormVerificationId[0].entityType, id, "?$select=alt_changesaftermanagerapproval").then(
                (success) => {
                    if (!Utils.JsExtantions.String.IsNullOrEmpty(success.alt_changesaftermanagerapproval)) {
                        Utils.CrmPage.SetSectionVisibleMode(formContext, "GeneralTab", "ChangesAfterManagerApprovalSection", true);
                    }
                },
                (error) => {
                    console.log(error);
                }
            )
        }
    };

    const handleUIBackConrolReason = function () {
        const controlStageStatusCode = formContext.getAttribute(formAttributes.alt_controlstagestatuscode);
        if (controlStageStatusCode != null && controlStageStatusCode.getValue() != null) {
            const code = controlStageStatusCode.getValue();
            if (code === ControlStageStatusCode.BackControl) {
                Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_backconrolreason, true);
                Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_backconrolreason, Utils.CrmPage.RequirementLevel.Required);
            }
            else {
                Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_backconrolreason, false);
                Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_backconrolreason, Utils.CrmPage.RequirementLevel.None);
            }
        }
        else {
            Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_backconrolreason, false);
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_backconrolreason, Utils.CrmPage.RequirementLevel.None);
        }
    };

    const handleUIControlStageStatusCode = function () {

        const controlStageStatusCode = formContext.getAttribute(formAttributes.alt_controlstagestatuscode);
        if (controlStageStatusCode != null) {
            const code = controlStageStatusCode.getValue();
            if (code !== ControlStageStatusCode.SentBackManagementControl) {
                Utils.Global.RemoveOptionsetValuesByGlobalParams(formContext, "TradeOptionSetsValuesToRemove", "alt_authorizationmanagement", formAttributes.alt_controlstagestatuscode);
            }
        }
    };

    return {
        OnLoad: onLoad,
        OnSave: onSave
    };
})();
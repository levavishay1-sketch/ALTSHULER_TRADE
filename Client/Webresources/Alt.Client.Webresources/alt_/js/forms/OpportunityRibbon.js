var OpportunityRibbonBL = (function () {

    const LEAD_NOT_EXIST_ERROR = "לא ניתן לסגור הזדמנות ללא הפניה מקושרת.";
    const INVALID_OPPORTUNITY_FOR_WIN = "לא ניתן לאשר הזדמנות זו מכיוון שהיא מקושרת לתהליך של טופס דיגיטלי פעיל.";
    const INVALID_OPPORTUNITY_FOR_LOST = "לא ניתן לפסול הזדמנות זו מכיוון שהיא מקושרת לתהליך של טופס דיגיטלי פעיל. במידה ורוצים לבטל את ההזדמנות, יש להיכנס באמצעות הלינק לטופס הדיגיטלי ולבטל משם.";
    const opportunityOperation = {
        Win: 1,
        Lost: 2
    };
    let formContext;

    const winOpportunityOnClick = function (primaryControl) {

        formContext = primaryControl;
        handleCloseOpportunity(opportunityOperation.Win);
    };

    const lostOpportunityOnClick = function (primaryControl) {

        formContext = primaryControl;
        handleCloseOpportunity(opportunityOperation.Lost);
    };

    const populationRegisterValidationRequestOnClick = function (primaryControl, isRelatedCustomer) {

        formContext = primaryControl;

        if (formContext.data.entity.getIsDirty()) {
            formContext.data.save().then(function () {
                openPopulationRegistryCustomerVerificationForm(isRelatedCustomer)
            });
        }
        else {
            openPopulationRegistryCustomerVerificationForm(isRelatedCustomer)
        }
    };

    const openPopulationRegistryCustomerVerificationForm = function (isRelatedCustomer) {

        let dto = {
            joiningProcessNumber :formContext.getAttribute('alt_opportunityidentitynumber').getValue()
        };
        if (isRelatedCustomer) {
            dto.customer = formContext.getAttribute('customerid').getValue()
                && formContext.getAttribute('customerid').getValue()[0];
        }
        CustomerActivitiesCommonBL.OpenPopulationRegistryCustomerVerificationForm(formContext, dto, false, true);
    };

    const refreshForm = function () {
        formContext.data.refresh(false);
        var stateCodeValue = formContext.getAttribute("statecode").getValue();
        if (stateCodeValue !== 0) {
            Xrm.Utility.closeProgressIndicator();
            return;
        }
        setTimeout(refreshForm, 1000);
    };

    const handleCloseOpportunity = function (operation) {

        const originatingLeadId = formContext.getAttribute("originatingleadid").getValue();
        if (originatingLeadId && originatingLeadId[0]) {
            var leadId = Utils.JsExtantions.String.RemoveBraces(originatingLeadId[0].id);
            getTradeJoiningDigitalFormsByRegardingObject(leadId, function (result) {

                if (result && result.length > 0) {
                    const errorMessage = operation == opportunityOperation.Win ?
                        INVALID_OPPORTUNITY_FOR_WIN : INVALID_OPPORTUNITY_FOR_LOST;
                    Xrm.Navigation.openAlertDialog({ text: errorMessage });
                }
                else {
                    closeOpportunity(operation);
                }
            }, function (error) {
                Xrm.Navigation.openAlertDialog({ text: INTERNAL_SERVER_ERROR });
                console.log(error);
            });
        }
        else {
            Xrm.Navigation.openAlertDialog({ text: LEAD_NOT_EXIST_ERROR });
        }
    };

    const closeOpportunity = function (opportunityOperation) {

        Xrm.Utility.showProgressIndicator("מתבצעת סגירת הזדמנות...");
        formContext.getAttribute("alt_opportunityoperationcode").setValue(opportunityOperation);
        formContext.data.save().then(
            function () {
                refreshForm();
            },
            function (error) {
                Xrm.Utility.closeProgressIndicator();
                Xrm.Navigation.openAlertDialog({ text: error });
                formContext.getAttribute("alt_opportunityoperationcode").setValue(null);
                console.log(error);
            });
    };

    const getTradeJoiningDigitalFormsByRegardingObject = function (regardingObjectid, successCallBack, errorCallback) {

        var fetchXml = [
            '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">',
            '<entity name="alt_digitalform">',
            '<filter type="and">',
            '<condition attribute="regardingobjectid" operator="eq" uiname="Test Qualify" uitype="lead" value="' + regardingObjectid + '"/>',
            '<condition attribute="alt_digitalformtypecode" operator="eq" value="1"/>',
            '<condition attribute="statecode" operator="ne" value="2"/>',
            '</filter>',
            '</entity>',
            '</fetch>'
        ].join("");

        Utils.Server.Fetch("alt_digitalform", fetchXml, successCallBack, errorCallback);
    };

    const isWinOpportunityButtonEnabled = function (primaryControl) {
        formContext = primaryControl;
        return true;
    };

    const isLostOpportunityButtonEnabled = function (primaryControl) {
        formContext = primaryControl;
        return true;
    };

    const isPopulationRegisterValidationButtonEnabled = function (primaryControl) {
        formContext = primaryControl;
        return true;
    };

    return {
        WinOpportunityOnClick: winOpportunityOnClick,
        LostOpportunityOnClick: lostOpportunityOnClick,
        IsLostOpportunityButtonEnabled: isLostOpportunityButtonEnabled,
        IsWinOpportunityButtonEnabled: isWinOpportunityButtonEnabled,
        PopulationRegisterValidationRequestOnClick: populationRegisterValidationRequestOnClick,
        IsPopulationRegisterValidationButtonEnabled: isPopulationRegisterValidationButtonEnabled
    };

}());
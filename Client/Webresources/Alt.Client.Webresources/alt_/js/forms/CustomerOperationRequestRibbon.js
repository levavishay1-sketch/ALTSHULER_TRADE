var CustomerOperationRequestRibbon = (function () {

    const CREATE_IN_PROGRESS_NOTIFICATION_MESSAGE = 'מתבצעת...';
    const CustomerOperationTypeCode = {
        TradeOneUser: 1,
        Benefit: 2,
        CustomerAgreement: 3,
        ClubMembershipEligibility: 4
    };
    const customerOperationRequestEntityLogicalName = 'alt_customeroperationrequest';
    const settings = {
        "parentLogicalName": "alt_accountholder",
        "gridName": "CustomerOperationRequestsGrid",
        "flyoutButtons": [
            {
                "buttonName": "יוזר טרייד 1",
                "buttonId": 1
            },
            {
                "buttonName": "קורס חינם",
                "buttonId": 2
            },
            {
                "buttonName": "שלח הסכם לקוח",
                "buttonId": 3
            },
            {
                "buttonName": "בדוק זכאות מבצע",
                "buttonId": 4
            }
        ]
    };

    let buttons;
    let formContext;

    var createCustomerOperationRequestFromDynamicMenu = function (commandProperties, primaryControl) {

        formContext = primaryControl ? primaryControl : null;

        populateCreateIncidentDynamicMenu(commandProperties, primaryControl);
    };

    var populateCreateIncidentDynamicMenu = function (commandProperties, primaryControl) {

        formContext = primaryControl ? primaryControl : Xrm.Page;

        var command = "alt.SubGrid.alt_customeroperationrequest.Create.FlyoutItem.Command";
        if (commandProperties.SourceControlId) {
            var source = commandProperties.SourceControlId.split('|');
            if (source.length > 3) {
                command = source[0] + "|" + source[1] + "|" + source[2] + "|" + command;
            }
        }
        createPopulationXML(commandProperties, command);
    };

    var createPopulationXML = function (commandProperties, command) {

        const imagePath = "$webresource:alt_/img/CustomerOperationRequest.svg";
        var ribbonXml = "<MenuSection Id='alt.SubGrid.alt_customeroperationrequest.Create.MenuSection' Sequence='10'>" +
            "<Controls Id='alt.SubGrid.alt_customeroperationrequest.Create.Control'>";
        setFlyoutButtons();
        for (var i = 0; i < buttons.length; i++) {
            var name = buttons[i].name;
            var value = buttons[i].id;

            ribbonXml +=
                "<Button Id='" + value +
                "' Command='" + command +
                "' ModernImage='" + imagePath +
                "' Sequence='" + (i + 1) * 10 +
                "' LabelText='" + name + "' />";
        }
        ribbonXml += "</Controls></MenuSection>";
        commandProperties["PopulationXML"] = '<Menu Id="alt.SubGrid.alt_customeroperationrequest.Create.Menu">' + ribbonXml + "</Menu>";
    };

    var setFlyoutButtons = function () {

        buttons = [];
        for (var i = 0; i < settings.flyoutButtons.length; i++) {
            if (isButtonEnabled(settings.flyoutButtons[i].buttonId)) {
                let button = {};
                button.name = settings.flyoutButtons[i].buttonName;
                button.id = settings.flyoutButtons[i].buttonId;
                buttons.push(button);
            }
        }
    };

    var createRecord = function (relatedRecordEntityName, relatedRecordId, buttonId) {

        Xrm.Utility.showProgressIndicator(CREATE_IN_PROGRESS_NOTIFICATION_MESSAGE);

        let data = {
            'alt_customeroperationtemplatecodeint': buttonId
        };
        data[`alt_RelatedRecordId_${relatedRecordEntityName}@odata.bind`] = `/${relatedRecordEntityName}s(${relatedRecordId})`;
        Xrm.WebApi.createRecord(customerOperationRequestEntityLogicalName, data).then(
            function success(result) {
                let gridContext = formContext.getControl(settings.gridName);
                gridContext.refresh();
                Xrm.Utility.closeProgressIndicator();
            },
            function (error) {
                Xrm.Navigation.openAlertDialog({ text: 'יצירה נכשלה. נא פנה למנהל מערכת.' });
                Xrm.Utility.closeProgressIndicator();
                console.log(error);
            }
        );
    }

    var createCustomerOperationRequestOnClick = function (commandProperties, primaryControl) {

        let id = commandProperties.SourceControlId;
        formContext = primaryControl ? primaryControl : Xrm.Page;

        const buttonSettings = settings.flyoutButtons.filter(function (value) {
            return value.buttonId == id;
        });
        if (buttonSettings.length > 0) {
            retrieveCustomerOperationRequests(buttonSettings[0].buttonId);
        }
    };

    var retrieveCustomerOperationRequests = function (id) {

        let primaryEnityName = formContext.data.entity.getEntityName();
        let primaryEnityId = formContext.data.entity.getId().replace("{", "").replace("}", "");

        const query = `?$select=alt_customeroperationrequestid&$filter=_alt_relatedrecordid_value eq ${primaryEnityId} and alt_customeroperationtemplatecodeint eq ${id}`;
        Xrm.WebApi.retrieveMultipleRecords(customerOperationRequestEntityLogicalName, query).then(
            function success(result) {
                if (result.entities.length > 0) {
                    Xrm.Navigation.openAlertDialog({ text: 'בקשה קיימת כבר. לא ניתן ליצור רשומה נוספת.' });
                }
                else {
                    createRecord(primaryEnityName, primaryEnityId, id)
                }
            },
            function (error) {
                Xrm.Navigation.openAlertDialog({ text: 'לא הייתה אפשרות לבצע את הפעולה, נא פנה למנהל מערכת.' });
                console.log(error);
            }
        )
    };

    var createCustomerOperationRequestEnableRule = function (primaryControl) {

        formContext = primaryControl ? primaryControl : Xrm.Page;

        return true;
        //let primaryEnityName = formContext.data.entity.getEntityName();
        //switch (primaryEnityName) {
        //    case "alt_accountholder": {
        //        return formContext.getAttribute("alt_digitalformverificationid").getValue() != null
        //            && formContext.getAttribute("alt_portfolioid").getValue() != null
        //            && formContext.getAttribute("statuscode").getValue() == 1;
        //        break;
        //    }
        //    default:
        //        return false;
        //}
    };

    var createTradeOneUserEnableRule = function () {

        return formContext.getAttribute("alt_mainaccountholderbit").getValue() == true
            && formContext.getAttribute("alt_accountholdertypecode").getValue() == 1
            && formContext.getAttribute("alt_shouldsendtradeinterfacebit").getValue()
            && formContext.getAttribute("alt_digitalformverificationid").getValue() != null
            && formContext.getAttribute("alt_portfolioid").getValue() != null
            && formContext.getAttribute("statuscode").getValue() == 1;
    };

    var sendCustomerAgreementEnableRule = function () {

        return formContext.getAttribute("alt_mainaccountholderbit").getValue() == true
            && formContext.getAttribute("alt_accountholdertypecode").getValue() == 1
            && formContext.getAttribute("alt_digitalformverificationid").getValue() != null
            && formContext.getAttribute("alt_portfolioid").getValue() != null
            && formContext.getAttribute("statuscode").getValue() == 1;
    };

    var sendJoiningBenefitEnableRule = function () {

        return formContext.getAttribute("alt_accountholdertypecode").getValue() == 1
            && formContext.getAttribute("alt_digitalformverificationid").getValue() != null
            && formContext.getAttribute("alt_portfolioid").getValue() != null
            && formContext.getAttribute("statuscode").getValue() == 1; // פעיל
    };

    var checkClubMembershipEligibilityEnableRule = function () {

        return formContext.getAttribute("alt_mainaccountholderbit").getValue() == true
            && formContext.getAttribute("alt_accountholdertypecode").getValue() == 1
            && formContext.getAttribute("alt_digitalformverificationid").getValue() != null
            && formContext.getAttribute("alt_portfolioid").getValue() == null
            && formContext.getAttribute("statuscode").getValue() == 455710001; // בהקמה
    };

    var isButtonEnabled = function (buttonId) {

        switch (buttonId) {
            case CustomerOperationTypeCode.TradeOneUser: {
                return createTradeOneUserEnableRule();
            }
            case CustomerOperationTypeCode.CustomerAgreement: {
                return sendCustomerAgreementEnableRule();
            }
            case CustomerOperationTypeCode.Benefit: {
                return sendJoiningBenefitEnableRule();
            }
            case CustomerOperationTypeCode.ClubMembershipEligibility: {
                return checkClubMembershipEligibilityEnableRule();
            }
            default:
                return true;
        }
    }

    return {
        CreateCustomerOperationRequestFromDynamicMenu: createCustomerOperationRequestFromDynamicMenu,
        CreateCustomerOperationRequestOnClick: createCustomerOperationRequestOnClick,
        CreateCustomerOperationRequestEnableRule: createCustomerOperationRequestEnableRule
    };
})();
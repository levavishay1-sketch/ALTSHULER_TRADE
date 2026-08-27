/// <reference path="../utils/Utils.Validators.js" />
/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Enums.js" />

var PortfolioMain = (function () {

    const LineAndAllowedTabNotification = "לתשומת לבך - הנתונים במסך זה עדכניים לבוקר היום הנוכחי בלבד. לנתוני זמן אמת, יש לבדוק מול שנהב.";
    const NoticeMessage = "שים לב!";
    const PreferredPortfolioMessage = "זהו חשבון מועדף";
    var IsLineAndAllowedTabNotification = false;

    const formAttributes = {
        alt_customersignals: 'alt_customersignals',
        alt_defultaccountbit: 'alt_defultaccountbit'
    };

    let formContext;

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();
        if (Utils.CrmPage.IsFirstLoad()) {
            initTabUI();
            initFormUI();
        }
    };

    const initFormUI = function () {
        handlePopupByCustomerSignals();
    };

    const initTabUI = function () {
        formContext.ui.tabs.get("LineAndAllowedTab").addTabStateChange(handleUIByLineAndAllowedTab);
    };

    const handleUIByLineAndAllowedTab = function () {
        if (IsLineAndAllowedTabNotification) {
            formContext.ui.clearFormNotification("LineAndAllowedTabNotification");
            IsLineAndAllowedTabNotification = false;
        } else {
            formContext.ui.setFormNotification(LineAndAllowedTabNotification, notificationLevel.Warning, "LineAndAllowedTabNotification");
            IsLineAndAllowedTabNotification = true;
        }
    };

    const handlePopupByCustomerSignals = function () {

        const defultAccountBit = formContext.getAttribute(formAttributes.alt_defultaccountbit).getValue();
        const customerSignals = formContext.getAttribute(formAttributes.alt_customersignals).getValue();
        const hasCustomerSignals = customerSignals != null && customerSignals.trim() !== "";
        let message = NoticeMessage;

        if (defultAccountBit === true) {
            message += "\n" + PreferredPortfolioMessage;
        }

        if (hasCustomerSignals) {
            message += "\n" + customerSignals;
        }

        if (defultAccountBit === true || hasCustomerSignals) {
            Xrm.Navigation.openAlertDialog({ text: message });
        }
    };

    return {
        OnLoad: onLoad
    };
})();
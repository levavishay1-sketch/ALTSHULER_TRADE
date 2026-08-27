/// <reference path="../utils/Utils.Enums.js" />

var DigitalFormRibbonBL = (function () {

    let formContext;


    const sendDigitalFormLinkRequest = function (primaryControl) {
        formContext = primaryControl ? primaryControl : Xrm.Page;

        let statusCode = formContext.getAttribute("alt_transfertooutsystemstatuscode");
        statusCode.setValue(transferStatusCode.Send);

        formContext.data.save().then(function () {
            statusCode = formContext.getAttribute("alt_transfertooutsystemstatuscode");
            if (statusCode.getValue() === transferStatusCode.Sending) {
                Xrm.Utility.showProgressIndicator(LINK_REQUEST_NOTIFICATION_MESSAGE);
                refreshForm();
            }
        },
            function (error) {
                console.log(error);
            });
    };

    const dataConstructionRetryOnClick = function (primaryControl) {
        formContext = primaryControl ? primaryControl : Xrm.Page;

        let dataReceptionStatus = formContext.getAttribute("alt_datareceptionstatuscode");
        dataReceptionStatus.setValue(dataReceptionStatusCode.Retry);

        formContext.data.save().then(function () {
            dataReceptionStatus = formContext.getAttribute("alt_datareceptionstatuscode");
            if (dataReceptionStatus.getValue() === dataReceptionStatusCode.UnderConstruction) {
                Xrm.Utility.showProgressIndicator(DATA_CONSTRUCTION_NOTIFICATION_MESSAGE);
                refreshFormAfterDataConstructionRetry();
            }
        },
            function (error) {
                console.log(error);
            });
    };

    const refreshFormAfterDataConstructionRetry = function () {
        formContext.data.refresh(false);
        var statusCodeValue = formContext.getAttribute("alt_datareceptionstatuscode").getValue();
        if (statusCodeValue == dataReceptionStatusCode.Faild || statusCodeValue == dataReceptionStatusCode.Success) {
            Xrm.Utility.closeProgressIndicator();
            return;
        }
        setTimeout(refreshForm, 2000);
    };

    const refreshForm = function () {
        formContext.data.refresh(false);
        var statusCodeValue = formContext.getAttribute("alt_transfertooutsystemstatuscode").getValue();
        if (statusCodeValue !== transferStatusCode.Sending) {
            Xrm.Utility.closeProgressIndicator();
            if (statusCodeValue == transferStatusCode.Faild) {
                Xrm.Navigation.openAlertDialog({ text: 'בקשה לקבלת לינק לטופס דיגיטלי נכשלה.' });
            }
            return;
        }
        setTimeout(refreshForm, 2000);
    };

    return {
        SendDigitalFormLinkRequest: sendDigitalFormLinkRequest,
        DataConstructionRetryOnClick: dataConstructionRetryOnClick
    };

}());
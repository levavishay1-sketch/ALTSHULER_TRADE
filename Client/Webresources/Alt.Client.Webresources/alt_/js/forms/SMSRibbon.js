var SMSRibbonBL = (function () {
    let formContext;

    const CANT_SEND_SMS_WITHOUT_BODY = 'לא ניתן לבצע שליחה ללא תיאור';
    const CANT_SEND_SMS_WITHOUT_MOBILEPHONE = 'לא ניתן לבצע שליחה ללא טלפון נייד';

    const sendSms = function (primaryControl) {
        formContext = primaryControl ? primaryControl : Xrm.Page;
        if (formContext.getAttribute('alt_mobilephone').getValue()) {
            if (formContext.getAttribute('description').getValue()) {
                let statusCode = formContext.getAttribute("statuscode");
                statusCode.setValue(smsStatus.Send);

                formContext.data.save().then(function () {
                    statusCode = formContext.getAttribute("statuscode");
                    if (statusCode.getValue() === smsStatus.SendingNow) {
                        Xrm.Utility.showProgressIndicator(SENDING_NOW_SMS_NOTIFICATION_MESSAGE);
                        refreshForm();
                    }
                },
                    function (error) {
                        console.log(error);
                    });
            } else {
                Xrm.Navigation.openAlertDialog({ text: CANT_SEND_SMS_WITHOUT_BODY });
            }
        } else {
            Xrm.Navigation.openAlertDialog({ text: CANT_SEND_SMS_WITHOUT_MOBILEPHONE });
        }
    };

    const refreshForm = function () {
        formContext.data.refresh(false);
        var statusCodeValue = formContext.getAttribute("statuscode").getValue();
        if (statusCodeValue !== smsStatus.SendingNow) {
            Xrm.Utility.closeProgressIndicator();
            return;
        }

        setTimeout(refreshForm, 2000);
    };

    return {
        SendSms: sendSms
    };

}());
if (typeof (Utils) == "undefined")
    Utils = {};

Utils.Validators = (function () {

    const isValidMobileNumber = function (phoneNumber) {

        const reg = /^0(5[012345689]){1}(\-)?[^0\D]{1}\d{6}$/;
        if (phoneNumber !== null && !reg.test(phoneNumber)) {
            return false;
        }
        return true;
    };

    const isValidPhoneNumber = function (phoneNumber) {
        const reg = /^0(5[012345689]|7[12346789]|[23489]){1}(\-)?[^0\D]{1}\d{6}$/;
        if (phoneNumber !== null && !reg.test(phoneNumber)) {
            return false;
        }
        return true;
    };

    const isValidLandlinePhoneNumber = function (phoneNumber) {
        const reg = /^0(7[12346789]|[23489]){1}(\-)?[^0\D]{1}\d{6}$/;
        if (phoneNumber !== null && !reg.test(phoneNumber)) {
            return false;
        }
        return true;
    };

    const isValidEmailAddress = function (emailAddress) {
        const reg = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        if (emailAddress !== null && !reg.test(emailAddress)) {
            return false;
        }
        return true;
    };

    const isValidGovId = function (govId) {

        if (Utils.JsExtantions.String.IsNullOrEmpty(govId) || (govId.length > 9) || (govId.length < 5)) {
            return false;
        }

        if (govId.length < 9) {
            while (govId.length < 9) {
                govId = '0' + govId;
            }
        }

        let mone = 0;
        for (let i = 0; i < 9; i++) {
            let incNum = Number(govId.charAt(i));
            incNum *= (i % 2) + 1;
            if (incNum > 9)
                incNum -= 9;
            mone += incNum;
        }

        return (mone % 10 === 0);
    };

    const isValidAccountNumber = function (accountNumber) {

        if (accountNumber !== null && (accountNumber.length != 9 || Number(accountNumber) == false || Number(accountNumber.charAt(0)) != 5)) {
            return false;
        }
        return true;
    };

    const isOnlyDigitsAndEnglishLetters = function (textToEvaluate) {
        const reg = /^[a-z0-9 ]+$/i;

        if (textToEvaluate !== null && !reg.test(textToEvaluate)) {
            return false;
        }
        return true;
    };

    const isOnlyEnglishDigitsInLowcase = function (textToEvaluate) {
        const reg = /^[a-z]+$/;
        if (textToEvaluate !== null && !reg.test(textToEvaluate)) {
            return false;
        }
        return true;
    };

    return {
        IsValidMobileNumber: isValidMobileNumber,
        IsValidPhoneNumber: isValidPhoneNumber,
        IsValidEmailAddress: isValidEmailAddress,
        IsValidGovId: isValidGovId,
        IsValidAccountNumber: isValidAccountNumber,
        IsOnlyDigitsAndEnglishLetters: isOnlyDigitsAndEnglishLetters,
        IsValidLandlinePhoneNumber: isValidLandlinePhoneNumber,
        IsOnlyEnglishDigitsInLowcase: isOnlyEnglishDigitsInLowcase
    };

})(window.Utils.Validators = window.Utils.Validators || {});
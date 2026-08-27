var PortfolioRibbonBL = (function () {

    let formContext;
    let accountHolders;
    let selectedAccountHolder;

    const EMPTY_MOBILE_PHONE_ERROR_MESSAGE = 'יש להשלים מספר טלפון נייד, לצורך ביצוע אימות ב- SMS';
    const EMPTY_EMAIL_ERROR_MESSAGE = 'יש להשלים כתובת דוא"ל, לצורך ביצוע אימות בדוא"ל';

    const sendOTPCodeOnClick = function (primaryControl, activityTemplateType) {

        formContext = primaryControl;

        const select = 'alt_accountholderid, alt_email, alt_mobilephone, _alt_customerid_value';
        let formId = formContext.data.entity.getId();
        let filter = "alt_PortfolioId/alt_portfolioid eq " + Utils.JsExtantions.String.RemoveBraces(formId).toLowerCase() + " and statuscode eq 1";
        Utils.Server.RetrieveMultiple("alt_accountholder", select, filter, null, null, function (result) {
            if (result) {
                accountHolders = result;
                showLookupObjects(result, activityTemplateType, showLookupObjectsSuccessCallback);
            }
            else {
                Xrm.Navigation.openAlertDialog({ text: "לא קיימים לחשבון בעלי חשבון פעילים." });
                Xrm.Utility.closeProgressIndicator();
            }
        }, function (error) {
            Xrm.Navigation.openAlertDialog({ text: 'לא הייתה אפשרות לבצע את הפעולה, אנא פנה למנהל מערכת.' });
            Xrm.Utility.closeProgressIndicator();
            console.log(error);
        });
    };

    const showLookupObjects = function (result, activityType, showLookupObjectsSuccessCallback) {
        var lookupOptions = {};
        lookupOptions.allowMultiSelect = false;
        lookupOptions.disableMru = true;
        lookupOptions.defaultEntityType = "alt_accountholder";
        lookupOptions.entityTypes = ["alt_accountholder"];
        let filterConditions = [];
        result.forEach(function (accountHolder) {
            filterConditions.push('<condition attribute="alt_accountholderid" operator="eq" uitype="alt_accountholder" value="{' + accountHolder.alt_accountholderid + '}"/>');
        });

        lookupOptions.filters = [{
            filterXml: '<filter type="or">' + filterConditions.join('') + ' </filter>',
            entityLogicalName: "alt_accountholder"
        }];
        Xrm.Utility.lookupObjects(lookupOptions).
            then(function (selectedAccountHolder) {
                showLookupObjectsSuccessCallback(selectedAccountHolder, activityType);
            }, function (e) {
                console.log(e.error.message);
            });
    };

    const showLookupObjectsSuccessCallback = function (selectedAccountHolders, activityType) {

        if (selectedAccountHolders !== null && selectedAccountHolders.length > 0) {

            const selectedCustomer = selectedAccountHolders[0];
            const to = getContactingDetails(selectedCustomer, activityType);
            if (isContactingDetalisValid(to, activityType)) {

                const regardingObjectId = {
                    entityType: formContext.data.entity.getEntityName(),
                    id: Utils.JsExtantions.String.RemoveBraces(formContext.data.entity.getId())
                };
                const retrievedCustomer = selectedAccountHolder.alt_customerid;
                const customerId = generateLookupValue(retrievedCustomer.Id, retrievedCustomer.LogicalName, retrievedCustomer.Name);
                const parserCustomEntryPoint = Utils.Global.GenerateParserCustomEntryPointEntityReference(selectedCustomer.entityType, Utils.JsExtantions.String.RemoveBraces(selectedCustomer.id));
                CustomerActivitiesCommonBL.SendOTPCode(formContext, activityType, to, regardingObjectId, parserCustomEntryPoint, customerId);
            }
        }
        else {
            Xrm.Navigation.openAlertDialog({ text: 'לא נבחר אף בעל חשבון.' });
        }
    };

    const getContactingDetails = function (accountHolder, activityType) {

        const contactingAttributeName = activityType == activityTemplateType.Sms ? 'alt_mobilephone' : 'alt_email';
        const filteredAccountHolders = accountHolders.filter(function (value) {

            return value.alt_accountholderid.toLowerCase() == Utils.JsExtantions.String.RemoveBraces(accountHolder.id).toLowerCase();
        });
        selectedAccountHolder = filteredAccountHolders[0];

        return selectedAccountHolder[contactingAttributeName];
    };

    const isContactingDetalisValid = function (to, activityType) {

        let isValid = false;
        if (to) {
            isValid = true;
        }
        else {
            const errorMessage = activityType == activityTemplateType.Sms ?
                EMPTY_MOBILE_PHONE_ERROR_MESSAGE : EMPTY_EMAIL_ERROR_MESSAGE;
            Xrm.Navigation.openAlertDialog({ text: errorMessage });
        }
        return isValid;
    };

    const generateLookupValue = function (valueId, valueEntityType, valueName) {

        return {
            id: valueId,
            entityType: valueEntityType,
            name: valueName
        }
    };

    const sendOTPEnableRule = function (primaryControl) {
        formContext = primaryControl;
        return true;
    };

    return {
        SendOTPEnableRule: sendOTPEnableRule,
        SendOTPCodeOnClick: sendOTPCodeOnClick
    };

}());
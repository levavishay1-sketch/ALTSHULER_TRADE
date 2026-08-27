var ContactMain = (function () {

    let formContext;
    const formAttributes = {
        governmentid: 'governmentid'

    };

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();
        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;
            switch (formType) {
                case crmFormTypes.Create: {
                    handlePopUp();
                    break;
                }
                case crmFormTypes.Update: {
                    initOnChange();
                    break;
                }
                default: {
                    break;
                }
            }
        }
    };

    const initOnChange = function () {

        formContext.getAttribute(formAttributes.governmentid).addOnChange(governmentIdOnChange);
    };

    const governmentIdOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();

    };

    const handlePopUp = function () {

        const queryStringParameters = Xrm.Utility.getGlobalContext().getQueryStringParameters();
        if (queryStringParameters && queryStringParameters[formAttributes.governmentid]) {
            Utils.Global.GetContactByGovernmentId(queryStringParameters[formAttributes.governmentid], 'contactid', getContactSuccessCallback,
                function () { alert('Opsss!!!') });
        }
    };

    const getContactSuccessCallback = function (result) {
        if (result && result[0]) {
            var pageInput = {
                pageType: "entityrecord",
                entityName: "contact",
                entityId: result[0].contactid
            };
            Xrm.Navigation.navigateTo(pageInput);
        }      
    };

    return {
        OnLoad: onLoad
    }
})();
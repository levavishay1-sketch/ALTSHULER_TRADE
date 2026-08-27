var ScheduledOperationMain = (function () {

    let formContext;
    const RUNNING_SCHEDULED_OPERATION_MESSAGE = 'פעולה מתוזמנת מתבצעת...';
    const scheduledOperationStatusCode = {
        Run: 491170001,
        Running: 491170002
    };

    const formAttributes = {
        alt_schedulersetupid: 'alt_schedulersetupid',
        statuscode: 'statuscode'
    };

    const attributesToChangeDisabledMode = [
        formAttributes.alt_schedulersetupid,
        formAttributes.statuscode
    ];

    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();
        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;
            switch (formType) {
                case crmFormTypes.Create: {
                    Utils.CrmPage.DisableAttributes(formContext, attributesToChangeDisabledMode, false);
                    Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_schedulersetupid, Utils.CrmPage.RequirementLevel.Required);
                    filterSchedulerSetupLookup();
                    break;
                }
                default: {
                    break;
                }
            }
        }
        else {
            refreshForm();
        }
    };

    const onSave = function (executionContext) {

        formContext = executionContext.getFormContext();
        if (formContext.ui.getFormType() == Utils.CrmPage.FormType.Create
            && formContext.getAttribute(formAttributes.statuscode).getValue() == scheduledOperationStatusCode.Run) {
            Xrm.Utility.showProgressIndicator(RUNNING_SCHEDULED_OPERATION_MESSAGE);
        }
    };

    const refreshForm = function () {
        formContext.data.refresh(false);
        var statusCodeValue = formContext.getAttribute(formAttributes.statuscode).getValue();
        if (statusCodeValue !== scheduledOperationStatusCode.Run
            && statusCodeValue !== scheduledOperationStatusCode.Running) {
            Xrm.Utility.closeProgressIndicator();
            return;
        }
        setTimeout(refreshForm, 1000);
    };

    const filterSchedulerSetupLookup = function () {

        const customViewDisplayName = 'הגדרות תזמון';
        const defaultViewId = '{00000000-0000-0000-0000-000000000001}';
        let fetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">' +
            '<entity name="alt_schedulersetup">' +
            '<attribute name="alt_name" />' +
            '<attribute name="alt_schedulersetupid" />' +
            '<order attribute="createdon" descending="false" />' +
            '<filter type="and">' +
            '<condition attribute="alt_userdisplaybit" operator="eq" value="1" />' +
            '</filter>' +
            '</entity>' +
            '</fetch>';

        let layoutXml = '<grid name="resultset" jump="alt_name" select="1" preview="1" icon="1"><row name="result" id="alt_schedulersetupid">' +
            '<cell name="alt_name" width="100"/>' +
            '</row></grid>';

        let lookupControl = formContext.getControl(formAttributes.alt_schedulersetupid);
        lookupControl.addCustomView(defaultViewId, 'alt_schedulersetup', customViewDisplayName, fetchXml, layoutXml, true);
    };

    return {
        OnLoad: onLoad,
        OnSave: onSave
    }
})();
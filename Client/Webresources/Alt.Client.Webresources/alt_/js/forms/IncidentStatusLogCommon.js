/// <reference path="../utils/Utils.CrmPage.js" />

var IncidentStatusLogCommonBL = (function () {
    const NOT_OPENED_FROM_INCIDENT_ALERT_TEXT = 'לא ניתן לשנות מצב שלא מתוך אירוע';

    let formContext;

    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();

        const legalityCreationSetting =
            [
                {
                    conditionCallback: validateStatusLogConditionCallback,
                    errorMessage: NOT_OPENED_FROM_INCIDENT_ALERT_TEXT,
                }
            ];

        const formType = formContext.ui.getFormType();
        const crmFormTypes = Utils.CrmPage.FormType;

        switch (formType) {
            case crmFormTypes.Create: {
                Utils.CrmPage.HandleRecordLegalityCreation(formContext, legalityCreationSetting);
                break;
            }
            default:
                break;
        }
    };

    const validateStatusLogConditionCallback = function () {
        return formContext.getAttribute('alt_incidentid').getValue() && formContext.getAttribute('alt_subject2id').getValue();
    };

    return {
        OnLoad: onLoad
    };
})();


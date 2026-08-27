var PopulationRegistryCustomerVerificationQuickCreate = (function () {
    let formContext;
    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();
        const POPULATION_REGISTRY_VERIFICATION_CONFORM_MESSAGE = 'להמשך תהליך אימות הנתונים ולפניה למרשם האוכלוסין יש ללחוץ על כפתור "אישור". לביטול המשך התהליך יש ללחוץ על כפתור "ביטול".';
        var confirmStrings = {
            text: POPULATION_REGISTRY_VERIFICATION_CONFORM_MESSAGE,
            confirmButtonLabel: 'אישור',
            cancelButtonLabel: 'ביטול'
        };
        var confirmOptions = { height: 150, width: 400 };
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(function (success) {
            if (success.confirmed) {
                PopulationRegistryCustomerVerificationCommonBL.OnLoad(executionContext);
            }
            else {
                formContext.ui.close();
            }
        });       
    };
  
    return {
        OnLoad: onLoad
    };
})();
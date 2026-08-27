var BlacklistsCheckRibbonBL = (function () {

    let formContext;

    const createBlacklistsCheckOnClick = function (primaryControl) {
        formContext = primaryControl;
        createNewBlacklistsCheckForm(formContext);
    };

    const createNewBlacklistsCheckForm = function (formContext) {

        let parentId = formContext?.data?.entity?.getId?.();
        let parentEntityName = formContext?.data?.entity?.getEntityName?.();

        let pageInput = {
            pageType: "entityrecord",
            entityName: "alt_blacklistscheck"
        };

        if (parentId && parentEntityName) {
            pageInput.createFromEntity = {
                id: parentId.replace(/[{}]/g, ""),
                entityType: parentEntityName
            };
        }

        let navigationOptions = {
            target: 2,
            position: 1,
            width: { value: 85, unit: "%" },
            height: { value: 85, unit: "%" }
        };

        Xrm.Navigation.navigateTo(pageInput, navigationOptions);
    };

    const createBlacklistsCheckButtonEnabled = function () {
        return true;
    };

    return {
        CreateBlacklistsCheckOnClick: createBlacklistsCheckOnClick,
        CreateBlacklistsCheckButtonEnabled: createBlacklistsCheckButtonEnabled,
    };

}());
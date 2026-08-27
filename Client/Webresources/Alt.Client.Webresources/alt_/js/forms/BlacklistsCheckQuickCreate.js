/// <reference path="../utils/Utils.Validators.js" />
/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Enums.js" />
/// <reference path="../forms/BlacklistsCheckCommonBL.js" />

var AccountHolderQuickCreate = (function () {

    const onLoad = function (executionContext) {

        BlacklistsCheckCommonBL.OnLoad(executionContext);
    };

    return {
        OnLoad: onLoad
    };
})();
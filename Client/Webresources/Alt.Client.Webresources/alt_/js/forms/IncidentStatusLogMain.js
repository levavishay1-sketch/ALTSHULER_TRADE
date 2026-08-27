/// <reference path="IncidentStatusLogCommon.js" />

var IncidentStatusLogMain = (function () {

    const onLoad = function (executionContext) {

        if (Utils.CrmPage.IsFirstLoad()) {

            IncidentStatusLogCommonBL.onLoad(executionContext);
        }
    };

    return {
        OnLoad: onLoad
    };
})();

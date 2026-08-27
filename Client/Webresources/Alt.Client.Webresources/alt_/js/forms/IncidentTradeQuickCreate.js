/// <reference path="../forms/IncidentTradeCommon.js" />

var IncidentTradeQuickCreate = (function () {

    const onLoad = function (executionContext) {

        IncidentTradeCommonBL.OnLoad(executionContext);
    };
    return {
        OnLoad: onLoad
    };
}())
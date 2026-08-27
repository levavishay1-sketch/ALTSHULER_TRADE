if (typeof (Utils) == "undefined")
    Utils = {};

Utils.JsExtantions = (function () {

    const string = function () {

        const isNullOrEmpty = function (string) {

            return !string || string === "" || string.trim().length === 0;
        };

        const removeBraces = function (string) {

            if (string && string.indexOf('{') !== -1 && string.indexOf('}') !== -1) {
                string = string.replace(/{|}/g, '');
            }

            return string;
        };

        const endsWith = function (string) {
            return this.substr(this.length - string.length) === string;
        };

        const startsWith = function (string) {
            return this.substr(0, string.length) === string;
        };

        const format = function (string) {
            let formatted = string;

            for (let i = 0; i < arguments.length; i++) {
                const regexp = new RegExp('\\{' + i + '\\}', 'gi');
                formatted = formatted.replace(regexp, arguments[i + 1]);
            }
            return formatted;
        };


        return {
            IsNullOrEmpty: isNullOrEmpty,
            RemoveBraces: removeBraces,
            EndsWith: endsWith,
            StartsWith: startsWith,
            Format: format
        };
    }();

    if (!String.prototype.endsWith) {
        String.prototype.endsWith = string.EndsWith;
    }

    if (!String.prototype.startsWith) {
        String.prototype.startsWith = string.StartsWith;
    }

    const getDayDifferenceBetweenDates = function (startDate, dateToSubstract) {
        const msPerDay = 1000 * 60 * 60 * 24;

        return Math.floor((startDate - dateToSubstract) / msPerDay);
    };

    const convertSecondsToMMSSFormat = function (seconds) {
        const mmss = (seconds - (seconds %= 60)) / 60 + (9 < seconds ? ':' : ':0') + seconds;
        if (mmss.indexOf(':') === 1) {
            return '0' + mmss;
        } else {
            return mmss;
        }
    };

    const entity = function () {

        const getEntityPluralName = function (entityName) {

            if (entityName.endsWith('s') || entityName.endsWith('sh') || entityName.endsWith('ch') || entityName.endsWith('x') || entityName.endsWith('z')) {

                return entityName + 'es';
            }

            if (entityName.endsWith('y')) {

                return entityName.substr(0, entityName.length - 1) + 'ies';
            }

            return entityName + 's';
        };

        return {
            GetEntityPluralName: getEntityPluralName
        };
    }();

    return {
        String: string,
        GetDayDifferenceBetweenDates: getDayDifferenceBetweenDates,
        ConvertSecondsToMMSSFormat: convertSecondsToMMSSFormat,
        Entity: entity
    };

})(window.Utils.JsExtantions = window.Utils.JsExtantions || {});

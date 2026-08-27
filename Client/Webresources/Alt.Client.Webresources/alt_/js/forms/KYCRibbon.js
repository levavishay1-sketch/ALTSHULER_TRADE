/// <reference path="../utils/Utils.Enums.js" />
/// <reference path="../utils/Utils.JsExtantions.js" />
/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Server.js" />

var KYCRibbonBL = (function () {

    const MESSAGE_CANT_DUPLICATE_KYC_MUST_FILL_LAUNDERING_CALCULATION = "לא ניתן לבצע שיכפול - ראשית יש להשלים את החישוב בטופס הנוכחי, לאחר מכן ניתן לשכפל לפי הצורך";

    let formContext;
	const duplicateFormOnClick = function (primaryControl) {

		formContext = primaryControl ? primaryControl : Xrm.Page;
        if (formContext.data.entity.getIsDirty()) {
            formContext.data.save().then(function () {

                openDuplicatedFormIfHasAnyMoneyLaunderingCalculation();
            });
        }
        else {
            openDuplicatedFormIfHasAnyMoneyLaunderingCalculation();
        }
    };

    const openDuplicatedFormIfHasAnyMoneyLaunderingCalculation = function () {

        let kycId = Utils.JsExtantions.String.RemoveBraces(formContext.data.entity.getId());
        let filter = `_alt_kycid_value eq ${kycId}`;
        Utils.Server.RetrieveMultiple('alt_moneylaunderingcalculation', 'alt_moneylaunderingcalculationid', filter, null, null, function (receivedData) {
            if (receivedData?.length > 0) {

                openDuplicatedform();
            } else {

                Xrm.Navigation.openAlertDialog({ text: MESSAGE_CANT_DUPLICATE_KYC_MUST_FILL_LAUNDERING_CALCULATION });
            }
        });
    };

    const openDuplicatedform = function () {

        var pageInput = {

            createFromEntity: {
                entityType: formContext.data.entity.getEntityName(),
                id: formContext.data.entity.getId().replace(/{|}/g, '')
            },
            pageType: "entityrecord",
            entityName: "alt_kyc"
        };
        var navigationOptions = {
            target: 2,
            height: { value: 95, unit: "%" },
            width: { value: 95, unit: "%" },
            position: 1
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions);
      
    };

    const isDuplicateFormButtonEnabled = function () {

        return true;
    };

    return {
        DuplicateFormOnClick: duplicateFormOnClick,
        IsDuplicateFormButtonEnabled: isDuplicateFormButtonEnabled
    };
})();
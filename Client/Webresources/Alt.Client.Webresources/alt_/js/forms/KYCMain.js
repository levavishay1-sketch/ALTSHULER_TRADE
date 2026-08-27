/// <reference path="../utils/Utils.Enums.js" />
/// <reference path="../utils/Utils.JsExtantions.js" />
/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Server.js" />

var KYCMain = (function () {

    const NO_MATCH_BETWEEN_DEPOSITS_AND_WITHDRAWALS = 'אין התאמה בין סכום הפקדות וסכום משיכות';
    const NO_MATCH_BETWEEN_DEPOSIT_FREQUENCY_AND_AMOUNT = 'אין התאמה בין סכום הפקדות ותדירות הפקדה';
    const EMPTY_RELATED_PORTFOLIO_IDENTITY_NUMBER = "על מנת להציג את החשבונות הקשורים יש למלא מספר זהות של חשבון קשור הקיים במערכת וללחוץ על \"שמור\"";

    const totalDepositForecastPerYearCode = {
        BetweenZeroAndFiftyThousand: 1,
        TwoHundredAndFiftyThousandAndAbove: 4,
    };

    const yearlyTotalWithdrawalTransferForecastCode = {
        TwoHundredAndFiftyThousandAndAbove: 4,
    };

    const fundsDepositFrequencyForecastCode = {
        OnceAMonth: 2,
    };

    const employmentTypeCode = {
        Independent: 1,
        Employee: 2,
        CompanyOwner: 3,
        Else: 8,
    };

    const fundsSourceCodeGlobal = {
        Inheritance: 7,
        Gift: 8,
        ForeignTerritory: 11,
        Else: 13,
    };

    const employmentCategoryOccupationId = {
        Other: 'אחר'
    };

    const manualHandlingReasonsCode = {
        RepresentativeDecision: 8
    };

    const formAttributes = {

        alt_employmenttypecode: 'alt_employmenttypecode',
        alt_traderelationriskterritorybit: 'alt_traderelationriskterritorybit',
        alt_publicpersonbit: 'alt_publicpersonbit',
        alt_additionalaccountexistsataltshulerbit: 'alt_additionalaccountexistsataltshulerbit',
        alt_fundssourcecode: 'alt_fundssourcecode',
        alt_transactionstofromthirdpartybit: 'alt_transactionstofromthirdpartybit',
        alt_employmentcategoryoccupationid: 'alt_employmentcategoryoccupationid',
        alt_scoressectioninternalbit: 'alt_scoressectioninternalbit',
        alt_performcalculationswithnochanges: 'alt_performcalculationswithnochanges',
        alt_employmentcategorydesc: 'alt_employmentcategorydesc',
        alt_thirdpartyordepositdesc: 'alt_thirdpartyordepositdesc',
        alt_fundssourceprivate: 'alt_fundssourceprivate',
        alt_additionalaccountdetails: 'alt_additionalaccountdetails',
        alt_publicpersonrole: 'alt_publicpersonrole',
        alt_relationtopublicperson: 'alt_relationtopublicperson',
        alt_financialresourcesource: 'alt_financialresourcesource',
        alt_traderelationdesc: 'alt_traderelationdesc',
        alt_traderelationriskcountryid: 'alt_traderelationriskcountryid',
        alt_workplacename: 'alt_workplacename',
        alt_workplacerole: 'alt_workplacerole',
        alt_businessname: 'alt_businessname',
        alt_parentkycid: 'alt_parentkycid',
        alt_transactionsrelationtofromthirdparty: 'alt_transactionsrelationtofromthirdparty',
        alt_fundssourcefinancial: 'alt_fundssourcefinancial',
        alt_fundssourcescoreint: 'alt_fundssourcescoreint',
        alt_otheremploymentdesc: 'alt_otheremploymentdesc',
        alt_manualhandlingrequiredbit: 'alt_manualhandlingrequiredbit',
        alt_manualhandlingreasonscode: 'alt_manualhandlingreasonscode',
        alt_totaldepositforecastperyearcode: 'alt_totaldepositforecastperyearcode',
        alt_yearlytotalwithdrawaltransferforecastcode: 'alt_yearlytotalwithdrawaltransferforecastcode',
        alt_fundsdepositfrequencyforecastcode: 'alt_fundsdepositfrequencyforecastcode',
        alt_relatedportfoliocustomerid: 'alt_relatedportfoliocustomerid',
        alt_relatedportfolioidentitynumber: 'alt_relatedportfolioidentitynumber'
    };

    const tradeRelationRiskTerritoryAttributes = [
        formAttributes.alt_traderelationdesc,
        formAttributes.alt_traderelationriskcountryid
    ];

    const publicPersonAttributes = [
        formAttributes.alt_publicpersonrole,
        formAttributes.alt_relationtopublicperson,
        formAttributes.alt_financialresourcesource
    ];

    const soureCodeRequired = [fundsSourceCodeGlobal.Inheritance, fundsSourceCodeGlobal.Gift, fundsSourceCodeGlobal.Else];
    const alertScoreTheCalculator = 'לתשומת ליבך- נעשה שינוי בשדות הציונים, ביצוע השמירה תייצר רשומה חדשה של סיכון הלבנת הון';
    const alertScoreNoChangeCalculator = 'לתשומת ליבך- ביצוע השמירה תייצר רשומה חדשה של סיכון הלבנת הון ללא שינויים בציונים';
    const moneyLaunderingCalculationGridSectionName = 'MoneyLaunderingCalculationGridSection';
    const scoreTheCalculatorSectionName = 'ScoreTheCalculatorSection';
    const timeLineSectionName = 'TimeLineSection';
    const questionsForAccountOwnerSectionName = 'QuestionsForAccountOwnerSection';
    const accountLevelQuestionsSectionName = 'AccountLevelQuestionsSection';
    const generalTabName = 'GeneralTab';
    const moneyLaunderingCalculationGridName = 'MoneyLaunderingCalculationGrid';

    const sectionsToEnableAttributes = [
        questionsForAccountOwnerSectionName,
        accountLevelQuestionsSectionName
    ];
    const sectionsToToggle = [
        moneyLaunderingCalculationGridSectionName,
        scoreTheCalculatorSectionName,
        timeLineSectionName
    ];

    let notificationPopup = true;
    let formContext;
    let isQuestionsDesabled;

    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();

        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;

            switch (formType) {
                case crmFormTypes.Create: {

                    initOnChangeOnCreate();
                    initOnChange();
                    initFormUIOnCreate();
                    formContext.data.entity.addOnPostSave(onPostSave);
                    break;
                }
                case crmFormTypes.Update: {

                    showScoreSections();
                    initOnChange();
                    initFormUI();
                    formContext.data.entity.addOnPostSave(onPostSave);
                    break;
                }
                default: {
                    showScoreSections();
                    initFormUI();
                    break;
                }
            }

        }
        else {
            reload();
        }
    };

    const onSave = function (executionContext) {

        formContext = executionContext.getFormContext();
        if (formContext.ui.getFormType() == Utils.CrmPage.FormType.Create) {

            formContext.getAttribute(formAttributes.alt_manualhandlingrequiredbit).removeOnChange(manualHandlingRequiredBitOnChange);
        }

        if (notificationPopup) {
            notificationPopupHandlerOnSave(executionContext);
        }
        else {
            notificationPopup = true;
        }
    };

    const onPostSave = function () {
        showEmptyRelatedpPortfolioCustomerIdNotification();
    };

    const reload = function () {

        Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.alt_manualhandlingrequiredbit, true);
        showScoreSections();
        handleUIByPerformCalculationsWithNoChanges();
    };

    const initOnChange = function () {

        formContext.getAttribute(formAttributes.alt_employmenttypecode).addOnChange(employmentTypeCodeOnChange);
        formContext.getAttribute(formAttributes.alt_traderelationriskterritorybit).addOnChange(tradeRelationRiskTerritoryBitOnChange);
        formContext.getAttribute(formAttributes.alt_publicpersonbit).addOnChange(publicPersonBitOnChange);
        formContext.getAttribute(formAttributes.alt_additionalaccountexistsataltshulerbit).addOnChange(additionalAccountExistsAtAltshulerBitOnChange);
        formContext.getAttribute(formAttributes.alt_fundssourcecode).addOnChange(fundsSourceCodeOnChange);
        formContext.getAttribute(formAttributes.alt_transactionstofromthirdpartybit).addOnChange(transactionsToFromThirdPartyBitOnChange);
        formContext.getAttribute(formAttributes.alt_employmentcategoryoccupationid).addOnChange(employmentCategoryOccupationIdOnChange);
    };

    const initOnChangeOnCreate = function () {

        formContext.getAttribute(formAttributes.alt_manualhandlingrequiredbit).addOnChange(manualHandlingRequiredBitOnChange);
    };

    const initFormUIOnCreate = function () {

        enableQuestionsAttributes();
        Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.alt_manualhandlingrequiredbit, false);
        initFormUI();
    };

    const initFormUI = function () {

        handleUIByEmploymentTypeCode();
        handleUIByTradeRelationRiskTerritoryBit();
        handleUIByPublicPersonBit();
        handleUIByFundsSourceCode();
        handleUIByTransactionsToFromThirdPartyBit();
        handleUIByEmploymentCategoryOccupationId();
        handleUIByPerformCalculationsWithNoChanges();
        handleUIByAdditionalAccountDetails();
        showDepositMismatchNotifications();
        showEmptyRelatedpPortfolioCustomerIdNotification();
    };

    const employmentTypeCodeOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        handleUIByEmploymentTypeCode();
    };

    const tradeRelationRiskTerritoryBitOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        handleUIByTradeRelationRiskTerritoryBit();
    };

    const publicPersonBitOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        handleUIByPublicPersonBit();
    };

    const additionalAccountExistsAtAltshulerBitOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
    };

    const fundsSourceCodeOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        handleUIByFundsSourceCode();
    };

    const transactionsToFromThirdPartyBitOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        handleUIByTransactionsToFromThirdPartyBit();
    };

    const employmentCategoryOccupationIdOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        handleUIByEmploymentCategoryOccupationId();
    };

    const manualHandlingRequiredBitOnChange = function (executionContext) {

        formContext = executionContext.getFormContext();
        setManualHandlingReasonsCode();
    };

    const moneyLaunderingCalculationGridOnLoad = function (executionContext) {

        formContext = executionContext.getFormContext();

        const gridContext = formContext.getControl(moneyLaunderingCalculationGridName);
        let recordCount = gridContext.getGrid().getTotalRecordCount();
        if (recordCount > 0) {

            if (!formContext.getAttribute(formAttributes.alt_parentkycid).getValue()) {

                Utils.CrmPage.DisableSectionAttributesMode(formContext, generalTabName, scoreTheCalculatorSectionName, true);
            }
            else {
                Utils.CrmPage.DisableAllFormFields(formContext);
            }
        }
        else if ((formContext.getAttribute(formAttributes.alt_parentkycid).getValue())
            && !isQuestionsDesabled) {

            enableQuestionsAttributes();
        }
    };

    const handleUIByEmploymentTypeCode = function () {

        let isWorkplaceNameVisible = false;
        let isWorkPlaceRoleVisible = false;
        let isBusinessNameVisible = false;
        let otherEmploymentDescRequirementLevel = Utils.CrmPage.RequirementLevel.None;

        const employmentTypeCodeValue = formContext.getAttribute(formAttributes.alt_employmenttypecode).getValue();
        switch (employmentTypeCodeValue) {
            case employmentTypeCode.Employee: {

                isWorkplaceNameVisible = true;
                isWorkPlaceRoleVisible = true;
                break;
            }
            case employmentTypeCode.Independent:
            case employmentTypeCode.CompanyOwner: {

                isBusinessNameVisible = true;
                break;
            }
            case employmentTypeCode.Else: {

                otherEmploymentDescRequirementLevel = Utils.CrmPage.RequirementLevel.Required;
                break;
            }
            default: {
                break;
            }
        }

        Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_workplacename, isWorkplaceNameVisible, true);
        Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_workplacerole, isWorkPlaceRoleVisible, true);
        Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_businessname, isBusinessNameVisible, true);

        const workPlaceNameRequiredLevel = isWorkplaceNameVisible ?
            Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.None;
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_workplacename, workPlaceNameRequiredLevel);

        const businessNameRequiredLevel = isBusinessNameVisible ?
            Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.None;
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_businessname, businessNameRequiredLevel);

        const employmentcategoryoccupationRequiredLevel = isBusinessNameVisible || isWorkplaceNameVisible || isWorkPlaceRoleVisible ?
            Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.None;
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_employmentcategoryoccupationid, employmentcategoryoccupationRequiredLevel);

        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_otheremploymentdesc, otherEmploymentDescRequirementLevel);

    };

    const handleUIByTradeRelationRiskTerritoryBit = function () {

        const requirementLevel = formContext.getAttribute(formAttributes.alt_traderelationriskterritorybit).getValue() == true ?
            Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.None;

        tradeRelationRiskTerritoryAttributes.forEach(function (attributeName) {
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, attributeName, requirementLevel);
        });
    };

    const handleUIByPublicPersonBit = function () {

        const requirementLevel = formContext.getAttribute(formAttributes.alt_publicpersonbit).getValue() == true ?
            Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.None;

        publicPersonAttributes.forEach(function (attributeName) {
            Utils.CrmPage.SetAttributeRequiredLevel(formContext, attributeName, requirementLevel);
        });
    };

    const handleUIByFundsSourceCode = function () {

        const fundsSourceCodeValue = formContext.getAttribute(formAttributes.alt_fundssourcecode).getValue();
        let fundsSourcePrivateRequirementLevel = fundsSourceCodeValue
            && fundsSourceCodeValue.some(x => soureCodeRequired.includes(x)) ?
            Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.None;
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_fundssourceprivate, fundsSourcePrivateRequirementLevel);

        let fundsSourceFinancialRequirementLevel = fundsSourceCodeValue
            && fundsSourceCodeValue.includes(fundsSourceCodeGlobal.ForeignTerritory) ?
            Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.None;
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_fundssourcefinancial, fundsSourceFinancialRequirementLevel);

        let fundsSourceScoreIntRequirementLevel = fundsSourceCodeValue
            && fundsSourceCodeValue.includes(fundsSourceCodeGlobal.Else) ?
            Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.None;
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_fundssourcescoreint, fundsSourceScoreIntRequirementLevel);
    };

    const handleUIByTransactionsToFromThirdPartyBit = function () {

        const requirementLevel = formContext.getAttribute(formAttributes.alt_transactionstofromthirdpartybit).getValue() == true ?
            Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.None;

        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_thirdpartyordepositdesc, requirementLevel);
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_transactionsrelationtofromthirdparty, requirementLevel);
    };

    const handleUIByEmploymentCategoryOccupationId = function () {

        const isEmploymentCategoryDescVisible = formContext.getAttribute(formAttributes.alt_employmentcategoryoccupationid).getValue() != null
            && formContext.getAttribute(formAttributes.alt_employmentcategoryoccupationid).getValue()[0].name == employmentCategoryOccupationId.Other;
        const requirementLevel = isEmploymentCategoryDescVisible ?
            Utils.CrmPage.RequirementLevel.Required : Utils.CrmPage.RequirementLevel.None;

        Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_employmentcategorydesc, isEmploymentCategoryDescVisible, true);
        Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_employmentcategorydesc, requirementLevel);
    };

    const handleUIByPerformCalculationsWithNoChanges = function () {

        var kycId = formContext.data.entity.getId();
        var fetchXml = `
            <fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                <entity name="alt_moneylaunderingcalculation">
                    <attribute name="alt_moneylaunderingcalculationid"/>
                    <order attribute="alt_name" descending="false"/>
                    <filter type="and">
                        <condition attribute="alt_kycid" value="${kycId}" operator="eq"/>
                    </filter>
                </entity>
            </fetch>
        `;

        Utils.Server.Fetch(
            'alt_moneylaunderingcalculation',
            fetchXml,
            function (result) {
                if (result.length > 0) {
                    formContext.getControl(formAttributes.alt_performcalculationswithnochanges).setDisabled(true);
                }
            },
            null
        )
    };

    const notificationPopupHandlerOnSave = function (executionContext) {

        let columnsScoreTheCalculatorSection = formContext.ui.tabs.get(generalTabName).sections
            .get(scoreTheCalculatorSectionName).controls.get()
            .filter((column) => column.getName() != formAttributes.alt_performcalculationswithnochanges);

        let isDirty = columnsScoreTheCalculatorSection.some((column) => column.getAttribute().getIsDirty());
        var alertStrings = { text: alertScoreTheCalculator };
        var alertStringsNoChange = { text: alertScoreNoChangeCalculator };

        if (isDirty) {
            executionContext.getEventArgs().preventDefault();
            Xrm.Navigation.openConfirmDialog(alertStrings).then(
                function (success) {
                    if (success.confirmed) {
                        notificationPopup = false;
                        formContext.getAttribute(formAttributes.alt_performcalculationswithnochanges).setValue(false);
                        formContext.getControl(formAttributes.alt_performcalculationswithnochanges).setDisabled(true);
                        formContext.getAttribute(formAttributes.alt_scoressectioninternalbit).setValue(true);
                        formContext.getAttribute(formAttributes.alt_scoressectioninternalbit).setSubmitMode(SubmitMode.Always);
                        formContext.data.save()
                            .then(function () {
                                Utils.CrmPage.DisableAllFormFields(formContext);
                                isQuestionsDesabled = true;
                            });
                    }
                }
            );
        }
        else if (formContext.getAttribute(formAttributes.alt_performcalculationswithnochanges).getValue()) {
            executionContext.getEventArgs().preventDefault();
            Xrm.Navigation.openConfirmDialog(alertStringsNoChange).then(
                function (success) {
                    if (success.confirmed) {
                        notificationPopup = false;
                        formContext.getControl(formAttributes.alt_performcalculationswithnochanges).setDisabled(true);
                        formContext.getAttribute(formAttributes.alt_scoressectioninternalbit).setValue(true);
                        formContext.getAttribute(formAttributes.alt_scoressectioninternalbit).setSubmitMode(SubmitMode.Always);
                        formContext.data.save()
                            .then(function () {
                                Utils.CrmPage.DisableAllFormFields(formContext);
                                isQuestionsDesabled = true;
                            });
                    }
                }
            );
        }
    };

    const showScoreSections = function () {

        formContext.getControl(moneyLaunderingCalculationGridName).addOnLoad(moneyLaunderingCalculationGridOnLoad);
        sectionsToToggle.forEach(function (sectionName) {

            Utils.CrmPage.SetSectionVisibleMode(formContext, generalTabName, sectionName, true);
        });
    };

    const enableQuestionsAttributes = function () {

        sectionsToEnableAttributes.forEach(function (sectionName) {
            Utils.CrmPage.DisableSectionAttributesMode(formContext, generalTabName, sectionName, false);

        });
    };

    const setManualHandlingReasonsCode = function () {

        const value = formContext.getAttribute(formAttributes.alt_manualhandlingrequiredbit).getValue() == true ?
            [manualHandlingReasonsCode.RepresentativeDecision] : null;

        formContext.getAttribute(formAttributes.alt_manualhandlingreasonscode).setValue(value);
    };

    const showDepositMismatchNotifications = function () {

        const fundsDepositFrequencyForecastCodeValue = formContext.getAttribute(formAttributes.alt_fundsdepositfrequencyforecastcode).getValue(); // 8
        const totalDepositForecastPerYearCodeValue = formContext.getAttribute(formAttributes.alt_totaldepositforecastperyearcode).getValue(); // 9
        const yearlyTotalWithdrawalTransferForecastCodeValue = formContext.getAttribute(formAttributes.alt_yearlytotalwithdrawaltransferforecastcode).getValue(); // 11

        if (totalDepositForecastPerYearCodeValue == totalDepositForecastPerYearCode.BetweenZeroAndFiftyThousand
            && yearlyTotalWithdrawalTransferForecastCodeValue == yearlyTotalWithdrawalTransferForecastCode.TwoHundredAndFiftyThousandAndAbove) {
            formContext.ui.setFormNotification(NO_MATCH_BETWEEN_DEPOSITS_AND_WITHDRAWALS, notificationLevel.Warning);
        }

        if (fundsDepositFrequencyForecastCodeValue == fundsDepositFrequencyForecastCode.OnceAMonth
            && totalDepositForecastPerYearCodeValue == totalDepositForecastPerYearCode.TwoHundredAndFiftyThousandAndAbove) {
            formContext.ui.setFormNotification(NO_MATCH_BETWEEN_DEPOSIT_FREQUENCY_AND_AMOUNT, notificationLevel.Warning);
        }
    };

    const handleUIByAdditionalAccountDetails = function () {

        const additionalAccountDetails = formContext.getAttribute(formAttributes.alt_additionalaccountdetails);
        if (additionalAccountDetails != null && !Utils.JsExtantions.String.IsNullOrEmpty(additionalAccountDetails.getValue())) {
            Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_additionalaccountdetails, true, false);
        }
        else {
            Utils.CrmPage.SetControlVisibleMode(formContext, formAttributes.alt_additionalaccountdetails, false, false);
        }
    };

    const showEmptyRelatedpPortfolioCustomerIdNotification = function () {
        const relatedPortfolioCustomerId = formContext.getAttribute(formAttributes.alt_relatedportfoliocustomerid);
        if (relatedPortfolioCustomerId == null || relatedPortfolioCustomerId.getValue() == null) {
            formContext.ui.setFormNotification(EMPTY_RELATED_PORTFOLIO_IDENTITY_NUMBER, notificationLevel.Warning, "emptyRelatedPortfolioidentitynumber");
        }
        else {
            formContext.ui.clearFormNotification("emptyRelatedPortfolioidentitynumber");
        }
    };

    return {
        OnLoad: onLoad,
        OnSave: onSave
    };
})();
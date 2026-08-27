/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Validators.js" />
/// <reference path="../utils/Utils.JsExtantions.js" />
/// <reference path="../utils/Utils.Server.js" />
/// <reference path="../utils/Utils.Enums.js" />

var DigitalFormVerificationMain = (function () {

    const COMPARISON_NOTITIFCATION_MESSAGE_WARNING = "אין התאמה בין מספר הזיהוי שהוקלד ע\"י הלקוח למספר זיהוי המקוון";
    const NO_MATCH_BETWEEN_DEPOSITS_AND_WITHDRAWALS = 'אין התאמה בין סכום הפקדות וסכום משיכות';
    const NO_MATCH_BETWEEN_DEPOSIT_FREQUENCY_AND_AMOUNT = 'אין התאמה בין סכום הפקדות ותדירות הפקדה';

    const controlStageTeamId = {
        JoiningControl: "בקרת הצטרפות",
        ManagementControl: "בקרת מנהל/ת",
        MoneyLaunderingControl: "בקרת הלבנת הון",
        OperationalControl: "בקרה תפעולית"
    };
    const initialDepositCode = {
        AcceptedDeposit: 1,
        AwaitinglDeposit: 2,
        CreateAccountWithoutFirstDeposit: 3,
        AcceptedDepositForApproval: 4
    };
    const managerVerificationRequiredCode = {
        Yes: 1,
        No: 2
    };
    const accountHolderTypeCode = {
        AccountHolder: 1
    };
    const userCharacteristicCode = {
        professionalUser: 1
    };
    const digitalVisualRecognitionCode = {
        SoftFailure: 2
    };
    const verificationResultCode = {
        Verified: 1,
        NotVerified: 2
    };
    const iDIssuanceDateVerificationResultCode = {
        Verified: 1,
        NotVerified: 2
    };
    const dataComparisonStatusCode = {
        Match: 1,
        NotMatch: 2
    };
    const beneficieryDeclarationCode = {
        requiredForFurtherCheck: 1,
        Valid: 2,
        Invalid: 3
    };
    const beneficieryRequiredBit = {
        No: false,
        Yes: true
    };
    const terrorOrganizationCheckCode = {
        Valid: 1,
        Invalid: 2
    };
    const calculatedMoneyLaunderingLevelCode = {
        Low: 1,
        Medium: 2,
        High: 3
    };
    const clubMembershipEligibilityCode = {
        UnidentifiedNoEligibility: 0,
        Operation1: 1,
        Operation2: 2
    };
    const comparisonCode = {
        Identical: 1,
        NotIdentical: 2
    };
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

    const notificationIsRequiredBeneficiaryDeclaration = 'לתשומת לבך- נדרשת בקרה על הצהרת נהנה';
    const notificationBirthCountryIsNotIsrael = ' מדינת הלידה אינה ישראל';
    const notificationUsPersonDeclaration = 'קיימת הצהרה professional user עבור ';
    const notificationSoftFailureDigitalVisualRecognition = 'זיהוי אוטנטיקס לא עבר באופן מלא, יש לגשת לקונסול - עבור ';
    const notificationcInvalidPopulationRegister = 'תוצאות בדיקת מרשם האוכלוסין אינן תקינות עבור ';
    const NOTIFICATION_ACCOUNTHOLDER_UNIDENTIFIED_NOELIGIBILITY = 'הגיע במבצע 1 ונכשל בבדיקת מועדונים- להמשך בירור נציג';

    const requiredManagerControlReasonsText = {
        initialCheckOnBeneficiery: 'בקרה ראשונית על הצהרה נהנה אינה תקינה או מצריכה המשך בדיקה',
        beneficieryCheckRequired: 'נדרשת בקרה על הצהרה על נהנה',
        creditLimitRequestExists: 'בקשה למסגרת אשראי',
        missingSellingRequestExists: 'בקשה למכירה בחסר',
        optionsTradingRequestExists: 'בקשה למסחר באופציות',
        moneyLaunderingLevelNotLow: 'דרגת סיכון הלבנת הון שונה מנמוך',
        manualCalculationMoneyLaundering: 'בוצע חישוב ידני להלבנת הון',
        populationRegistryNotValid: 'תוצאות בדיקת מרשם אוכלוסין אינן תקינות בבעל חשבון',
        terroristOrganizationCheckNotValid: 'בדיקת ארגון טרור אינו תקין בבעל חשבון',
        manualChangeToManagerControl: 'הועבר ידנית לבקרת מנהל'
    };

    const formAttributes = {
        alt_loyaltyprogramid: 'alt_loyaltyprogramid',
        alt_commissionclienttypeid: 'alt_commissionclienttypeid',
        alt_referralsourceid: 'alt_referralsourceid',
        alt_encouragingdepositsystemuserid: 'alt_encouragingdepositsystemuserid',
        alt_primaryaccountholderid: 'alt_primaryaccountholderid'
    };

    const KYCAttributesForNotifications = {
        alt_fundsdepositfrequencyforecastcode: 'alt_fundsdepositfrequencyforecastcode',
        alt_totaldepositforecastperyearcode: 'alt_totaldepositforecastperyearcode',
        alt_yearlytotalwithdrawaltransferforecastcode: 'alt_yearlytotalwithdrawaltransferforecastcode'
    };

    let formContext;
    const onLoad = function (executionContext) {

        formContext = executionContext.getFormContext();

        if (Utils.CrmPage.IsFirstLoad()) {
            const formType = formContext.ui.getFormType();
            const crmFormTypes = Utils.CrmPage.FormType;

            switch (formType) {
                case crmFormTypes.Create:
                case crmFormTypes.Update: {

                    initFormUI();
                    initOnChange();

                    GridUtilities.refreshParentFormByCreatingRecordInGrid(formContext, "AccountHolderGrid", initFormUI);

                    break;
                }
                default:
                    break;
            }
        }
    };

    var GridUtilities = {
        // משתני זיכרון פנימיים מבודדים למניעת לולאות ומצמוצים
        _lastRecordCount: -1,
        _isInternalRefresh: false,

        /**
         * מאזין לסאבגריד ומבצע רענון לטופס האב רק כאשר נוספה או נמחקה שורה בגריד (למשל מתוך Quick Create)
         * @param {object} formContext - הקשר הטופס של האב
         * @param {string} subgridName - השם הלוגי של הסאבגריד בטופס
         * @param {function} successCallback - פונקציה להרצה מחדש לאחר שהטופס התרענן בהצלחה (אופציונלי)
         */
        refreshParentFormByCreatingRecordInGrid: function (formContext, subgridName, successCallback) {
            const subgridControl = formContext.getControl(subgridName);
            if (!subgridControl) {
                console.warn(`[GridUtilities] Subgrid with name '${subgridName}' was not found.`);
                return;
            }

            // רישום אירוע ה-onLoad של הגריד
            subgridControl.addOnLoad(function () {

                // 1. הגנה מפני לולאה פנימית של הרענון של עצמנו
                if (GridUtilities._isInternalRefresh) {
                    GridUtilities._isInternalRefresh = false;
                    return;
                }

                // 2. שליפת כמות הרשומות הנוכחית שיש בגריד כרגע
                const currentRows = subgridControl.getGrid()?.getRows();
                const currentCount = currentRows ? currentRows.getLength() : 0;

                // 3. אם זו הטעינה הראשונה של הטופס, רק נשמור את הכמות הנוכחית ונעצור (מונע מצמוץ ראשוני)
                if (GridUtilities._lastRecordCount === -1) {
                    GridUtilities._lastRecordCount = currentCount;
                    return;
                }

                // 4. בדיקה קריטית: אם כמות הרשומות לא השתנתה (למשל סתם שמירת שדה בטופס האב), נעצור מיד!
                if (currentCount === GridUtilities._lastRecordCount) {
                    return;
                }

                // עדכון הזיכרון למצב החדש
                GridUtilities._lastRecordCount = currentCount;
                GridUtilities._isInternalRefresh = true;

                // ביצוע הרענון השקט של טופס האב
                formContext.data.refresh(false).then(
                    function success() {

                        // הרצת פונקציית האתחול מחדש (כמו initFormUI) כדי לעדכן נוטיפיקציות באב
                        if (typeof successCallback === "function") {
                            successCallback();
                        }
                    },
                    function error(e) {
                        GridUtilities._isInternalRefresh = false;
                    }
                );
            });
        }
    };

    const onSave = function (executionContext) {
        formContext = executionContext.getFormContext();
        handelInitialDepositCode();
    };

    const initFormUI = function () {
        setNotification();
        handleUIByControlStageTeamId();
        handleManagerVerificationRequiredCode();
        handleMoneyLaunderingVerificationCode();
        handelInitialDepositCode();
        setNotificationsForRequiredManagerControl();
        handleAccountHolderMainOwner();
    };

    const initOnChange = function () {

        formContext.getAttribute(formAttributes.alt_loyaltyprogramid).addOnChange(loyaltyProgramOnChange);
    };

    const loyaltyProgramOnChange = function () {

        setCommissionClientTypeIdByLoyaltyProgram();
    };

    const setNotification = function () {
        notificationForKYCLinkAccountHoldersOfAccountHolderType();
        getAllAccountHolderByDigitalFormVerificationId();
        notificationForPopulationRegister();
        notificationForScreeningQuestionsChanges();
        handleNotificationFromMainAccountHolder();
        handleNotificationsFromLatestKYC();
    };

    const notificationForScreeningQuestionsChanges = function () {
        const fetchXml = [
            "<fetch mapping='logical' no-lock='true' distinct='true'>",
            "<entity name='alt_accountholder'>",
            "<attribute name='alt_name' />",
            "<attribute name='alt_changeisraeliresidencybit' />",
            "<attribute name='alt_changeuspersondeclarationbit' />",
            "<attribute name='alt_changeforeigntaxresidencybit' />",
            "<filter type='and'>",
            "<condition attribute='alt_digitalformverificationid' operator='eq'  uitype='alt_digitalformverification' value='" + formContext.data.entity.getId() + "'/>",
            "<condition attribute='alt_accountholdertypecode' operator='eq' value='" + accountHolderTypeCode.AccountHolder + "' />",
            "<condition attribute='statecode' operator='eq' value='0' />",
            "</filter>",
            "</entity>",
            "</fetch>",
        ].join("");

        Utils.Server.Fetch('alt_accountholder', fetchXml, setNotificationForScreeningQuestionsChanges, null);
    }

    const setNotificationForScreeningQuestionsChanges = function (receivedData) {
        receivedData.forEach(accountHolder => {
            const messages = [];
            if (accountHolder.alt_changeisraeliresidencybit)
                messages.push("תיקון תושבות ישראליות");
            if (accountHolder.alt_changeuspersondeclarationbit)
                messages.push("תיקון תשובות US Person");
            if (accountHolder.alt_changeforeigntaxresidencybit)
                messages.push("תיקון הצהרת תשובות מס זרה");

            if (messages.length > 0) {
                const alertMessage = `לתשומת ליבך - ` + accountHolder.alt_name + ` ביצע שינויים בשאלה/ות הניפוי: ` + messages.join(", ");
                formContext.ui.setFormNotification(alertMessage, notificationLevel.Warning);
            }
        })
        console.log(receivedData);
    }

    const notificationForKYCLinkAccountHoldersOfAccountHolderType = function () {
        const fetchXml = [
            "<fetch  mapping='logical' no-lock='true' distinct='true'>",
            "<entity name='alt_kyc'>",
            "<attribute name='alt_kycid' />",
            "<attribute name='createdon' />",
            "<attribute name='alt_accountholderid' />",
            "<attribute name='alt_manualhandlingreasonscode' />",
            "<order attribute='alt_accountholderid' descending='true' />",
            "<order attribute='createdon' descending='true' />",
            "<link-entity name='alt_accountholder' from='alt_accountholderid' to='alt_accountholderid' link-type='inner'>",
            "<attribute name='alt_usercharacteristiccode' />",
            "<attribute name='alt_birthcountryid' />",
            "<attribute name='alt_beneficiarydeclarationrequiredbit' />",
            "<attribute name='alt_name' />",
            "<filter type='and'>",
            "<condition attribute='alt_digitalformverificationid' operator='eq'  uitype='alt_digitalformverification' value='" + formContext.data.entity.getId() + "'/>",
            "<condition attribute='alt_accountholdertypecode' operator='eq' value='" + accountHolderTypeCode.AccountHolder + "' />",
            "<condition attribute='statecode' operator='eq' value='0' />",
            "</filter>",
            "</link-entity>",
            "</entity>",
            "</fetch>",
        ].join("");

        Utils.Server.Fetch('alt_kyc', fetchXml, setNotificationForAccountHolderAndKYC, null);
    };

    const setNotificationForAccountHolderAndKYC = function (receivedData) {
        const accountHolderIds = [];
        const manualHandlingReasons = [];
        receivedData.forEach(item => {
            if (item.alt_accountholderid && item.alt_accountholderid.Id && !accountHolderIds.includes(item.alt_accountholderid.Id)) {
                accountHolderIds.push(item.alt_accountholderid.Id);
                setNotificationForAccountHolder(item);
                if (item.alt_manualhandlingreasonscode_FormattedValue && item.alt_manualhandlingreasonscode_FormattedValue.Name) {
                    const currentManualHandlingReasons = item.alt_manualhandlingreasonscode_FormattedValue.Name.split('; ');
                    currentManualHandlingReasons.forEach(function (reason) {
                        if (!manualHandlingReasons.includes(reason)) {
                            manualHandlingReasons.push(reason);
                        }
                    });
                }
            }
        });
        if (manualHandlingReasons.length > 0) {
            setNotificationForManualHandlingReasons(manualHandlingReasons);
        }
    };

    const setNotificationForAccountHolder = function (itemAccountHolder) {
        if (itemAccountHolder["alt_accountholder1.alt_beneficiarydeclarationrequiredbit"] == true) {
            formContext.ui.setFormNotification(notificationIsRequiredBeneficiaryDeclaration, notificationLevel.Warning, "IsRequiredBeneficiaryDeclaration");
        }
        if (itemAccountHolder["alt_accountholder1.alt_name"]) {
            if (itemAccountHolder["alt_accountholder1.alt_birthcountryid_FormattedValue"] && itemAccountHolder["alt_accountholder1.alt_birthcountryid_FormattedValue"].Name && !itemAccountHolder["alt_accountholder1.alt_birthcountryid_FormattedValue"].Name.includes('ישראל')) {
                formContext.ui.setFormNotification(itemAccountHolder["alt_accountholder1.alt_name"] + notificationBirthCountryIsNotIsrael, notificationLevel.Warning, "BirthCountryIsNotIsrael" + itemAccountHolder["alt_accountholder1.alt_name"]);
            }
            if (itemAccountHolder["alt_accountholder1.alt_usercharacteristiccode"] == userCharacteristicCode.professionalUser) {
                formContext.ui.setFormNotification(notificationUsPersonDeclaration + itemAccountHolder["alt_accountholder1.alt_name"], notificationLevel.Warning, "UsPersonDeclaration" + itemAccountHolder["alt_accountholder1.alt_name"]);
            }
        }
    };

    const setNotificationForManualHandlingReasons = function (manualHandlingReasonsCode) {
        manualHandlingReasonsCode.forEach(function (item) {
            formContext.ui.setFormNotification(item, notificationLevel.Warning, "manualHandlingReasonsCode" + item);
        });
    };

    const getAllAccountHolderByDigitalFormVerificationId = function () {
        const fetchXml = [
            "<fetch mapping='logical' no-lock='true' distinct='true'>",
            "<entity name='alt_accountholder'>",
            "<attribute name='alt_usercharacteristiccode' />",
            "<attribute name='alt_birthcountryid' />",
            "<attribute name='alt_beneficiarydeclarationrequiredbit' />",
            "<attribute name='alt_name' />",
            "<attribute name='alt_accountholdertypecode' />",
            "<attribute name='alt_digitalvisualrecognitioncode' />",
            "<filter type='and'>",
            "<condition attribute='alt_digitalformverificationid' operator='eq' uitype='alt_digitalformverification' value='" + formContext.data.entity.getId() + "'/>",
            "<condition attribute='statecode' operator='eq' value='0' />",
            "</filter>",
            "<link-entity name='alt_kyc' from='alt_accountholderid' to='alt_accountholderid' link-type='outer' >",
            "<attribute name='alt_accountholderid' />",
            "</link-entity >",
            "</entity>",
            "</fetch>",
        ].join("");
        Utils.Server.Fetch('alt_accountholder', fetchXml, setNotificationForAccountsHolder, null);
    };

    const setNotificationForAccountsHolder = function (receivedData) {
        receivedData.forEach(itemAccountHolder => {
            if (!itemAccountHolder["alt_kyc1.alt_accountholderid"] && itemAccountHolder.alt_accountholdertypecode == accountHolderTypeCode.AccountHolder) {
                if (itemAccountHolder.alt_beneficiarydeclarationrequiredbit == true) {
                    formContext.ui.setFormNotification(notificationIsRequiredBeneficiaryDeclaration, notificationLevel.Warning, "IsRequiredBeneficiaryDeclaration");
                }
                if (itemAccountHolder.alt_name) {
                    if (itemAccountHolder.alt_birthcountryid && itemAccountHolder.alt_birthcountryid.Name && !itemAccountHolder.alt_birthcountryid.Name.includes('ישראל')) {
                        formContext.ui.setFormNotification(itemAccountHolder.alt_name + notificationBirthCountryIsNotIsrael, notificationLevel.Warning, "BirthCountryIsNotIsrael" + itemAccountHolder.alt_name);
                    }
                    if (itemAccountHolder.alt_usercharacteristiccode == userCharacteristicCode.professionalUser) {
                        formContext.ui.setFormNotification(notificationUsPersonDeclaration + itemAccountHolder.alt_name, notificationLevel.Warning, "UsPersonDeclaration" + itemAccountHolder.alt_name);
                    }
                }
            }
            if (itemAccountHolder.alt_name && itemAccountHolder.alt_digitalvisualrecognitioncode == digitalVisualRecognitionCode.SoftFailure) {
                formContext.ui.setFormNotification(notificationSoftFailureDigitalVisualRecognition + itemAccountHolder.alt_name, notificationLevel.Warning, "SoftFailureDigitalVisualRecognition" + itemAccountHolder.alt_name);
            }
        });
    };

    const notificationForPopulationRegister = function () {
        const fetchXml = [
            "<fetch mapping='logical' no-lock='true' distinct='true'>",
            "<entity name='alt_accountholder'>",
            "<attribute name='alt_name' />",
            "<filter type='and'>",
            "<condition attribute='alt_digitalformverificationid' operator='eq' uitype='alt_digitalformverification' value='" + formContext.data.entity.getId() + "'/>",
            "<condition attribute='alt_accountholdertypecode' operator='eq' value='" + accountHolderTypeCode.AccountHolder + "'/>",
            "<condition attribute='statecode' operator='eq' value='0' />",
            "</filter>",
            "<link-entity name='alt_populationregistrycustomerverification' from='alt_populationregistrycustomerverificationid' to='alt_populationregistercustomerverificationid' link-type='inner' >",
            "<filter type='and'>",
            "<filter type='or'>",
            "<condition attribute='alt_verificationresultcode' operator='eq' value='" + verificationResultCode.NotVerified + "'/>",
            "<condition attribute='alt_verificationresultcode' operator='null'/>",
            "<condition attribute='alt_idissuancedateverificationresultcode' operator='eq' value='" + iDIssuanceDateVerificationResultCode.NotVerified + "'/>",
            "<condition attribute='alt_idissuancedateverificationresultcode' operator='null'/>",
            "<filter type='and'>",
            "<condition attribute='alt_comparedatabit' operator='eq' value='1'/>",
            "<filter type='or'>",
            "<condition attribute='alt_datacomparisonstatuscode' operator='eq' value='" + dataComparisonStatusCode.NotMatch + "'/>",
            "<condition attribute='alt_datacomparisonstatuscode' operator='null'/>",
            "</filter>",
            "</filter>",
            "</filter>",
            "</filter>",
            "</link-entity>",
            "</entity>",
            "</fetch>",
        ].join("");
        Utils.Server.Fetch('alt_accountholder', fetchXml, setNotificationForPopulationRegister, null);
    };

    const setNotificationForPopulationRegister = function (receivedData) {
        receivedData.forEach(itemAccountHolder => {
            if (itemAccountHolder.alt_name) {
                formContext.ui.setFormNotification(notificationcInvalidPopulationRegister + itemAccountHolder.alt_name, notificationLevel.Warning, "invalidPopulationRegister" + itemAccountHolder.alt_name);
            }
        });
    };

    const handleUIByControlStageTeamId = function () {
        if (formContext.getAttribute('alt_initialdepositcode').getValue() != initialDepositCode.CreateAccountWithoutFirstDeposit) {
            if (formContext.getAttribute('alt_controlstageteamid').getValue() == null) {
                formContext.getControl('alt_initialdepositcode').removeOption(initialDepositCode.CreateAccountWithoutFirstDeposit);
            } else {
                switch (formContext.getAttribute('alt_controlstageteamid').getValue()[0].name) {
                    case controlStageTeamId.ManagementControl:
                        break;
                    case controlStageTeamId.JoiningControl:
                    case controlStageTeamId.MoneyLaunderingControl:
                    case controlStageTeamId.OperationalControl:
                        formContext.getControl('alt_initialdepositcode').removeOption(initialDepositCode.CreateAccountWithoutFirstDeposit);
                        break;
                    default: break;
                }
            }
        }
    };

    const setNotificationsForRequiredManagerControl = function () {
        const controlStageTeam = formContext.getAttribute('alt_controlstageteamid').getValue();
        const requiredManagerControl = formContext.getAttribute('alt_managerverificationrequiredcode').getValue();
        let reasons = [];

        if (controlStageTeam !== null && controlStageTeam[0].name == controlStageTeamId.ManagementControl &&
            requiredManagerControl === managerVerificationRequiredCode.Yes) {

            if (formContext.getAttribute('alt_creditrequestexistscode').getValue() == 1) {
                reasons.push(requiredManagerControlReasonsText.creditLimitRequestExists);
            }
            if (formContext.getAttribute('alt_shortsalerequestapprovaiexistscode').getValue() == 1) {
                reasons.push(requiredManagerControlReasonsText.missingSellingRequestExists);
            }
            if (formContext.getAttribute('alt_optionexerciserequestapprovalexistscode').getValue() != 1) {
                reasons.push(requiredManagerControlReasonsText.optionsTradingRequestExists);
            }

            getAdditionalReasonsFromAccountHolders(reasons)
        }
    }

    const getAdditionalReasonsFromAccountHolders = function (reasons) {
        Xrm.WebApi.retrieveMultipleRecords('alt_accountholder', "?fetchXml=" + fetchAllAccountHoldersRelatedToDigitalFormVerification()).then(
            function success(result) {
                if (result.entities.length > 0) {
                    if (result.entities.some((accountHolder) => accountHolder.alt_accountholdertypecode === accountHolderTypeCode.AccountHolder &&
                        accountHolder.alt_beneficiarydeclarationcontrolcode === beneficieryDeclarationCode.Invalid &&
                        accountHolder.alt_beneficiarydeclarationcontrolcode === beneficieryDeclarationCode.requiredForFurtherCheck)) {
                        reasons.push(requiredManagerControlReasonsText.initialCheckOnBeneficiery);
                    }
                    if (result.entities.some((accountHolder) => accountHolder.alt_accountholdertypecode === accountHolderTypeCode.AccountHolder &&
                        accountHolder.alt_beneficiarydeclarationrequiredbit === beneficieryRequiredBit.Yes)) {
                        reasons.push(requiredManagerControlReasonsText.beneficieryCheckRequired);
                    }
                    if (result.entities.some((accountHolder) => accountHolder.alt_checkterrororganizationcode === terrorOrganizationCheckCode.Invalid)) {
                        reasons.push(requiredManagerControlReasonsText.terroristOrganizationCheckNotValid);
                    }
                }
                getAdditionalReasonsFromPopulationRegistry(reasons);
            },
            null
        )
    }

    const getAdditionalReasonsFromPopulationRegistry = function (reasons) {
        Xrm.WebApi.retrieveMultipleRecords('alt_accountholder', "?fetchXml=" + fetchInvalidPopuationRegistryForAccountHolders()).then(
            function success(result) {
                if (result.entities.length > 0) {
                    reasons.push(requiredManagerControlReasonsText.populationRegistryNotValid);
                }
                getAdditionalReasonsFromMoneyLaundreingCalculation(reasons);
            }
        )
    }

    const getAdditionalReasonsFromMoneyLaundreingCalculation = function (reasons) {
        Xrm.WebApi.retrieveMultipleRecords('alt_moneylaunderingcalculation', "?fetchXml=" + fetchMoneyLaunderingCalculationsRelatedToDigitalFormVerification()).then(
            function success(result) {
                if (result.entities.length > 1) {
                    reasons.push(requiredManagerControlReasonsText.manualCalculationMoneyLaundering);
                }
                if (result.entities.some((moneyLaundering) =>
                    moneyLaundering.alt_calculetedmoneylaunderinglevelcode !== calculatedMoneyLaunderingLevelCode.Low)) {
                    reasons.push(requiredManagerControlReasonsText.moneyLaunderingLevelNotLow);
                }
                displayNotificationForManagerControlReasons(reasons);
            }
        )
    }

    const displayNotificationForManagerControlReasons = function (reasons) {
        let requiredManagerControlReasonsNotificationText;
        if (reasons.length > 0) {
            const reasonsText = reasons.join(", ");
            requiredManagerControlReasonsNotificationText = 'נדרשת בקרת מנהל: ' + reasonsText;
        }
        else {
            requiredManagerControlReasonsNotificationText = 'נדרשת בקרת מנהל: ' + requiredManagerControlReasonsText.manualChangeToManagerControl;
        }
        formContext.ui.setFormNotification(requiredManagerControlReasonsNotificationText, notificationLevel.Warning);
    }

    const fetchAllAccountHoldersRelatedToDigitalFormVerification = function () {
        const fetchXml = [
            "<fetch mapping='logical' no-lock='true' distinct='true'>",
            "<entity name='alt_accountholder'>",
            "<attribute name='alt_beneficiarydeclarationcontrolcode' />",
            "<attribute name='alt_beneficiarydeclarationrequiredbit' />",
            "<attribute name='alt_checkterrororganizationcode' />",
            "<attribute name='alt_accountholdertypecode' />",
            "<filter type='and'>",
            "<condition attribute='alt_digitalformverificationid' operator='eq' uitype='alt_digitalformverification' value='" + formContext.data.entity.getId() + "'/>",
            "</filter>",
            "</entity>",
            "</fetch>",
        ].join("");
        return fetchXml;
    }

    const fetchInvalidPopuationRegistryForAccountHolders = function () {
        const fetchXml = [
            "<fetch mapping='logical' no-lock='true' distinct='true'>",
            "<entity name='alt_accountholder'>",
            "<attribute name='alt_name' />",
            "<filter type='and'>",
            "<condition attribute='alt_digitalformverificationid' operator='eq' uitype='alt_digitalformverification' value='" + formContext.data.entity.getId() + "'/>",
            "<condition attribute='alt_accountholdertypecode' operator='eq' value='" + accountHolderTypeCode.AccountHolder + "'/>",
            "</filter>",
            "<link-entity name='alt_populationregistrycustomerverification' from='alt_populationregistrycustomerverificationid' to='alt_populationregistercustomerverificationid' link-type='inner' >",
            "<filter type='and'>",
            "<condition attribute='alt_verificationresultcode' operator='eq' value='" + verificationResultCode.NotVerified + "'/>",
            "</filter>",
            "</link-entity>",
            "</entity>",
            "</fetch>",
        ].join("");
        return fetchXml;
    }

    const fetchMoneyLaunderingCalculationsRelatedToDigitalFormVerification = function () {
        const fetchXml = [
            "<fetch mapping='logical' no-lock='true' distinct='true'>",
            "<entity name='alt_moneylaunderingcalculation'>",
            "<attribute name='alt_calculetedmoneylaunderinglevelcode' />",
            "<filter type='and'>",
            "<condition attribute='alt_digitalformverificationid' operator='eq' uitype='alt_digitalformverification' value='" + formContext.data.entity.getId() + "'/>",
            "</filter>",
            "</entity>",
            "</fetch>",
        ].join("");
        return fetchXml;
    }

    const handleManagerVerificationRequiredCode = function () {
        if (formContext.getAttribute('alt_managerverificationrequiredcode').getValue() == managerVerificationRequiredCode.Yes) {
            Utils.CrmPage.SetControlDisabledMode(formContext, 'alt_managerverificationrequiredcode', true);
        }
    };

    const handleMoneyLaunderingVerificationCode = function () {
        if (formContext.getAttribute('alt_moneylaunderingverificationcode').getValue() == managerVerificationRequiredCode.No && formContext.getAttribute('alt_controlstageteamid').getValue() && formContext.getAttribute('alt_controlstageteamid').getValue()[0].name == controlStageTeamId.ManagementControl) {
            Utils.CrmPage.SetControlDisabledMode(formContext, 'alt_moneylaunderingverificationcode', false);
        }
    };

    const handelInitialDepositCode = function () {
        const controlTeam = formContext.getAttribute('alt_controlstageteamid').getValue();
        const intialDepositCode = formContext.getAttribute('alt_initialdepositcode').getValue();

        let isDisabled = intialDepositCode == initialDepositCode.AcceptedDeposit ?
            true : false;
        Utils.CrmPage.SetControlDisabledMode(formContext, 'alt_initialdepositcode', isDisabled);
        if (!isDisabled) {
            handleInitialDepositCodeOptionsByUserTeams()
        }
    };

    const getFetchXmlAllTeamsByUserId = function (userId) {
        return `
        <fetch>
          <entity name="team">
            <attribute name="name" />
            <attribute name="teamid" />
            <attribute name="alt_teamcodeint" />
            <link-entity name="teammembership" from="teamid" to="teamid" intersect="true">
              <filter>
                <condition attribute="systemuserid" operator="eq" value="${userId}" />
              </filter>
            </link-entity>
          </entity>
        </fetch>
        `;
    }

    const handleInitialDepositCodeOptionsByUserTeams = function () {

        Utils.Global.GetGlobalParamValue('TeamCodesPermittedForAcceptedDeposit', function (globalParamValue) {
            if (globalParamValue) {
                let teamCodesPermittedForAcceptedDeposit = globalParamValue.split(',').map(Number);
                let userId = Xrm.Utility.getGlobalContext().userSettings.userId.replace(/[{}]/g, "");

                Utils.Server.Fetch('team', getFetchXmlAllTeamsByUserId(userId), function (userTeams) {

                    if (!userTeams
                        || !userTeams.some(x => teamCodesPermittedForAcceptedDeposit.includes(x.alt_teamcodeint))) {

                        formContext.getControl('alt_initialdepositcode').removeOption(initialDepositCode.AcceptedDeposit);
                    }
                }, null);
            }
        }, null);
    };

    const handleAccountHolderMainOwner = function () {

        let currentRecordId = Utils.JsExtantions.String.RemoveBraces(formContext.data.entity.getId());
        let select = 'alt_accountholderid, alt_clubmembershipeligibilitycode';
        let filter = `_alt_digitalformverificationid_value eq ${currentRecordId} 
                      and alt_accountholdertypecode eq ${accountHolderTypeCode.AccountHolder}
                      and alt_mainaccountholderbit eq true`;

        Utils.Server.RetrieveMultiple('alt_accountholder', select, filter, null, null, function (retrievedAccountHolders) {

            if (retrievedAccountHolders?.length > 0) {

                let accountHolderMainOwner = retrievedAccountHolders[0];

                setNotificationForAccountHolderOwnerUnidentifiedNoEligibility(accountHolderMainOwner);
                handleAccountHolderCustomerOperationRequest(accountHolderMainOwner);
                disableLoyaltyProgramIdByAccountHolderClubMembershipEligibility(accountHolderMainOwner);
            }
        });
    };

    const setNotificationForAccountHolderOwnerUnidentifiedNoEligibility = function (accountHolderMainOwner) {

        if (accountHolderMainOwner.alt_clubmembershipeligibilitycode == clubMembershipEligibilityCode.UnidentifiedNoEligibility) {

            formContext.ui.setFormNotification(NOTIFICATION_ACCOUNTHOLDER_UNIDENTIFIED_NOELIGIBILITY,
                notificationLevel.Warning,
                "NOTIFICATION_ACCOUNTHOLDER_UNIDENTIFIED_NOELIGIBILITY");
        }
    };

    const handleAccountHolderCustomerOperationRequest = function (accountHolderMainOwner) {

        const Mivza1Mivza2Codes = [2, 3];

        let referralsource = formContext.getAttribute(formAttributes.alt_referralsourceid).getValue();
        if (referralsource) {

            Utils.Server.Retrieve('alt_referralsource', referralsource[0].id, 'alt_codeint', null,
                function (retrievedReferralSource) {

                    if (retrievedReferralSource && Mivza1Mivza2Codes.includes(retrievedReferralSource.alt_codeint)) {

                        showAlertDialogForFailedOrIncompleteOperationRequest(accountHolderMainOwner);
                    }
                }
            );
        }
    };

    const showAlertDialogForFailedOrIncompleteOperationRequest = function (accountHolderMainOwner) {

        const operationRequestStatusFailed = 399020005;
        const operationRequestStatusSuccess = 399020004;
        const operationRequestOp1Op2Code = 4;
        const clubMembershipEligibilityMessagesParamName = "ClubMembershipEligibilityMessages";

        Utils.Global.GetGlobalParamValue(clubMembershipEligibilityMessagesParamName, function (retrievedClubMembershipEligibilityMessages) {

            if (retrievedClubMembershipEligibilityMessages) {

                let clubMembershipEligibilityMessages = JSON.parse(retrievedClubMembershipEligibilityMessages);
                let select = 'alt_customeroperationrequestid, statuscode';
                let filter = `_alt_relatedrecordid_value eq ${accountHolderMainOwner.alt_accountholderid}
                              and alt_CustomerOperationTemplateId/alt_codeint eq ${operationRequestOp1Op2Code}`;

                Utils.Server.RetrieveMultiple('alt_customeroperationrequest', select, filter, null, null, function (retrievedOperationRequests) {

                    if (retrievedOperationRequests?.length > 0) {

                        if (retrievedOperationRequests[0].statuscode == operationRequestStatusFailed) {

                            Utils.CrmPage.SetAttributeRequiredLevel(formContext, formAttributes.alt_loyaltyprogramid,
                                Utils.CrmPage.RequirementLevel.Required);
                            Xrm.Navigation.openAlertDialog({ text: clubMembershipEligibilityMessages.RequestFailedMessage });
                            return;
                        } else if (retrievedOperationRequests[0].statuscode == operationRequestStatusSuccess) {
                            return;
                        }
                    }

                    Xrm.Navigation.openAlertDialog({ text: clubMembershipEligibilityMessages.RequestIncompleteMessage });
                });
            }
        });
    };

    const disableLoyaltyProgramIdByAccountHolderClubMembershipEligibility = function (accountHolderMainOwner) {

        let isClubMembershipEligibilityOperation1Or2 =
            accountHolderMainOwner.alt_clubmembershipeligibilitycode == clubMembershipEligibilityCode.Operation1
            || accountHolderMainOwner.alt_clubmembershipeligibilitycode == clubMembershipEligibilityCode.Operation2;

        Utils.CrmPage.SetControlDisabledMode(formContext, formAttributes.alt_loyaltyprogramid, isClubMembershipEligibilityOperation1Or2);
    };

    const setCommissionClientTypeIdByLoyaltyProgram = function () {

        let loyaltyProgram = formContext.getAttribute(formAttributes.alt_loyaltyprogramid).getValue();
        if (loyaltyProgram) {

            let loyaltyProgramId = Utils.JsExtantions.String.RemoveBraces(loyaltyProgram[0].id);

            Utils.Server.Retrieve('alt_loyaltyprogram', loyaltyProgramId, '_alt_commissionclienttypeid_value', null,
                function (retrievedLoyaltyProgram) {
                    let commisionClientTypeValue = retrievedLoyaltyProgram.alt_commissionclienttypeid;

                    Utils.CrmPage.SetLookup(formContext,
                        formAttributes.alt_commissionclienttypeid,
                        commisionClientTypeValue?.Id,
                        commisionClientTypeValue?.Name,
                        commisionClientTypeValue?.LogicalName);
                }
            );

        } else {

            formContext.getAttribute(formAttributes.alt_commissionclienttypeid).setValue(null);
        }
    };

    const handleNotificationFromMainAccountHolder = function () {

        const primaryAccountHolderId = formContext.getAttribute(formAttributes.alt_primaryaccountholderid).getValue();
        if (primaryAccountHolderId != null) {
            const entityLogicalName = primaryAccountHolderId[0].entityType;
            const id = Utils.JsExtantions.String.RemoveBraces(primaryAccountHolderId[0].id);
            Utils.Server.Retrieve(entityLogicalName, id, 'alt_identificationnumbercontrolcomparisoncode', null,
                function (result) {
                    if (result.alt_identificationnumbercontrolcomparisoncode === comparisonCode.NotIdentical) {
                        formContext.ui.setFormNotification(COMPARISON_NOTITIFCATION_MESSAGE_WARNING, notificationLevel.Warning);
                    }
                },
                function (error) {
                    console.log(error);
                });
        }
    };

    const handleNotificationsFromLatestKYC = function () {

        let currentRecordId = Utils.JsExtantions.String.RemoveBraces(formContext.data.entity.getId());
        let select = Object.values(KYCAttributesForNotifications).join(", ");
        let filter = `_alt_digitalformverificationid_value eq ${currentRecordId}`;
        let orderby = 'createdon desc';

        Utils.Server.RetrieveMultiple('alt_kyc', select, filter, orderby, null,
            function (entity) {
                if (entity.length > 0) {
                    setNotificationsFromLatestKYC(entity[0]);
                }
            },
            function (error) {
                console.log(error);
            });
    };

    const setNotificationsFromLatestKYC = function (kycRecord) {
        const fundsDepositFrequencyForecastCodeValue = kycRecord.alt_fundsdepositfrequencyforecastcode;
        const totalDepositForecastPerYearCodeValue = kycRecord.alt_totaldepositforecastperyearcode;
        const yearlyTotalWithdrawalTransferForecastCodeValue = kycRecord.alt_yearlytotalwithdrawaltransferforecastcode;

        if (totalDepositForecastPerYearCodeValue == totalDepositForecastPerYearCode.BetweenZeroAndFiftyThousand
            && yearlyTotalWithdrawalTransferForecastCodeValue == yearlyTotalWithdrawalTransferForecastCode.TwoHundredAndFiftyThousandAndAbove) {
            formContext.ui.setFormNotification(NO_MATCH_BETWEEN_DEPOSITS_AND_WITHDRAWALS, notificationLevel.Warning);
        }

        if (fundsDepositFrequencyForecastCodeValue == fundsDepositFrequencyForecastCode.OnceAMonth
            && totalDepositForecastPerYearCodeValue == totalDepositForecastPerYearCode.TwoHundredAndFiftyThousandAndAbove) {
            formContext.ui.setFormNotification(NO_MATCH_BETWEEN_DEPOSIT_FREQUENCY_AND_AMOUNT, notificationLevel.Warning);
        }
    }

    return {
        OnLoad: onLoad,
        OnSave: onSave
    };
})();
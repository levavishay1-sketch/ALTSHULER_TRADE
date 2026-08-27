var CustomerActivitiesCommonBL = (function () {

    let formContext;
    let conformDialogTitle;
    let conformDialogText = 'הסיסמה החד פעמית היא {0}';
    let templateType;
    let otpTemplateCodes;
    let otpCode;

    const SMS_OTP_CONFIRM_DIALOG_TITLE = 'שליחת OTP ב- SMS';
    const EMAIL_OTP_CONFIRM_DIALOG_TITLE = 'שליחת OTP בדוא"ל';
    const RETRY_MESSAGE = 'האם לשלוח פעם נוספת?'
    const POPULATION_REGISTRY_VERIFICATION_CONFORM_MESSAGE = 'להמשך תהליך אימות הנתונים ולפניה למרשם האוכלוסין יש ללחוץ על כפתור "אישור". לביטול המשך התהליך יש ללחוץ על כפתור "ביטול".';

    const sendOTPCode = function (formcontext, activityType, to, regardingObjectId, parserCustomEntryPoint, customerId) {

        formContext = formcontext;
        templateType = activityType;
        conformDialogTitle = templateType == activityTemplateType.Sms ?
            SMS_OTP_CONFIRM_DIALOG_TITLE : EMAIL_OTP_CONFIRM_DIALOG_TITLE;
        Utils.Global.GetGlobalParamValue('OTPTemplateCodes', function (globalParam) {
            if (globalParam) {
                const parsedJson = JSON.parse(globalParam);
                const templateCodes = parsedJson.otpTemplateCodes.filter(function (value) {
                    return value.activityTemplateType == templateType;
                });
                otpTemplateCodes = templateCodes && templateCodes[0] && templateCodes[0];
                if (otpTemplateCodes) {
                    let contactId;
                    if (customerId.entityType = entityName.Contact) {
                        contactId = customerId.id;
                    }
                    callOTPManagerAction(templateType, otpTemplateCodes.templateCode, regardingObjectId.id, to, parserCustomEntryPoint, contactId,
                        function (result) {
                            sendOTPSuccessCallback(result, activityType, to, regardingObjectId, parserCustomEntryPoint, customerId);
                        });
                }
                else {
                    Xrm.Navigation.openAlertDialog({ text: Utils.CrmPage.CommonRequestFailedMessage });
                    console.log('OTP template codes not found in global parameter.');
                }

            } else {
                Xrm.Navigation.openAlertDialog({ text: Utils.CrmPage.CommonRequestFailedMessage });
            }
        }, null);
    };

    const callOTPManagerAction = function (templateType, templateCode, regardingObjectId, to, parserCustomEntryPoint, contactId, successCallback) {

        Xrm.Utility.showProgressIndicator("מתבצעת שליחת קוד אימות...");
        const crmDataTypes = Utils.Server.CrmDataTypes;
        let payload = [
            { 'key': 'ActivityTemplateType', 'value': templateType, 'type': crmDataTypes.Int },
            { 'key': 'TemplateCode', 'value': templateCode, 'type': crmDataTypes.Int },
            { 'key': 'RegardingObjectId', 'value': regardingObjectId, 'type': crmDataTypes.String },
            { 'key': 'To', 'value': to, 'type': crmDataTypes.String }
        ];
        if (parserCustomEntryPoint) {
            payload.push({ 'key': 'ParserCustomEntryPoint', 'value': parserCustomEntryPoint, 'type': crmDataTypes.String });
        }
        if (contactId) {
            payload.push({ 'key': 'ContactId', 'value': Utils.JsExtantions.String.RemoveBraces(contactId), 'type': crmDataTypes.String });
        }
        Utils.Server.CallAction("alt_OTPManager", null, null, payload,
            function (result) {
                Xrm.Utility.closeProgressIndicator();
                if (successCallback) {
                    successCallback(result);
                }
            },
            function (error) {
                Xrm.Navigation.openAlertDialog({ text: INTERNAL_SERVER_ERROR });
                console.log(error);
                Xrm.Utility.closeProgressIndicator();
            });
    };

    var sendOTPSuccessCallback = function (result, activityType, to, regardingObjectId, parserCustomEntryPoint, customerId) {
        if (result.IsSuccess) {
            otpCode = result.ReturnObject;
            var confirmStrings = {
                text: Utils.JsExtantions.String.Format(conformDialogText, otpCode),
                title: conformDialogTitle,
                confirmButtonLabel: 'זיהוי תקין',
                cancelButtonLabel: 'לא זוהה'
            };
            var confirmOptions = { height: 150, width: 400 };
            Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(function (success) {
                if (success.confirmed) {
                    openNewIncident(regardingObjectId, customerId);
                }
                else {
                    Xrm.Navigation.openConfirmDialog({ text: RETRY_MESSAGE, confirmButtonLabel: 'שלח שוב' }, confirmOptions).then(function (success) {
                        if (success.confirmed) {
                            sendOTPCode(formContext, activityType, to, regardingObjectId, parserCustomEntryPoint, customerId);
                        }
                        else {
                            createIncident(regardingObjectId, customerId);
                        }
                    });
                }
            });
        } else {
            Xrm.Navigation.openAlertDialog({ text: result.ReturnObject });
        }
    };

    const openNewIncident = function (regardingObjectId, customerId) {

        var formParameters = {};
        formParameters["caseorigincode"] = 1;// שיחה נכנסת
        formParameters["description"] = " זיהוי על ידי נציג - סיסמה חד פעמית " + otpCode;
        formParameters["customerid"] = customerId.id;
        formParameters["customeridname"] = customerId.name;
        formParameters["customeridtype"] = customerId.entityType;
        if (regardingObjectId.entityType == entityName.Portfolio) {
            formParameters["alt_portfolioid"] = regardingObjectId.id;
            formParameters["alt_portfolioidname"] = regardingObjectId.name;
            formParameters["alt_portfolioidtype"] = regardingObjectId.entityType;
        }

        var pageInput = {
            pageType: "entityrecord",
            entityName: "incident",
            data: formParameters
        };
        var navigationOptions = {
            target: 2,
            height: { value: 850, unit: "%" },
            width: { value: 850, unit: "%" },
            position: 1
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions);
    };

    const createIncident = function (regardingObjectId, customerId) {
        Xrm.Utility.showProgressIndicator("מתבצעת יצירת אירוע תיעוד זיהוי נכשל...");
        let incidentToCreate = {};
        incidentToCreate["alt_automaticincidenttemplatekey"] = otpTemplateCodes.incidentTemplates.failedOTP;
        incidentToCreate["description"] = "סיסמה חד פעמית לזיהוי לקוח " + otpCode;

        const customeridId = Utils.JsExtantions.String.RemoveBraces(customerId.id);
        const customerPropertyName = Utils.Global.GenerateLookupObjectPropertyName(customerId.entityType, "customerid_");
        incidentToCreate[customerPropertyName] = Utils.Global.GenerateLookupObjectValue(customerId.entityType, customeridId);

        if (regardingObjectId.entityType == entityName.Portfolio) {
            const portfolioPropertyName = Utils.Global.GenerateLookupObjectPropertyName('alt_PortfolioId');
            incidentToCreate[portfolioPropertyName] = Utils.Global.GenerateLookupObjectValue(regardingObjectId.entityType, Utils.JsExtantions.String.RemoveBraces(regardingObjectId.id));
        }
        Xrm.WebApi.createRecord("incident", incidentToCreate).then(
            function success(result) {
                Xrm.Utility.closeProgressIndicator();
                Xrm.Navigation.openAlertDialog({ text: 'נוצר לחשבון אירוע שירות לתיעוד זיהוי OTP נכשל.' });
            },
            function (error) {
                Xrm.Utility.closeProgressIndicator();
                Xrm.Navigation.openAlertDialog({ text: error });
            }
        );
    };

    const openPopulationRegistryCustomerVerificationForm = function (formcontext, dto, isCompareData, isDialogForm) {

        formContext = formcontext;

        var confirmStrings = {
            text: POPULATION_REGISTRY_VERIFICATION_CONFORM_MESSAGE,
            confirmButtonLabel: 'אישור',
            cancelButtonLabel: 'ביטול'
        };
        var confirmOptions = { height: 150, width: 400 };
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(function (success) {
            if (success.confirmed) {
                openNewPopulationRegistryCustomerVerification(dto, isCompareData, isDialogForm);
            }
        });
    };

    const openNewPopulationRegistryCustomerVerification = function (dto, isCompareData, isDialogForm) {
        var formParameters = {};
        if (dto.customer && dto.customer.entityType == entityName.Contact) {
            formParameters["alt_contactid"] = dto.customer.id;
            formParameters["alt_contactidname"] = dto.customer.name;
            formParameters["alt_contactidtype"] = dto.customer.entityType;
        }
        if (dto.identityNumber) {
            formParameters["alt_identitynumber"] = dto.identityNumber;
        }
        if (dto.birthdate) {
            formParameters["alt_birthdate"] = dto.birthdate;
        }
        if (dto.idissuanceDate) {
            formParameters["alt_idissuancedate"] = dto.idissuanceDate;
        }
        if (dto.joiningProcessNumber) {
            formParameters["alt_joiningprocessnumber"] = dto.joiningProcessNumber;
        }
        if (isCompareData) {
            formParameters["alt_comparedatabit"] = isCompareData;
        }
        formParameters["alt_relatedrecordid"] = Utils.JsExtantions.String.RemoveBraces(formContext.data.entity.getId());
        formParameters["alt_relatedrecordidtype"] = formContext.data.entity.getEntityName();

        if (isDialogForm) {
            var pageInput = {
                pageType: "entityrecord",
                entityName: "alt_populationregistrycustomerverification",
                data: formParameters
            };
            var navigationOptions = {
                target: 2,
                height: { value: 850, unit: "%" },
                width: { value: 850, unit: "%" },
                position: 1
            };
            Xrm.Navigation.navigateTo(pageInput, navigationOptions);
        }
        else {
            var entityFormOptions = {
                entityName: 'alt_populationregistrycustomerverification',
                height: 500,
                width: 800,
                openInNewWindow: true,
                navbar:'off'
            };
            Xrm.Navigation.openForm(entityFormOptions, formParameters);
        }
    };

    return {
        SendOTPCode: sendOTPCode,
        OpenPopulationRegistryCustomerVerificationForm: openPopulationRegistryCustomerVerificationForm
    }

})();
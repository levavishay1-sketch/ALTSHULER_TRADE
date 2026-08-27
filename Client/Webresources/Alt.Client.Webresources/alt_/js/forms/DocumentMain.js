/// <reference path="../utils/Utils.CrmPage.js" />
/// <reference path="../utils/Utils.Server.js" />
/// <reference path="../utils/Utils.JsExtantions.js" />


var DocumentMain = (function () {
    let formContext;
    let maxFileSize = 1024 * 1024 * 11;

    const formAttributes = {
        alt_file: 'alt_file'
    };

    const onLoad = function (executionContext) {
        formContext = executionContext.getFormContext();
        //initOnChange();
        //formContext.data.entity.addOnPostSave(UploadFile);
    };

    const UploadFile = function () {
        if (formContext.getAttribute(formAttributes.alt_file).getValue() == null) {
            const entityId = Utils.JsExtantions.String.RemoveBraces(formContext.data.entity.getId())
            Xrm.Device.pickFile(pickFileOptions = { maximumAllowedFileSize: maxFileSize }).then(function (data) {
                Xrm.Utility.showProgressIndicator('מעלה קובץ ל-CRM...');

                var fileData = {
                    Content: data[0].fileContent,
                    MimeType: data[0].mimeType,
                    FileName: data[0].fileName,
                    DocumentId: entityId
                };

                var actionInput = [{
                    key: 'Data',
                    value: JSON.stringify(fileData),
                    type: "string"
                }];

                Utils.Server.CallAction(
                    'alt_UploadFile',
                    'alt_document',
                    null,
                    actionInput,
                    function (res) {
                        Xrm.Utility.closeProgressIndicator();
                        Xrm.Navigation.openAlertDialog({ text: "קובץ עלה בהצלחה" }).then(
                            function (res) {
                                Xrm.Page.data.refresh();
                            },
                            null);
                    },
                    function () {
                        Xrm.Utility.closeProgressIndicator();
                        Xrm.Navigation.openAlertDialog({ text: "כישלון בהעלאת קובץ" });
                    });
            }, null);
        }
    }

    const initOnChange = function () {
        formContext.getAttribute(formAttributes.alt_file).addOnChange(fileOnChange);
    }

    const fileOnChange = function (executionContext) {
        console.log(executionContext.getDepth())
        if (executionContext.getDepth() == 1) {

        }
    }

    return {
        OnLoad: onLoad,
    };
})();
/// <reference path="../utils/Utils.Server.js" />
/// <reference path="../utils/Utils.Global.js" />

var DocumentRibbonBL = (function () {

    const maxFileSize = 11 * 1024 * 1024 // 11MB
    const downloadTimeoutValue = 2 * 60 // 2 minutes
    const documentEntityName = "alt_document";
    const documentEntitySetName = "alt_documents";
    const fileAttributeName = "alt_file";
    const accountHolderTypeCodeFieldName = "alt_accountholdertypecode";

    const customActionsNames = {
        UploadFile: "alt_UploadFile",
        DownloadFile: "alt_DownloadFile"
    };

    const accountHolderTypes = {
        Owner: 1,
        Beneficiary: 2,
        PowerOfAttorney: 3,
        Shareholder: 4,
        AppointedByOrder: 5,
        BeneficiaryShareholder: 6,
        RelatedCorporationShareholder: 7,
        Custodian: 8,
        Trustee: 9,
        AuthorizedForOperations: 10,
        AuthorizedForInformation: 11
    }

    const messages = {
        UploadingFile: "מעלה קובץ ל-CRM...",
        UploadSuccess: "קובץ עלה בהצלחה",
        UploadFail: "כישלון בהעלאת קובץ",
        UploadFailTimeOut: "חריגה בזמן בעת העלאת קובץ",
        DownloadingFile: "מכין קובץ להורדה...",
        DownloadSuccess: "קובץ ירד בהצלחה",
        DownloadFail: "כישלון בהורדת קובץ",
        DownloadFailTimeOut: "חריגה בזמן בהורדת קובץ",
        RetrieveAccountHoldersEmpty: "לא קיימים בעלי חשבון שניתן לעלות להם קבצים"
    }

    const relatedEntitesNames = ["alt_digitalformverification", "alt_portfolio"];

    let formContext;
    let entityName;
    let entityId;

    const createDocument = function (primaryControl) {
        formContext = primaryControl ? primaryControl : Xrm.Page;
        entityId = Utils.JsExtantions.String.RemoveBraces(formContext.data.entity.getId());
        entityName = formContext.data.entity.getEntityName();

        if (relatedEntitesNames.indexOf(entityName) != -1) {
            createDocumentForAccountHolders();
        }
        else {
            createDocumentForGenericEntity();
        }
    }

    const createDocumentForAccountHolders = function () {
        let lookupOptions = {
            allowMultiSelect: true,
            disableMru: true,
            defaultEntityType: `alt_accountholder`,
            entityTypes: [`alt_accountholder`]
        }

        let filterConditions = [
            `<condition attribute="${entityName}id" operator="eq" uitype="${entityName}id" value="{${entityId}}"/>`,
            `<condition attribute="${accountHolderTypeCodeFieldName}" operator="ne" uitype="${accountHolderTypeCodeFieldName}" value="${accountHolderTypes.Beneficiary}">`
        ]

        lookupOptions.filters = [{
            filterXml: '<filter type="and">' + filterConditions.join('') + ' </filter>',
            entityLogicalName: `alt_accountholder`
        }]

        let select = 'alt_accountholderid, _alt_customerid_value';
        let filter = `_${entityName}id_value eq ${entityId} and ${accountHolderTypeCodeFieldName} ne ${accountHolderTypes.Beneficiary}`;

        Utils.Server.RetrieveMultiple('alt_accountholder', select, filter, null, null, function (accountHolderRetrieveResults) {
            if (accountHolderRetrieveResults === null) {
                Xrm.Navigation.openAlertDialog({ text: messages.RetrieveAccountHoldersEmpty });
            }
            else {
                Xrm.Device.pickFile(pickFileOptions = { maximumAllowedFileSize: maxFileSize }).then(function (fileData) {
                    if (fileData && fileData.length > 0) {
                        if (accountHolderRetrieveResults.length === 1) {
                            createDocumentRecord(entityId, accountHolderRetrieveResults[0].alt_customerid, fileData);
                        }
                        else {
                            Xrm.Utility.lookupObjects(lookupOptions).then(function (results) {
                                if (results && results.length > 0) {
                                    results.forEach((accountHolder) => {
                                        createDocumentRecord(
                                            entityId,
                                            accountHolderRetrieveResults.find(element => element.alt_accountholderid === Utils.JsExtantions.String.RemoveBraces(accountHolder.id).toLowerCase()).alt_customerid,
                                            fileData
                                        )
                                    });
                                }
                            }, function (e) {
                                console.log(e.error.message);
                            });
                        }
                    }
                }, null);
            }
        });
    }

    const createDocumentForGenericEntity = function () {
        let select = '_alt_customerid_value';

        Xrm.Device.pickFile(pickFileOptions = { maximumAllowedFileSize: maxFileSize }).then(function (fileData) {
            if (fileData && fileData.length > 0) {
                Utils.Server.Retrieve(entityName, entityId, select, null, function (entity) {
                    if (entity) {
                        Xrm.WebApi.createRecord(documentEntityName, data).then(
                            function success(result) {
                                createDocumentRecord(entityId, entity.alt_customerid, fileData);
                            },
                            function (error) {
                                console.log(error.message);
                            }
                        );
                    }
                }, null);
            }
        }, null);
    }

    const createDocumentRecord = function (entityId, customerLookup, fileData) {
        let data = {};
        data[`alt_RegardingId_${entityName}@odata.bind`] = `/${entityName}s(${entityId})`;
        data[`alt_CustomerId_${customerLookup.LogicalName}@odata.bind`] = `/${customerLookup.LogicalName}s(${customerLookup.Id})`;

        Xrm.WebApi.createRecord(documentEntityName, data).then(
            function success(result) {
                fileUpload(result.id, fileData);
            },
            function (error) {
                console.log(error.message);
            }
        );
    }

    const fileUpload = function (documentId, fileData) {

        var fileDataForCustomAction = {
            Content: fileData[0].fileContent,
            MimeType: fileData[0].mimeType,
            FileName: fileData[0].fileName,
            DocumentId: documentId
        };

        var actionInput = [{
            key: 'Data',
            value: JSON.stringify(fileDataForCustomAction),
            type: Utils.Server.CrmDataTypes.String
        }];

        Utils.Server.CallAction(
            customActionsNames.UploadFile,
            documentEntityName,
            null,
            actionInput,
            function (res) {
                Xrm.Page.data.refresh();
            },
            function () { }
        );
    }

    const downloadDocumentFromSubGrid = function (primaryControl, selectedItem) {
        formContext = primaryControl ? primaryControl : Xrm.Page;
        const selectedDocumentId = selectedItem[0]["Id"];

        Utils.Server.Retrieve(documentEntityName, selectedDocumentId, "alt_file, alt_file_name", null,
            function (result) {
                if (result.alt_file != null) {
                    prepareFile(result.alt_file_name, selectedDocumentId);
                }
                else {
                    downloadFile(formContext, selectedDocumentId);
                }
            }, null)
    }

    const downloadFile = function (formContext, documentId) {

        var fileData = {
            DocumentId: documentId
        };

        var actionInput = [{
            key: 'Data',
            value: JSON.stringify(fileData),
            type: Utils.Server.CrmDataTypes.String
        }];

        Xrm.Utility.showProgressIndicator(messages.DownloadingFile);

        Utils.Server.CallAction(
            customActionsNames.DownloadFile,
            documentEntityName,
            null,
            actionInput,
            function (res) {
                let timer = 0;
                let id = setInterval(check, 500)
                function check() {
                    if (timer >= downloadTimeoutValue) {
                        Xrm.Navigation.openAlertDialog({ text: messages.DownloadFailTimeOut });
                        clearInterval(id);
                        Xrm.Utility.closeProgressIndicator();
                    }
                    else {
                        Utils.Server.Retrieve(documentEntityName, documentId, "alt_file, alt_file_name", null,
                            function (result) {
                                if (result.alt_file != null) {
                                    clearInterval(id);
                                    Xrm.Utility.closeProgressIndicator();
                                    prepareFile(result.alt_file_name, documentId);
                                }
                            }, null)
                        timer += 0.5
                    }
                }
            },
            function () {
                Xrm.Utility.closeProgressIndicator();
                Xrm.Navigation.openAlertDialog({ text: messages.DownloadFail });
            });
    }

    const prepareFile = async function (fileName, documentId) {
        var startBytes = 0;
        var increment = 4194304; // 4mb
        var baseUrl = Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.1";
        var url = `${baseUrl}/${documentEntitySetName}(${documentId})/${fileAttributeName}?size=full`;
        var finalContent = "";
        var fileSize = 0;
        while (startBytes <= fileSize) {
            var result = await makeRequest("GET", url, startBytes, increment);
            var req = result.target;
            if (req.status === 206) {
                finalContent += JSON.parse(req.responseText).value.replace(/={1,2}$/, '');
                startBytes += increment;
                if (fileSize === 0) {
                    fileSize = req.getResponseHeader("x-ms-file-size");
                }
            }
        }

        finalContent = finalContent + Array((4 - finalContent.length % 4) % 4 + 1).join('=')

        const linkSource = `data:application/octet-stream;base64,${finalContent}`;
        const blob = dataURItoBlob(linkSource);
        const linkSourceBlob = URL.createObjectURL(blob);
        const downloadLink = document.createElement("a");

        document.body.appendChild(downloadLink);

        downloadLink.href = linkSourceBlob;
        downloadLink.target = '_self';
        downloadLink.download = fileName;
        downloadLink.click();

        document.body.removeChild(downloadLink);
    }

    const makeRequest = function (method, url, startBytes, increment) {
        return new Promise(function (resolve, reject) {
            var request = new XMLHttpRequest();
            request.open(method, url);
            request.setRequestHeader("Range", "bytes=" + startBytes + "-" + (startBytes + increment - 1));
            request.onload = resolve;
            request.onerror = reject;
            request.send();
        });
    }

    const dataURItoBlob = function (dataURI) {
        var byteString = atob(dataURI.split(',')[1]);
        var ab = new ArrayBuffer(byteString.length);
        var ia = new Uint8Array(ab);

        for (var i = 0; i < byteString.length; i++) {
            ia[i] = byteString.charCodeAt(i);
        }

        var bb = new Blob([ab]);
        return bb;
    }

    return {
        CreateDocument: createDocument,
        DownloadDocumentFromSubGrid: downloadDocumentFromSubGrid
    };
}());


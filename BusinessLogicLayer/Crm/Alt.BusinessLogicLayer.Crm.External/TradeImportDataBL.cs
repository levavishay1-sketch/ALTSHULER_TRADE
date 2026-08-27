using Alt.DataAccessLayer.Crm.External;
using Alt.DataAccessLayer.ExternalServices.ESB;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Models;
using Alt.Framework;
using Alt.Framework.External.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class TradeImportDataBL : ExternalBLBase
    {
        ETLLogMessageBlock importDataLogMessageBlock;
        public TradeImportDataBL(GlobalContext globalContext) : base(globalContext)
        {         
        }

        public ActionResult HandleSSISLogResult(ApiScheduledOperation apiScheduledOperation, ApiSchedulerSetup apiSchedulerSetup)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            ETLLogMessageBlock eTLLogMessageBlock = null;
            bool isExceedded = false;
            string filteredExecutionResult = new string(apiScheduledOperation.ExecutionResult?
                .Where(c => !char.IsControl(c)).ToArray());

            if (!string.IsNullOrWhiteSpace(filteredExecutionResult))
            {
                eTLLogMessageBlock = base.GetDeserializedContent<ETLLogMessageBlock>(filteredExecutionResult);
                eTLLogMessageBlock.ParseToHtml(out isExceedded);
                actionResult.ReturnObject = isExceedded ? eTLLogMessageBlock.htmlWithoutWarnings : eTLLogMessageBlock.html;

                if (!string.IsNullOrWhiteSpace(eTLLogMessageBlock.exception))
                {
                    actionResult.SetToFailedActionResult(CustomErrorCodes.PackageExecutionCompletedWithError);
                }
                else if (eTLLogMessageBlock.warnings != null
                    && eTLLogMessageBlock.warnings.Count > 0)
                {
                    actionResult.SetToFailedActionResult(CustomErrorCodes.DataReceptionCompletedWithWarnings);
                }
            }
            return actionResult;
        }

        internal ActionResult HandleImportFromFiles(ApiSchedulerSetup apiSchedulerSetup)
        {
            this.GlobalContext.LogEntry();

            ApiConfigurationDAL apiConfigurationDal = new ApiConfigurationDAL(this.GlobalContext);
            ApiConfiguration apiConfiguration = apiConfigurationDal.GetApiConfigurationByCode((int)ApiConfigurationCode.GetTradeImportZipFile);

            ESBFileImportDAL eSBFileImportDal = new ESBFileImportDAL(this.GlobalContext, apiConfiguration);
            ActionResult actionResult = eSBFileImportDal.ExecuteRequest(null);
            if (actionResult.IsSuccess)
            {
                this.importDataLogMessageBlock = new ETLLogMessageBlock()
                {
                    warnings = new List<ETLWarning>(),
                    counters = new List<ETLCounter>()
                };
                List<ImportDataMappingConfiguration> importDataMappingConfiguration;
                if (apiSchedulerSetup.TryGetSettingsItemValue<List<ImportDataMappingConfiguration>>(nameof(importDataMappingConfiguration), out importDataMappingConfiguration))
                {
                    try
                    {
                        var data = this.LoadData(actionResult.ReturnObject as Stream, importDataMappingConfiguration);
                        this.ImportDataToCRM(data);
                    }
                    catch (Exception ex)
                    {
                        importDataLogMessageBlock.exception = ex.ToString();
                        importDataLogMessageBlock.errorCode = ex.HResult;
                    }
                    finally
                    {
                        actionResult = this.GenerateImportDataReturnObject();
                    }
                }
                else
                {
                    actionResult.SetToFailedActionResult(CustomErrorCodes.ImportDataConfigurationsNotDefined);
                }
            }
            return actionResult;
        }

        private SortedDictionary<ImportDataMappingConfiguration, List<dynamic>> LoadData(Stream stream, List<ImportDataMappingConfiguration> importDataMappingConfiguration)
        {
            this.GlobalContext.LogEntry();

            SortedDictionary<ImportDataMappingConfiguration, List<dynamic>> data = new SortedDictionary<ImportDataMappingConfiguration, List<dynamic>>(new ImportDataMappingConfigurationComparer());
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read, false))
            {
                foreach (var entry in zip.Entries)
                {
                    string fileName = entry.Name;
                    var configuration = importDataMappingConfiguration
                        .FirstOrDefault(c => fileName.ToLower().Contains(c.FilePrefix.ToLower()));
                    if (configuration != null)
                    {
                        using (TextReader textReader = new StreamReader(entry.Open(), Encoding.GetEncoding(1255)))
                        {
                            List<dynamic> dynamicRecords = CsvUtils.LoadFromCsvStream(textReader);
                            data.Add(configuration, dynamicRecords);
                        }
                    }                 
                }
            }
            return data;
        }

        private void ImportDataToCRM(SortedDictionary<ImportDataMappingConfiguration, List<dynamic>> importData)
        {
            this.GlobalContext.LogEntry();

            foreach (var item in importData)
            {
                ImportDataMappingConfiguration configuration = item.Key;
                var dynamicDataList = item.Value;

                ETLCounter counter = new ETLCounter();
                List<ETLWarning> errors = new List<ETLWarning>();

                CommonDAL commonDal = new CommonDAL(this.GlobalContext, configuration.CrmEntityName);
                commonDal.UpsertDynamicList(dynamicDataList, configuration.EntityBuilderConfiguration, counter, errors);

                importDataLogMessageBlock.counters.Add(counter);
                importDataLogMessageBlock.warnings.AddRange(errors);
            }
        }

        public ActionResult GenerateImportDataReturnObject()
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            bool isExceedded;
            importDataLogMessageBlock.ParseToHtml(out isExceedded);
            actionResult.ReturnObject = isExceedded ? importDataLogMessageBlock.htmlWithoutWarnings : importDataLogMessageBlock.html;

            if (!string.IsNullOrWhiteSpace(importDataLogMessageBlock.exception))
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.PackageExecutionCompletedWithError);
            }
            else if (importDataLogMessageBlock.warnings != null
                && importDataLogMessageBlock.warnings.Count > 0)
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.DataReceptionCompletedWithWarnings);
            }

            return actionResult;
        }
    }
}

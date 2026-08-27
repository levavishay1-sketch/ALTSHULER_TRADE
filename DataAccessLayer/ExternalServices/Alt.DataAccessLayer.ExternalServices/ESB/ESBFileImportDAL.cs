using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices;
using Alt.Framework;
using Alt.Framework.Utils;
using System;
using System.IO;
using System.Net.Http;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBFileImportDAL: ExternalServicesBaseDAL<ExternalEntityBase, ApiEntity>
    {
        public ESBFileImportDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration) : base(globalContext, apiConfiguration)
        {
        }

        protected override ExternalEntityBase MapApiEntityToTargetModel(ApiEntity apiEntity)
        {
            this.GlobalContext.LogEntry();
            return null;
        }

        protected override ActionResult CreateActionResultByHttpResponseMessage(HttpResponseMessage response)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            try
            {
                if (!response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    base.LogResponse(response, content);
                    actionResult.SetToFailedActionResult(CustomErrorCodes.FailedToGetImportFileError, new[] { content });
                }
                else
                {
                    base.LogResponse(response, null);
                    actionResult.ReturnObject = response.Content.ReadAsStreamAsync().Result;
                    var fileContent = actionResult.ReturnObject?.ToString();
                    if (string.IsNullOrWhiteSpace(fileContent))
                    {
                        actionResult.SetToFailedActionResult(CustomErrorCodes.NoImportFileReceivedError);
                    }
                }
            }
            catch (Exception ex)
            {
                actionResult.SetToFailedActionResult(ex.HResult, new string[] { ex.Message });
                this.GlobalContext.Log.Critical(ex.ToString());
            }

            return actionResult;
        }

        protected override object GetDebugModeResponse()
        {
            this.GlobalContext.Log.Warning($"{Environment.NewLine}!!!Api is in DebugMode!!!{Environment.NewLine}");
            Stream stream = null;
            string debugModeResponse;
            if (this.ApiConfiguration != null
                && this.ApiConfiguration.TryGetSettingsItemValue<string>(nameof(debugModeResponse), out debugModeResponse)
                && debugModeResponse != null)
            {
                stream = FileUtils.ReadFileAsStream(debugModeResponse);
                LogResponse(null, debugModeResponse);
            }
            return stream;

        }
    }
}

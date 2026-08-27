using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Alt.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alt.BusinessLogicLayer.Crm.External.DigitalJoiningForms
{
    public class OperationalIncidentDigitalFormBL : DigitalJoiningFormBaseBL
    {
        const string operationSystemDigitalFormComplitedStatusParameterName = "OperationSystemFormCompletedStatusCode";
        public OperationalIncidentDigitalFormBL(GlobalContext globalContext, ApiConfiguration apiConfiguration) : base(globalContext, apiConfiguration)
        {
            this.DigitalFormComplitedStatus = this.GlobalContext.CacheManager.GetGlobalParameter<string>(operationSystemDigitalFormComplitedStatusParameterName);
        }

        internal override ActionResult ConstractData(ApiDigitalForm apiDigitalForm, string joiningProcessNumber = null)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();
            var digitalFormToUpdate = new ApiDigitalForm() { Id = apiDigitalForm.Id };
            DataReceptionStatusCode dataReceptionStatus = DataReceptionStatusCode.Success;
            try
            {
                if (joiningProcessNumber == null)
                {
                    CommonDAL commonDal = new CommonDAL(this.GlobalContext, ApiIncident.EntityLogicalName);
                    apiDigitalForm.RegardingIncident.SourceSystemCode = apiDigitalForm.SourceSystemCode;
                    apiDigitalForm.RegardingIncident.Id = commonDal.Create(apiDigitalForm.RegardingIncident);
                    apiDigitalForm.RegardingObject = apiDigitalForm.RegardingIncident;

                    HandleOperationalDetailsForIncident(apiDigitalForm);

                    digitalFormToUpdate.Subject = apiDigitalForm.DigitalFormIdentityNumber;
                    digitalFormToUpdate.RegardingObject = apiDigitalForm.RegardingObject;
                    digitalFormToUpdate.Customers = this.GetCustomers(apiDigitalForm.RegardingObject);                 
                }
            }
            catch (Exception ex)
            {
                dataReceptionStatus = DataReceptionStatusCode.Failed;
                this.GlobalContext.Log.Critical(ex.ToString());
                actionResult.SetToFailedActionResult(ex.Message);
            }
            finally
            {
                DigitalFormDAL digitalFormDal = new DigitalFormDAL(this.GlobalContext);
                digitalFormToUpdate.DataReceptionStatusCode = (int)dataReceptionStatus;
                digitalFormToUpdate.DigitalFormDetails = apiDigitalForm.ToString();
                digitalFormDal.Update(digitalFormToUpdate);
            }

            return actionResult;
        }

        private List<ApiActivityParty> GetCustomers(ApiEntityBase apiIncident)
        {
            this.GlobalContext.LogEntry();

            var incident = new IncidentDAL(this.GlobalContext)
                .GetByAttribute("incidentid", apiIncident.Id.Value, new string[] { "customerid" })
                .FirstOrDefault();
            return
                     new List<ApiActivityParty>
                     {
                            new ApiActivityParty(incident.Customer.LogicalName)
                            {
                                Id = incident.Customer.Id
                            }
                     };
        }

        private void HandleOperationalDetailsForIncident(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();

            if (!string.IsNullOrWhiteSpace(apiDigitalForm.RegardingIncident.OperationalDetails))
            {
                ApiDigitalFormTemplate template = GetDigitalFormTemplate(apiDigitalForm);
                if (template != null)
                {
                    this.HandleOperationalDetailsByTemplate(apiDigitalForm, template);
                }
                else
                {
                    this.GlobalContext.Log.Warning("DigitalFormTemplate not found. Skipping operational mapping and PCF generation.");
                }
            }
            else
            {
                this.GlobalContext.Log.Info("No OperationalDetails found on RegardingIncident. Skipping operational handling.");

            }
        }

        private void HandleOperationalDetailsByTemplate(ApiDigitalForm apiDigitalForm, ApiDigitalFormTemplate template)
        {
            this.GlobalContext.LogEntry();
            if (!string.IsNullOrWhiteSpace(template.MappedEntityLogicalName))
            {
                this.HandleOperationalProcess(apiDigitalForm, template);
            }
            else
            {
                this.HandleDynamicFormPcfConfiguration(apiDigitalForm, template);
            }
        }

        private void HandleDynamicFormPcfConfiguration(ApiDigitalForm apiDigitalForm, ApiDigitalFormTemplate template)
        {
            this.GlobalContext.LogEntry();

            string pcfConfigJson = BuildPcfConfigJsonForIncident(apiDigitalForm.RegardingIncident, template);
            this.UpdateIncident(new ApiIncident
            {
                Id = apiDigitalForm.RegardingObject.Id,
                DynamicFormPcfConfigJson = pcfConfigJson
            });
        }

        private void HandleOperationalProcess(ApiDigitalForm apiDigitalForm, ApiDigitalFormTemplate template)
        {
            this.GlobalContext.LogEntry();

            var operationalProcess = CreateMappedEntityFromOperationalDetails(apiDigitalForm, template);
            operationalProcess.Customer = apiDigitalForm.RegardingIncident.Customer;

            apiDigitalForm.RegardingIncident.OperationalProcess = operationalProcess;

            operationalProcess.Id = new CommonDAL(this.GlobalContext, ApiWithdrawalRequest.EntityLogicalName).Create(operationalProcess);
            this.UpdateIncident(new ApiIncident() 
            { 
                Id = apiDigitalForm.RegardingObject.Id, 
                OperationalProcess = operationalProcess,
                Portfolio = operationalProcess.Portfolio
            });
        }

        private ApiDigitalFormTemplate GetDigitalFormTemplate(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            ApiDigitalFormTemplate template = null;
            string templateCode = apiDigitalForm.DigitalFormTemplate.Code;

            if (templateCode != null)
            {
                DigitalFormTemplateDAL templateDal = new DigitalFormTemplateDAL(this.GlobalContext);
                template = templateDal.GetActiveByAttribute<string>("alt_code", apiDigitalForm.DigitalFormTemplate.Code, new string[] { })
                    .FirstOrDefault();
            }

            return template;
        }

        private ApiOperationalProcess CreateMappedEntityFromOperationalDetails(ApiDigitalForm apiDigitalForm, ApiDigitalFormTemplate template)
        {
            ApiOperationalProcess operationalProcess = null;
            try
            {
                switch (template.MappedEntityLogicalName)
                {
                    case ApiWithdrawalRequest.EntityLogicalName:
                        {
                            operationalProcess = this.GetDeserializedContent<ApiWithdrawalRequest>(apiDigitalForm.RegardingIncident.OperationalDetails);
                            break;
                        }
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                this.GlobalContext.Log.Error($"Failed to Create Operational Process Entity. Error: {ex}");
            }
            return operationalProcess;
        }

        private string BuildPcfConfigJsonForIncident(ApiIncident apiIncident, ApiDigitalFormTemplate template)
        {
            this.GlobalContext.LogEntry();
            string result = string.Empty;

            var operationalDetails = apiIncident.OperationalDetails.ToDictionary<string, object>();
            PcfSettings pcfSettings = this.GetDeserializedContent<PcfSettings>(template.Configurations);

            foreach (var item in operationalDetails)
            {
                var control = pcfSettings.Controls.Where(c => c.externalName == item.Key).FirstOrDefault();
                this.FillControlValueFromOperationalDetails(control, operationalDetails);
            }

            return result;
        }

        private void UpdateIncident(ApiIncident apiIncident)
        {
            this.GlobalContext.LogEntry();

            IncidentBL incidentBl = new IncidentBL(this.GlobalContext);
            incidentBl.Update(apiIncident);
        }


        private void FillControlValueFromOperationalDetails(Control control, Dictionary<string, object> operationalDetails)
        {
          
        }
    }
}

public class PcfSettings
{
    public List<Control> Controls { get; set; }
}

public class Control
{
    public string logicalName { get; set; }
    public string externalName { get; set; }
    public string label { get; set; }
    public int type { get; set; }
    public List<Row> grid { get; set; }
    public Dictionary<int, string> options { get; set; }
    public object value { get; set; }
}
public class Row
{
    public List<Control> columns { get; set; }
}





using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Models;
using Alt.DataModel.ExernalServices.Enums;
using Alt.Framework;
using Alt.Framework.Extensions;
using Alt.Framework.Utils;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Alt.BusinessLogicLayer.Crm.External.Reports
{
    public class DepositsReportBL : ExternalBLBase
    {
        const string seperator = ",";
        const string endOfLine = "";

        EntityMetadata depositMetadata;
        Dictionary<string, string> depositProperties;

        public DepositsReportBL(GlobalContext globalContext) : base(globalContext) { }

        public ActionResult HandleDailyDepositsReport(ApiScheduledOperation apiScheduledOperation, ApiSchedulerSetup retrievedSchedulerSetup)
        {
            this.GlobalContext.LogEntry();

            ActionResult actionResult = new ActionResult();

            int? daysBack = retrievedSchedulerSetup.TryGetSettingsItemValue(nameof(daysBack), out daysBack)
                ? daysBack : 0;

            string firstPageName = retrievedSchedulerSetup.TryGetSettingsItemValue(nameof(firstPageName), out firstPageName)
                ? firstPageName : "גיליון 1";
            string secondPageName = retrievedSchedulerSetup.TryGetSettingsItemValue(nameof(secondPageName), out secondPageName)
                ? secondPageName : "גיליון 2";
            string thirdPageName = retrievedSchedulerSetup.TryGetSettingsItemValue(nameof(thirdPageName), out thirdPageName)
                ? thirdPageName : "גיליון 3";

            string[] firstPageHeaders = retrievedSchedulerSetup.TryGetSettingsItemValue(nameof(firstPageHeaders), out firstPageHeaders)
                ? firstPageHeaders : new string[] { };
            string[] secondPageHeaders = retrievedSchedulerSetup.TryGetSettingsItemValue(nameof(secondPageHeaders), out secondPageHeaders)
                ? secondPageHeaders : new string[] { };
            string[] thirdPageHeaders = retrievedSchedulerSetup.TryGetSettingsItemValue(nameof(thirdPageHeaders), out thirdPageHeaders)
                ? thirdPageHeaders : new string[] { };

            string fileNamePrefix = retrievedSchedulerSetup.TryGetSettingsItemValue(nameof(fileNamePrefix), out fileNamePrefix)
                ? fileNamePrefix : "דוח הפקדות לתאריכים";
            string fileNameDateFormat = retrievedSchedulerSetup.TryGetSettingsItemValue(nameof(fileNameDateFormat), out fileNameDateFormat)
                ? fileNameDateFormat : "yyyy_MM_dd";

            DateTime endDate = DateTime.Today.AddDays(1).AddTicks(-1);
            DateTime startDate = DateTime.Today.AddDays(-daysBack.Value);
            string fileName = $"{fileNamePrefix} {startDate.Date.ToString(fileNameDateFormat)} - {endDate.Date.ToString(fileNameDateFormat)}.xlsx";

            DepositDAL depositDAL = new DepositDAL(this.GlobalContext);
            List<ApiDeposit> deposits = depositDAL.GetDepositsByXMLDateRange(startDate, endDate);

            depositMetadata = depositDAL.GetEntityMetadata(ApiDeposit.EntityLogicalName);
            depositProperties = deposits != null && deposits.Count > 0 ? deposits[0].GetProperties() : null;

            byte[] firstPageCSV = GenerateFirstPageCSV(deposits, firstPageHeaders);
            byte[] secondPageCSV = GenerateSecondPageCSV(deposits, secondPageHeaders);
            byte[] thirdPageCSV = GenerateThirdPageCSV(deposits, thirdPageHeaders);
            byte[] xlsxBytes = ExcelUtils.ConvertToExcel(firstPageCSV, secondPageCSV, thirdPageCSV, firstPageName, secondPageName, thirdPageName);
            actionResult = this.CreateEmailWithAttachmentAndSend(apiScheduledOperation, retrievedSchedulerSetup, xlsxBytes, fileName);

            return actionResult;
        }

        private byte[] GenerateFirstPageCSV(List<ApiDeposit> deposits, string[] firstPageHeaders)
        {
            this.GlobalContext.LogEntry();
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
                {
                    writer.WriteLine(string.Join(seperator, firstPageHeaders));
                    if (deposits != null && deposits.Count > 0)
                    {
                        foreach (ApiDeposit deposit in deposits)
                        {
                            string currency = GetOptionSetLabel(depositMetadata, depositProperties[nameof(deposit.CurrencyCode)], deposit.CurrencyCode);
                            string matchToDigitalFormVerification = GetOptionSetLabel(depositMetadata, depositProperties[nameof(deposit.MatchForDigitalFormVerificationCode)], deposit.MatchForDigitalFormVerificationCode);
                            string digitalFormVerificationStatus = GetOptionSetLabel(depositMetadata, depositProperties[nameof(deposit.DigitalFormVerificationStatusCode)], deposit.DigitalFormVerificationStatusCode);
                            string matchPortfolio = GetOptionSetLabel(depositMetadata, depositProperties[nameof(deposit.MatchForPortfolioCode)], deposit.MatchForPortfolioCode);
                            string shenhavStatus1 = GetOptionSetLabel(depositMetadata, depositProperties[nameof(deposit.ShenhavStatusCode)], deposit.ShenhavStatusCode);
                            string shenhavStatus2 = GetOptionSetLabel(depositMetadata, depositProperties[nameof(deposit.FirstCreatedPortfolioShenhavStatusCode)], deposit.FirstCreatedPortfolioShenhavStatusCode);

                            DateTime? valueDate = null;
                            if (deposit.ValueDate.HasValue)
                            {
                                valueDate = deposit.ValueDate.Value;
                            }

                            DateTime? automaticLaunchShenhavPortfolioDate = null;
                            if (deposit.AutomaticLaunchShenhavPortfolioDate.HasValue)
                            {
                                automaticLaunchShenhavPortfolioDate = deposit.AutomaticLaunchShenhavPortfolioDate.Value;
                            }

                            writer.Write($"{valueDate}".Escape() + seperator);
                            writer.Write($"{deposit.DepositAmount}".Escape() + seperator);
                            writer.Write($"{currency}".Escape() + seperator);
                            writer.Write($"{deposit.ReferenceNumberInBank}".Escape() + seperator);
                            writer.Write($"{deposit.BankAccountName}".Escape() + seperator);
                            writer.Write($"{deposit.CRMOppositeBankNumber}".Escape() + seperator);
                            writer.Write($"{deposit.OpposingBranchNumber}".Escape() + seperator);
                            writer.Write($"{deposit.OpposingAccountNumber}".Escape() + seperator);
                            writer.Write($"{matchToDigitalFormVerification}".Escape() + seperator);
                            writer.Write($"{deposit.DigitalFormNumber}".Escape() + seperator);
                            writer.Write($"{digitalFormVerificationStatus}".Escape() + seperator);
                            writer.Write($"{deposit.FirstCreatedDigitalFormNumber}".Escape() + seperator);
                            writer.Write($"{deposit.BeneficiaryAccountHolder}".Escape() + seperator);
                            writer.Write($"{matchPortfolio}".Escape() + seperator);
                            writer.Write($"{deposit.ShenhavAccountNumber}".Escape() + seperator);
                            writer.Write($"{shenhavStatus1}".Escape() + seperator);
                            writer.Write($"{deposit.FirstCreatedShenhavAccountNumber}".Escape() + seperator);
                            writer.Write($"{shenhavStatus2}".Escape() + seperator);
                            writer.Write($"{ConvertBooleanToString(deposit.AutomaticLaunchedShenhavPortfolio)}".Escape() + seperator);
                            writer.Write($"{automaticLaunchShenhavPortfolioDate}".Escape() + seperator);
                            writer.Write($"{ConvertBooleanToString(deposit.DepositAmountBelow5000)}".Escape() + seperator);
                            writer.Write(seperator);
                            writer.Write(seperator);
                            writer.Write(endOfLine);

                            writer.WriteLine();
                        }
                    }
                    writer.Flush();
                }
                return memoryStream.ToArray();
            }
        }

        private byte[] GenerateSecondPageCSV(List<ApiDeposit> deposits, string[] secondPageHeaders)
        {
            this.GlobalContext.LogEntry();
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
                {
                    writer.WriteLine(string.Join(seperator, secondPageHeaders));
                    if (deposits != null && deposits.Count > 0)
                    {
                        foreach (ApiDeposit deposit in deposits)
                        {
                            bool hasDigitalFormVerificationMatch = deposit.MatchForDigitalFormVerificationCode != null
                                && deposit.MatchForDigitalFormVerificationCode.Value == (int)RecordMatchCode.Yes;

                            bool hasPortfolioaMatch = deposit.MatchForPortfolioCode != null
                                && deposit.MatchForPortfolioCode.Value == (int)RecordMatchCode.Yes;

                            DateTime? valueDate = null;
                            if (deposit.ValueDate.HasValue)
                            {
                                valueDate = deposit.ValueDate.Value;
                            }

                            if ((hasDigitalFormVerificationMatch && !deposit.AutomaticLaunchedShenhavPortfolio.Value) || hasPortfolioaMatch)
                            {
                                writer.Write($"{valueDate}".Escape() + seperator);
                                writer.Write($"{deposit.DigitalFormNumber}".Escape() + seperator);
                                writer.Write($"{deposit.ShenhavAccountNumber}".Escape() + seperator);
                                writer.Write($"{deposit.MainAccountHolderIdentificationNumber}".Escape() + seperator);
                                writer.Write($"{deposit.BankAccountName}".Escape() + seperator);
                                writer.Write($"{deposit.MainAccountHolder}".Escape() + seperator);
                                writer.Write($"{deposit.BeneficiaryAccountHolder}{endOfLine}".Escape());

                                writer.WriteLine();
                            }
                        }
                    }
                    writer.Flush();
                }
                return memoryStream.ToArray();
            }
        }

        private byte[] GenerateThirdPageCSV(List<ApiDeposit> deposits, string[] secondPageHeaders)
        {
            this.GlobalContext.LogEntry();
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
                {
                    writer.WriteLine(string.Join(seperator, secondPageHeaders));
                    if (deposits != null && deposits.Count > 0)
                    {
                        foreach (ApiDeposit deposit in deposits)
                        {
                            DateTime? valueDate = null;
                            if (deposit.ValueDate.HasValue)
                            {
                                valueDate = deposit.ValueDate.Value;
                            }

                            if (deposit.AutomaticLaunchedShenhavPortfolio.Value)
                            {
                                writer.Write($"{valueDate}".Escape() + seperator);
                                writer.Write($"{deposit.DigitalFormNumber}".Escape() + seperator);
                                writer.Write($"{deposit.MainAccountHolderIdentificationNumber}".Escape() + seperator);
                                writer.Write($"{deposit.BankAccountName}".Escape() + seperator);
                                writer.Write($"{deposit.MainAccountHolder}".Escape() + seperator);
                                writer.Write($"{deposit.BeneficiaryAccountHolder}{endOfLine}".Escape());

                                writer.WriteLine();
                            }
                        }
                    }
                    writer.Flush();
                }
                return memoryStream.ToArray();
            }
        }

        private string ConvertBooleanToString(bool? value)
        {
            string valueToReturn = string.Empty;
            if (value != null)
            {
                valueToReturn = ((YesNoCode)Convert.ToInt32(value)).GetDescriptionAttribute();
            }
            return valueToReturn;
        }

        private string GetOptionSetLabel(EntityMetadata entityMetadata, string fieldName, int? optionValue)
        {
            string label = string.Empty;
            if (optionValue != null)
            {
                var attribute = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == fieldName);
                if (attribute != null)
                {
                    if (attribute is EnumAttributeMetadata enumMetadata)
                    {
                        var option = enumMetadata.OptionSet.Options
                            .FirstOrDefault(o => o.Value == optionValue);

                        label = option?.Label?.UserLocalizedLabel?.Label ?? string.Empty;
                    }
                }
            }
            return label;
        }

        private ActionResult CreateEmailWithAttachmentAndSend(ApiScheduledOperation apiScheduledOperation, ApiSchedulerSetup retrievedSchedulerSetup, byte[] bodyAsBytes, string fileName)
        {
            this.GlobalContext.LogEntry();

            EmailSettings emailSettings = retrievedSchedulerSetup.TryGetSettingsItemValue(nameof(emailSettings), out emailSettings)
                ? emailSettings : null;

            EmailBL emailBL = new EmailBL(this.GlobalContext);
            emailSettings.Attachments[0].FileBody = Convert.ToBase64String(bodyAsBytes);
            emailSettings.Attachments[0].FileName = fileName;
            emailSettings.Regarding = apiScheduledOperation;
            ActionResult actionResult = emailBL.CreateEmailByEmailSettings(emailSettings);
            return actionResult;
        }
    }
}

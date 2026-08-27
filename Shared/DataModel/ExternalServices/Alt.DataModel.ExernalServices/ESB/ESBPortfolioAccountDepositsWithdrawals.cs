using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataModel.ExernalServices.ESB
{

    public class ESBPortfolioAccountDepositsWithdrawals : ExternalEntityBase
    {
        private int? bank;
        /// <summary>
        /// מספר בנק
        /// </summary>
        public int? Bank
        {
            get => this.bank;
            set
            {
                base.SetProperty(value);
                this.bank = value;
            }
        }

        private int? branch;
        /// <summary>
        /// סניף
        /// </summary>
        public int? Branch
        {
            get => this.branch;
            set
            {
                base.SetProperty(value);
                this.branch = value;
            }
        }

        private string clientAccountNumber;
        /// <summary>
        /// מספר חשבון לקוח - הפקדות ומשיכות במערכת סגורה/פתוחה
        /// </summary>
        public string ClientAccountNumber
        {
            get => this.clientAccountNumber;
            set
            {
                base.SetProperty(value);
                this.clientAccountNumber = value;
            }
        }

        /// <summary>
        /// שם חשבון לקוח- הפקדות ומשיכות במערכת סגורה/פתוחה
        /// </summary>
        public string ClientAccountName { get; set; }

        /// <summary>
        /// שם משפחה חשבון לקוח- הפקדות ומשיכות במערכת סגורה/פתוחה
        /// </summary>
        public string ClientAccountLName { get; set; }
    }
}

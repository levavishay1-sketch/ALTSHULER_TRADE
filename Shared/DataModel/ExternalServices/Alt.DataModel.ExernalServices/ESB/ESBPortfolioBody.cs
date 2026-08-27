using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBPortfolioBody : ExternalEntityBase
    {
        private ESBPortfolioGeneral general;
        [Required]
        public ESBPortfolioGeneral General
        {
            get => this.general;
            set
            {
                base.SetProperty(value);
                this.general = value;
            }
        }

        private ESBPortfolioEntitlements accountEntitlements;
        [Required]
        public ESBPortfolioEntitlements AccountEntitlements
        {
            get => this.accountEntitlements;
            set
            {
                base.SetProperty(value);
                this.accountEntitlements = value;
            }
        }

        private ESBPortfolioFrames accountFrames;
        [Required]
        public ESBPortfolioFrames AccountFrames
        {
            get => this.accountFrames;
            set
            {
                base.SetProperty(value);
                this.accountFrames = value;
            }
        }

        private List<PortfolioBeneficiary> accountBeneficiaries;
        [Required]
        public List<PortfolioBeneficiary> AccountBeneficiaries
        {
            get => this.accountBeneficiaries;
            set
            {
                base.SetProperty(value);
                this.accountBeneficiaries = value;
            }
        }

        private List<ESBPortfolioAccountDepositsWithdrawals> accountDepositsWithdrawals;
        /// <summary>
        /// חשבונות להפקדה
        /// </summary>
        public List<ESBPortfolioAccountDepositsWithdrawals> AccountDepositsWithdrawals
        {
            get => this.accountDepositsWithdrawals;
            set
            {
                base.SetProperty(value);
                this.accountDepositsWithdrawals = value;
            }
        }
    }
}

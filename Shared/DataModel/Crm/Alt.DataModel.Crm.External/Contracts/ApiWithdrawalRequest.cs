using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiWithdrawalRequest : ApiOperationalProcess
    {
        public const string EntityLogicalName = "alt_withdrawalrequest";
        public ApiWithdrawalRequest() : base(EntityLogicalName) { }

        private string bankAccountNumber;
        [CrmEntityMapper("alt_bankaccountnumber", CrmPropertyType.String)]
        public string BankAccountNumber
        {
            get
            {
                return bankAccountNumber;
            }
            set
            {
                this.SetProperty(value);
                this.bankAccountNumber = value;
            }
        }

        private string withdrawalReasonDetails;
        [CrmEntityMapper("alt_withdrawalreasondetails", CrmPropertyType.String)]
        public string WithdrawalReasonDetails
        {
            get
            {
                return withdrawalReasonDetails;
            }
            set
            {
                this.SetProperty(value);
                this.withdrawalReasonDetails = value;
            }
        }

        private int? withdrawalTypeCode;
        [CrmEntityMapper("alt_withdrawaltypecode", CrmPropertyType.OptionSet)]
        public int? WithdrawalTypeCode
        {
            get
            {
                return this.withdrawalTypeCode;
            }
            set
            {
                this.SetProperty(value);
                this.withdrawalTypeCode = value;
            }
        }

        private int? withdrawalReasonCode;
        [CrmEntityMapper("alt_withdrawalreasoncode", CrmPropertyType.OptionSet)]
        public int? WithdrawalReasonCode
        {
            get
            {
                return this.withdrawalReasonCode;
            }
            set
            {
                this.SetProperty(value);
                this.withdrawalReasonCode = value;
            }
        }

        private decimal? currency1Amount;
        [CrmEntityMapper("alt_currency1amountdcml", CrmPropertyType.Decimal)]
        public decimal? Currency1Amount
        {
            get
            {
                return currency1Amount;
            }
            set
            {
                this.SetProperty(value);
                this.currency1Amount = value;
            }
        }

        private decimal? currency2Amount;
        [CrmEntityMapper("alt_currency2amountdcml", CrmPropertyType.Decimal)]
        public decimal? Currency2Amount
        {
            get
            {
                return currency2Amount;
            }
            set
            {
                this.SetProperty(value);
                this.currency2Amount = value;
            }
        }

        private bool? currency1AutoConvertBit;
        [CrmEntityMapper("alt_currency1autoconvertbit", CrmPropertyType.Bool)]
        public bool? Currency1AutoConvertBit
        {
            get
            {
                return currency1AutoConvertBit;
            }
            set
            {
                this.SetProperty(value);
                this.currency1AutoConvertBit = value;
            }
        }

        private bool? currency2AutoConvertBit;
        [CrmEntityMapper("alt_currency2autoconvertbit", CrmPropertyType.Bool)]
        public bool? Currency2AutoConvertBit
        {
            get
            {
                return currency2AutoConvertBit;
            }
            set
            {
                this.SetProperty(value);
                this.currency2AutoConvertBit = value;
            }
        }

        private bool? withdrawToExistingAccountBit;
        [CrmEntityMapper("alt_withdrawtoexistingaccountbit", CrmPropertyType.Bool)]
        public bool? WithdrawToExistingAccountBit
        {
            get
            {
                return withdrawToExistingAccountBit;
            }
            set
            {
                this.SetProperty(value);
                this.withdrawToExistingAccountBit = value;
            }
        }

        private bool? accountAuthorizationAttachedBit;
        [CrmEntityMapper("alt_accountauthorizationattachedbit", CrmPropertyType.Bool)]
        public bool? AccountAuthorizationAttachedBit
        {
            get
            {
                return accountAuthorizationAttachedBit;
            }
            set
            {
                this.SetProperty(value);
                this.accountAuthorizationAttachedBit = value;
            }
        }

        private bool? clientApprovedWithdrawalDetailsBit;
        [CrmEntityMapper("alt_clientapprovedwithdrawaldetailsbit", CrmPropertyType.Bool)]
        public bool? ClientApprovedWithdrawalDetailsBit
        {
            get
            {
                return clientApprovedWithdrawalDetailsBit;
            }
            set
            {
                this.SetProperty(value);
                this.clientApprovedWithdrawalDetailsBit = value;
            }
        }

        private int? currency1TypeCode;
        [CrmEntityMapper("alt_currency1typecode", CrmPropertyType.OptionSet)]
        public int? Currency1TypeCode
        {
            get
            {
                return this.currency1TypeCode;
            }
            set
            {
                this.SetProperty(value);
                this.currency1TypeCode = value;
            }
        }

        private int? currency2TypeCode;
        [CrmEntityMapper("alt_currency2typecode", CrmPropertyType.OptionSet)]
        public int? Currency2TypeCode
        {
            get
            {
                return this.currency2TypeCode;
            }
            set
            {
                this.SetProperty(value);
                this.currency2TypeCode = value;
            }
        }

        private ApiBank bank;
        /// <summary>
        /// הבנק
        /// </summary>
        [CrmEntityMapper("alt_bankid", CrmPropertyType.EntityReference)]
        public ApiBank Bank
        {
            get
            {
                return this.bank;
            }
            set
            {
                this.SetProperty(value);
                this.bank = value;
            }
        }

        private ApiBranch branch;
        /// <summary>
        /// סניף
        /// </summary>
        [CrmEntityMapper("alt_branchid", CrmPropertyType.EntityReference)]
        public ApiBranch Branch
        {
            get
            {
                return branch;
            }
            set
            {
                this.SetProperty(value);
                this.branch = value;
            }
        }

        private bool? foreignCurrencyExistingBit;
        /// <summary>
        /// מט"ח קיים
        /// </summary>
        [CrmEntityMapper("alt_foreigncurrencyexistingbit", CrmPropertyType.Bool)]
        public bool? ForeignCurrencyExistingBit
        {
            get
            {
                return foreignCurrencyExistingBit;
            }
            set
            {
                this.SetProperty(value);
                this.foreignCurrencyExistingBit = value;
            }
        }
    }
}

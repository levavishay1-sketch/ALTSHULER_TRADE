using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataModel.Crm.External.Contracts
{
    /// <summary>
    /// ניהול האישורים
    /// </summary>
    public class ApiAuthorizationManagement : ApiEntity
    {
        public ApiAuthorizationManagement() : base(EntityLogicalName)
        {
        }

        public const string EntityLogicalName = "alt_authorizationmanagement";

        private bool? shortSaleRequestApprovalBit;
        /// <summary>
        /// אישור בקשת מכירת ני"ע בחסר
        /// </summary>
        [CrmEntityMapper("alt_shortsalerequestapprovalbit", CrmPropertyType.Bool)]
        public bool? ShortSaleRequestApprovalBit
        {
            get
            {
                return shortSaleRequestApprovalBit;
            }
            set
            {
                this.SetProperty(value);
                this.shortSaleRequestApprovalBit = value;
            }
        }

        private int? optinExerciseRequestApprovalCode;
        /// <summary>
        /// אישור בקשה לכתיבת אופציות
        /// </summary>
        [CrmEntityMapper("alt_optinexerciserequestapprovalcode", CrmPropertyType.OptionSet)]
        public int? OptinExerciseRequestApprovalCode
        {
            get
            {
                return optinExerciseRequestApprovalCode;
            }
            set
            {
                this.SetProperty(value);
                this.optinExerciseRequestApprovalCode = value;
            }
        }

        private int? creditRequestCode;
        /// <summary>
        /// אישור בקשת אשראי כספי
        /// </summary>
        [CrmEntityMapper("alt_creditrequestcode", CrmPropertyType.OptionSet)]
        public int? CreditRequestCode
        {
            get
            {
                return creditRequestCode;
            }
            set
            {
                this.SetProperty(value);
                this.creditRequestCode = value;
            }
        }

        private int? capitalRiskLevelAccountCode;
        /// <summary>
        /// דרגת סיכון הלבנת הון לחשבון
        /// </summary>
        [CrmEntityMapper("alt_capitalrisklevelaccountcode", CrmPropertyType.OptionSet)]
        public int? CapitalRiskLevelAccountCode
        {
            get
            {
                return capitalRiskLevelAccountCode;
            }
            set
            {
                this.SetProperty(value);
                this.capitalRiskLevelAccountCode = value;
            }
        }

        private decimal? creditAmountNIS;
        /// <summary>
        /// סכום אשראי מאושר בשקלים
        /// מסגרת אשראי כספי מאושר
        /// </summary>
        [CrmEntityMapper("alt_creditamountnismny", CrmPropertyType.Money)]
        public decimal? CreditAmountNIS
        {
            get
            {
                return creditAmountNIS;
            }
            set
            {
                this.SetProperty(value);
                this.creditAmountNIS = value;
            }
        }

        private decimal? lineAggregateCreditLimit;
        /// <summary>
        /// מסגרת אשראי מצרפי
        /// </summary>
        [CrmEntityMapper("alt_lineaggregatecreditlimitmny", CrmPropertyType.Money)]
        public decimal? LineAggregateCreditLimit
        {
            get
            {
                return lineAggregateCreditLimit;
            }
            set
            {
                this.SetProperty(value);
                this.lineAggregateCreditLimit = value;
            }
        }

        private decimal? lineStockShort;
        /// <summary>
        /// מסגרת שורט בני"ע
        /// </summary>
        [CrmEntityMapper("alt_linestockshortmny", CrmPropertyType.Money)]
        public decimal? LineStockShort
        {
            get
            {
                return lineStockShort;
            }
            set
            {
                this.SetProperty(value);
                this.lineStockShort = value;
            }
        }

        private decimal? lineWriteOptions;
        /// <summary>
        /// מסגרת כתיבת נגזרים מאושרת
        /// </summary>
        [CrmEntityMapper("alt_linewriteoptionsmny", CrmPropertyType.Money)]
        public decimal? LineWriteOptions
        {
            get
            {
                return lineWriteOptions;
            }
            set
            {
                this.SetProperty(value);
                this.lineWriteOptions = value;
            }
        }

        private int? lineAggregateCreditLimitPercent;
        /// <summary>
        /// מסגרת אשראי מצרפי כאחוז משווי תיק
        /// </summary>
        [CrmEntityMapper("alt_lineaggregatecreditlimitpercentint", CrmPropertyType.Int)]
        public int? LineAggregateCreditLimitPercent
        {
            get
            {
                return lineAggregateCreditLimitPercent;
            }
            set
            {
                this.SetProperty(value);
                this.lineAggregateCreditLimitPercent = value;
            }
        }

        private ApiDigitalFormVerification digitalFormVerification;
        /// <summary>
        /// מספר טופס דיגיטלי
        /// </summary>
        [CrmEntityMapper("alt_digitalformverificationid", CrmPropertyType.EntityReference)]
        public ApiDigitalFormVerification DigitalFormVerification
        {
            get
            {
                return digitalFormVerification;
            }
            set
            {
                this.SetProperty(value);
                this.digitalFormVerification = value;
            }
        }

    }
}

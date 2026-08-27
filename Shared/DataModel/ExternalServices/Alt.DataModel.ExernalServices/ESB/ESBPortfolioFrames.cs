using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBPortfolioFrames : ExternalEntityBase
    {
        private int? frOverdraftFinancialCredit;
        /// <summary>
        /// משיכת יתר (אשראי כספי)
        /// </summary>
        [Required]
        public int? FrOverdraftFinancialCredit
        {
            get => this.frOverdraftFinancialCredit;
            set
            {
                base.SetProperty(value);
                this.frOverdraftFinancialCredit = value;
            }
        }

        private int? frWriteOptions;
        /// <summary>
        /// כתיבה בנגזרים
        /// </summary>
        [Required]
        public int? FrWriteOptions
        {
            get => this.frWriteOptions;
            set
            {
                base.SetProperty(value);
                this.frWriteOptions = value;
            }
        }

        private int? frStockShort;
        /// <summary>
        /// שורט בני"ע
        /// </summary>
        [Required]
        public int? FrStockShort
        {
            get => this.frStockShort;
            set
            {
                base.SetProperty(value);
                this.frStockShort = value;
            }
        }

        private int? frAggCreditLimit;
        /// <summary>
        /// אשראי מצרפי
        /// </summary>
        [Required]
        public int? FrAggCreditLimit
        {
            get => this.frAggCreditLimit;
            set
            {
                base.SetProperty(value);
                this.frAggCreditLimit = value;
            }
        }

        private int? frAggCreditLimitPercent;
        /// <summary>
        /// אשראי מצרפי כאחוז משווי תיק
        /// </summary>
        [Required]
        public int? FrAggCreditLimitPercent
        {
            get => this.frAggCreditLimitPercent;
            set
            {
                base.SetProperty(value);
                this.frAggCreditLimitPercent = value;
            }
        }

        //private string dailyBuyStock;
        ///// <summary>
        ///// יומית - תוספת לקניה בני"ע
        ///// </summary>
        //public string DailyBuyStock
        //{
        //    get => this.dailyBuyStock;
        //    set
        //    {
        //        base.SetProperty(value);
        //        this.dailyBuyStock = value;
        //    }
        //}

        //private string dailySellShortStock;
        ///// <summary>
        ///// יומית - תוספת לשורט / מכירה בני"ע
        ///// </summary>
        //public string DailySellShortStock
        //{
        //    get => this.dailySellShortStock;
        //    set
        //    {
        //        base.SetProperty(value);
        //        this.dailySellShortStock = value;
        //    }
        //}

        //private string dailyBuyOption;
        ///// <summary>
        ///// יומית - תוספת לקניה בנגזרים
        ///// </summary>
        //public string DailyBuyOption
        //{
        //    get => this.dailyBuyOption;
        //    set
        //    {
        //        base.SetProperty(value);
        //        this.dailyBuyOption = value;
        //    }
        //}

        //private string dailySellShortOption;
        ///// <summary>
        ///// יומית - תוספת לשורט / מכירה בנגזרים
        ///// </summary>
        //public string DailySellShortOption
        //{
        //    get => this.dailySellShortOption;
        //    set
        //    {
        //        base.SetProperty(value);
        //        this.dailySellShortOption = value;
        //    }
        //}
    }
}

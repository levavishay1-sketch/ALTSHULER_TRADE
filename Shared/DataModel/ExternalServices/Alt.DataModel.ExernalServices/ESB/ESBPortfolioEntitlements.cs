using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBPortfolioEntitlements : ExternalEntityBase
    {
        private string allowedIsraelShort;
        /// <summary>
        /// הרשאת שורט ני"ע
        /// אישור בקשת מכירת ני"ע בחסר
        /// </summary>
        [Required]
        public string AllowedIsraelShort
        {
            get => this.allowedIsraelShort;
            set
            {
                base.SetProperty(value);
                this.allowedIsraelShort = value;
            }
        }

        private string allowedForeignShort;
        /// <summary>
        /// אם שדה "אישור בקשת מכירת ני"ע בחסר" שווה ל"כן" אז Y. אחרת N
        /// </summary>
        [Required]
        public string AllowedForeignShort
        {
            get => this.allowedForeignShort;
            set
            {
                base.SetProperty(value);
                this.allowedForeignShort = value;
            }
        }

        private string allowedForeignBuy;
        /// <summary>
        /// הרשאת קניית ניע"ז
        /// </summary>
        [Required]
        public string AllowedForeignBuy
        {
            get => this.allowedForeignBuy;
            set
            {
                base.SetProperty(value);
                this.allowedForeignBuy = value;
            }
        }

        private string allowedForeignSell;
        /// <summary>
        /// הרשאת מכירה בניע"ז
        /// </summary>
        [Required]
        public string AllowedForeignSell
        {
            get => this.allowedForeignSell;
            set
            {
                base.SetProperty(value);
                this.allowedForeignSell = value;
            }
        }

        private string allowedIsraeliBuy;
        /// <summary>
        /// הרשאת קניה ני"ע ישראלי
        /// </summary>
        [Required]
        public string AllowedIsraeliBuy
        {
            get => this.allowedIsraeliBuy;
            set
            {
                base.SetProperty(value);
                this.allowedIsraeliBuy = value;
            }
        }

        private string allowedIsraeliSell;
        /// <summary>
        /// הרשאת מכירה ני"ע ישראלי
        /// </summary>
        [Required]
        public string AllowedIsraeliSell
        {
            get => this.allowedIsraeliSell;
            set
            {
                base.SetProperty(value);
                this.allowedIsraeliSell = value;
            }
        }

        private string marketMakerIsraelETF;
        /// <summary>
        /// עושה שוק ב- ETF
        /// </summary>
        [Required]
        public string MarketMakerIsraelETF
        {
            get => this.marketMakerIsraelETF;
            set
            {
                base.SetProperty(value);
                this.marketMakerIsraelETF = value;
            }
        }

        //private string marketMakerForeignETF;
        ///// <summary>
        ///// עושה שוק ב- ETF
        ///// </summary>
        //[Required]
        //public string MarketMakerForeignETF
        //{
        //    get => this.marketMakerForeignETF;
        //    set
        //    {
        //        base.SetProperty(value);
        //        this.marketMakerForeignETF = value;
        //    }
        //}

        private string marketMakerIsraelBond;
        /// <summary>
        /// עושה שוק באג"ח
        /// </summary>
        [Required]
        public string MarketMakerIsraelBond
        {
            get => this.marketMakerIsraelBond;
            set
            {
                base.SetProperty(value);
                this.marketMakerIsraelBond = value;
            }
        }

        //private string marketMakerForeignBond;
        ///// <summary>
        ///// עושה שוק באג"ח
        ///// </summary>
        //[Required]
        //public string MarketMakerForeignBond
        //{
        //    get => this.marketMakerForeignBond;
        //    set
        //    {
        //        base.SetProperty(value);
        //        this.marketMakerForeignBond = value;
        //    }
        //}

        private string allowedIsraeliFuture;
        /// <summary>
        /// הרשאת מסחר בפיוצ'ר מקומי
        /// </summary>
        [Required]
        public string AllowedIsraeliFuture
        {
            get => this.allowedIsraeliFuture;
            set
            {
                base.SetProperty(value);
                this.allowedIsraeliFuture = value;
            }
        }

        //private string allowedForeignOptions;
        ///// <summary>
        ///// הרשאת מסחר בניע"ז נגזרים
        ///// </summary>
        //[Required]
        //public string AllowedForeignOptions
        //{
        //    get => this.allowedForeignOptions;
        //    set
        //    {
        //        base.SetProperty(value);
        //        this.allowedForeignOptions = value;
        //    }
        //}

        private string allowedIsraelOptionsTrading;
        public string AllowedIsraelOptionsTrading
        {
            get => this.allowedIsraelOptionsTrading;
            set
            {
                base.SetProperty(value);
                this.allowedIsraelOptionsTrading = value;
            }
        }

        private string allowedForeignOptionsTrading;
        public string AllowedForeignOptionsTrading
        {
            get => this.allowedForeignOptionsTrading;
            set
            {
                base.SetProperty(value);
                this.allowedForeignOptionsTrading = value;
            }
        }

        private string allowedWriteIsraelOptions;
        public string AllowedWriteIsraelOptions
        {
            get => this.allowedWriteIsraelOptions;
            set
            {
                base.SetProperty(value);
                this.allowedWriteIsraelOptions = value;
            }
        }

        private string allowedWriteForeignOptions;
        public string AllowedWriteForeignOptions
        {
            get => this.allowedWriteForeignOptions;
            set
            {
                base.SetProperty(value);
                this.allowedWriteForeignOptions = value;
            }
        }

        private string allowedIsraelCredit;
        public string AllowedIsraelCredit
        {
            get => this.allowedIsraelCredit;
            set
            {
                base.SetProperty(value);
                this.allowedIsraelCredit = value;
            }
        }

        //private string allowedForeignCredit;
        //public string AllowedForeignCredit
        //{
        //    get => this.allowedForeignCredit;
        //    set
        //    {
        //        base.SetProperty(value);
        //        this.allowedForeignCredit = value;
        //    }
        //}

        private string allowedForeignFuture;
        public string AllowedForeignFuture
        {
            get => this.allowedForeignFuture;
            set
            {
                base.SetProperty(value);
                this.allowedForeignFuture = value;
            }
        }

        private string allowedIsraelWeeklyOptions;
        public string AllowedIsraelWeeklyOptions
        {
            get => this.allowedIsraelWeeklyOptions;
            set
            {
                base.SetProperty(value);
                this.allowedIsraelWeeklyOptions = value;
            }
        }

        //private string allowedForeignWeeklyOptions;
        //public string AllowedForeignWeeklyOptions
        //{
        //    get => this.allowedForeignWeeklyOptions;
        //    set
        //    {
        //        base.SetProperty(value);
        //        this.allowedForeignWeeklyOptions = value;
        //    }
        //}

    }
}

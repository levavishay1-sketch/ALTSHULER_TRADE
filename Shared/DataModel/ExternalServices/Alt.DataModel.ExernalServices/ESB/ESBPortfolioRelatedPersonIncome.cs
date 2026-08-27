using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBPortfolioRelatedPersonIncome : ExternalEntityBase
    {
        private string employmentStatus;
        /// <summary>
        /// סטטוס תעסוקתי אחר
        /// </summary>
        public string EmploymentStatus
        {
            get => this.employmentStatus;
            set
            {
                base.SetProperty(value);
                this.employmentStatus = value;
            }
        }

        private string otherEmploymentStatus;
        /// <summary>
        /// תחום עיסוק
        /// </summary>
        public string OtherEmploymentStatus
        {
            get => this.otherEmploymentStatus;
            set
            {
                base.SetProperty(value);
                this.otherEmploymentStatus = value;
            }
        }

        private string employmentPosition;
        /// <summary>
        /// תפקיד
        /// </summary>
        public string EmploymentPosition
        {
            get => this.employmentPosition;
            set
            {
                base.SetProperty(value);
                this.employmentPosition = value;
            }
        }

        private string employmentJobName;
        /// <summary>
        /// שם מקום עבודה
        /// </summary>
        public string EmploymentJobName
        {
            get => this.employmentJobName;
            set
            {
                base.SetProperty(value);
                this.employmentJobName = value;
            }
        }

        private string employmentCompanyName;
        /// <summary>
        /// שם עסק
        /// </summary>
        public string EmploymentCompanyName
        {
            get => this.employmentCompanyName;
            set
            {
                base.SetProperty(value);
                this.employmentCompanyName = value;
            }
        }
   
        private string monthlyIncomeRange;
        /// <summary>
        /// רמת הכנסה חודשית
        /// </summary>
        public string MonthlyIncomeRange
        {
            get => this.monthlyIncomeRange;
            set
            {
                base.SetProperty(value);
                this.monthlyIncomeRange = value;
            }
        }

        private string isOpenAccountRefusal;
        /// <summary>
        /// הצהרת סירוב פתיחת חשבון בבנק / חבר בורסה
        /// </summary>
        [Required]
        public string IsOpenAccountRefusal
        {
            get => this.isOpenAccountRefusal;
            set
            {
                base.SetProperty(value);
                this.isOpenAccountRefusal = value;
            }
        }

        private string isMarketingDataApproval;
        /// <summary>
        /// אישור קבלת תוכן שיווקי
        /// </summary>
        [Required]
        public string IsMarketingDataApproval
        {
            get => this.isMarketingDataApproval;
            set
            {
                base.SetProperty(value);
                this.isMarketingDataApproval = value;
            }
        }

        private int? employmentClassification;
        /// <summary>
        /// תחום עיסוק
        /// </summary>
        public int? EmploymentClassification
        {
            get => this.employmentClassification;
            set
            {
                base.SetProperty(value);
                this.employmentClassification = value;
            }
        }

    }
}

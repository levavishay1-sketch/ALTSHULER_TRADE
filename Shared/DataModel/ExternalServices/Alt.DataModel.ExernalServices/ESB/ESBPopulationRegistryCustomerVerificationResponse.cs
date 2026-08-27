using System;
using System.ComponentModel;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBPopulationRegistryCustomerVerificationResponse
    {
        /// <summary>
        /// קוד אימות
        ///0 – לא עבר
        ///1 – עבר
        /// </summary>
        public int? KodImut { get; set; }

        /// <summary>
        /// מספר זהות
        /// </summary>
        [Description("")]
        public int? IdNumber { get; set; }

        /// <summary>
        /// אינדיקציה האם תאריך הנפקת  ת.ז אשר הועבר בקלט זהה למרשם
        /// </summary>
        public int? IndTaaricHanpakaTzMatchDb { get; set; }

        /// <summary>
        /// שם משפחה
        /// </summary>
        [Description("שם משפחה")]
        public string LastName { get; set; }

        /// <summary>
        /// שם פרטי
        /// </summary>
        [Description("שם פרטי")]
        public string FirstName { get; set; }

        /// <summary>
        /// קוד מין
        /// 1- זכר	
        /// 2- נקבה
        /// </summary>
        [Description("מין")]
        public int? Sex { get; set; }

        /// <summary>
        /// תיאור מין - זכר\נקבה
        /// </summary>
        [Description("תיאור מין")]
        public string SexDesc { get; set; }

        /// <summary>
        /// תאריך לידה
        /// </summary>
        [Description("תאריך לידה")]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// אינדיקציית פטירה
        /// </summary>
        public int? IndDead { get; set; }

        /// <summary>
        /// סמל ישוב
        /// </summary>
        public int? CityCode { get; set; }

        /// <summary>
        /// שם ישוב
        /// </summary>
        public string CityName { get; set; }

        /// <summary>
        /// סמל רחוב
        /// </summary>
        public int? StreetCode { get; set; }

        /// <summary>
        /// שם רחוב
        /// </summary>
        public string StreetName { get; set; }

        /// <summary>
        /// מספר בית
        /// </summary>
        public int? HouseNumber { get; set; }

        /// <summary>
        /// אות בית
        /// </summary>
        public string HouseLetter { get; set; }

        /// <summary>
        /// אות בית
        /// </summary>
        public int? EntranceNumber { get; set; }

        /// <summary>
        /// מספר דירה
        /// </summary>
        public int? ApartmentNumber { get; set; }

        /// <summary>
        /// מיקוד 5
        /// </summary>
        public int? Zipcode5 { get; set; }

        /// <summary>
        /// מיקוד 7
        /// </summary>
        public int? Zipcode7 { get; set; }

        public object this[string propertyName]
        {
            get { return typeof(ESBPopulationRegistryCustomerVerificationResponse).GetProperty(propertyName)?.GetValue(this); }
        }
    }
}

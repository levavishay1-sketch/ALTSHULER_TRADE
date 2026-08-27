
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
using System.Collections.Generic;

namespace Alt.External.Services.CrmApi.Models
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Controllers
    {

        private List<ControllersController> controllerField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Controller")]
        public List<ControllersController> Controller
        {
            get
            {
                return this.controllerField;
            }
            set
            {
                this.controllerField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ControllersController
    {

        private List<ControllersControllerRoute> routeField;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Route")]
        public List<ControllersControllerRoute> Route
        {
            get
            {
                return this.routeField;
            }
            set
            {
                this.routeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ControllersControllerRoute
    {

        private List<ControllersControllerRouteAction> actionField;

        private string pathField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Action")]
        public List<ControllersControllerRouteAction> Action
        {
            get
            {
                return this.actionField;
            }
            set
            {
                this.actionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Path
        {
            get
            {
                return this.pathField;
            }
            set
            {
                this.pathField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ControllersControllerRouteAction
    {

        private List<ControllersControllerRouteActionSourceSystemValidation> sourceSystemValidation;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SourceSystemValidation")]
        public List<ControllersControllerRouteActionSourceSystemValidation> SourceSystemValidation
        {
            get
            {
                return this.sourceSystemValidation;
            }
            set
            {
                this.sourceSystemValidation = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ControllersControllerRouteActionSourceSystemValidation
    {

        private List<ControllersControllerRouteActionProperty> propertyField;

        private string propertyToCheck;
        private string checkValue;
        private bool defaultSystem = false;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Property")]
        public List<ControllersControllerRouteActionProperty> Property
        {
            get
            {
                return this.propertyField;
            }
            set
            {
                this.propertyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PropertyToCheck
        {
            get
            {
                return this.propertyToCheck;
            }
            set
            {
                this.propertyToCheck = value;
            }
        }
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Default
        {
            get
            {
                return this.defaultSystem;
            }
            set
            {
                this.defaultSystem = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CheckValue
        {
            get
            {
                return this.checkValue;
            }
            set
            {
                this.checkValue = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ControllersControllerRouteActionProperty
    {

        private byte? requiredField;

        private string defaultValueField;

        private bool defaultValueFieldSpecified;

        private List<ControllersControllerRouteActionProperty> innerPropertyField;

        private string nameField;

        private int? maxLength;


        public int? MaxLength
        {
            get
            {
                return this.maxLength;
            }
            set
            {
                this.maxLength = value;
            }
        }

        /// <remarks/>
        public byte? Required
        {
            get
            {
                return this.requiredField;
            }
            set
            {
                this.requiredField = value;
            }
        }

        /// <remarks/>
        public string DefaultValue
        {
            get
            {
                return this.defaultValueField;
            }
            set
            {
                this.defaultValueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DefaultValueSpecified
        {
            get
            {
                return this.defaultValueFieldSpecified;
            }
            set
            {
                this.defaultValueFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("InnerProperty")]
        public List<ControllersControllerRouteActionProperty> InnerProperty
        {
            get
            {
                return this.innerPropertyField;
            }
            set
            {
                this.innerPropertyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ControllersControllerRouteActionPropertyInnerProperty
    {

        private byte requiredField;

        private string nameField;

        /// <remarks/>
        public byte Required
        {
            get
            {
                return this.requiredField;
            }
            set
            {
                this.requiredField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }
}


using Alt.DataModel.Crm.External.Contracts;
using Alt.External.Services.CrmApi.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Results;

namespace Alt.Test.CrmApi
{
    [TestClass]
    public class LeadApiUnitTest : BaseUnitTest
    {
        string controllerName = "Leads";
        string routePath = "api/leads";

        [TestMethod]
        [TestCategory("Integration")]
        public void CreateLead_Success()
        {
            ApiLead leadToCreate = new ApiLead()
            {
                MobilePhone = "050-1231231",
                CreationMethodCode = 2,
                LeadSourceCode = 4
            };

            var leadController = new LeadsController();
            HandleControllerSetup(leadController, HttpMethod.Post);

            var result = leadController.Post(leadToCreate);
            var returnedValue = ((ObjectContent)((ResponseMessageResult)result).Response.Content).Value;

            Assert.AreEqual(((ResponseMessageResult)result).Response.StatusCode, HttpStatusCode.Created);
            Assert.IsInstanceOfType(returnedValue, typeof(Guid));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void PostLead_ValidData()
        {
            ApiLead leadToValidate = new ApiLead()
            {
                MobilePhone = "050-1231231",
                LeadSourceCode = 4
            };

            var result = ValidateModel(leadToValidate, new LeadsController(), HttpMethod.Post, controllerName, "Post", routePath);

            Assert.IsTrue(result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void PostLead_InValidData_LeadSourceCodeMissing()
        {
            ApiLead leadToValidate = new ApiLead()
            {
                MobilePhone = "050-1231231",
            };
            var result = ValidateModel(leadToValidate, new LeadsController(), HttpMethod.Post, controllerName, "Post", routePath);
            Assert.IsFalse(result);
        }
    }
}

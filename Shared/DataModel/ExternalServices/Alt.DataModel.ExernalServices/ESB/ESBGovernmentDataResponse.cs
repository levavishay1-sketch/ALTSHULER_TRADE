using Alt.DataModel.Crm.External.Contracts;
using System;
using System.Collections.Generic;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBGovernmentDataResponse<T> where T : ApiEntity
    {
        public List<T> Data { get; set; }
    }
}

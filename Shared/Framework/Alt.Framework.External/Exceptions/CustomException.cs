using Alt.DataModel.Crm.Core.Errors;
using System;

namespace Alt.Framework.External.Exceptions
{
   public class CustomException: Exception
    {
        public CustomException() { }

        public CustomException(string message): base(message) { }

        public CustomException(string message, Exception innerException) : base(message, innerException) { }

        public CustomException(Error error): base(error.Message)
        {
            HResult = error.Code;
        }
        public CustomException(string message, int errorCode): base(message)
        {
            HResult = errorCode;
        }
    }
}

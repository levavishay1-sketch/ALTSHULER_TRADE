using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Framework.EntryPoints.External
{
    public class ThirdPartyTracingService : ITracingService
    {
        public void Trace(string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
        }
    }
}

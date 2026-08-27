using Microsoft.Xrm.Sdk;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Alt.External.WebJobs.RelayListener
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(
    InstanceContextMode = InstanceContextMode.PerCall,  // New instance per request
    ConcurrencyMode = ConcurrencyMode.Multiple,        // Allow multiple threads in parallel
    UseSynchronizationContext = false)]
    public class RelayServiceEndpoint : IWebHttpServiceEndpointPlugin
    {
        public RelayServiceEndpoint() { }

        public string Execute(RemoteExecutionContext executionContext)
        {
            return Program.Execute(executionContext);
        }
        
    }
}

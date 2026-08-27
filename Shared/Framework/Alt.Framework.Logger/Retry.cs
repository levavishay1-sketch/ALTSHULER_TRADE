using Microsoft.Xrm.Sdk;
using System;
using System.ServiceModel;
using System.Threading;

namespace Alt.Framework.Logger
{
    public class Retry
    {
        private const int RateLimitExceededErrorCode = -2147015902;
        private const int TimeLimitExceededErrorCode = -2147015903;
        private const int ConcurrencyLimitExceededErrorCode = -2147015898;

        public Retry()
        {
        }

        public OrganizationResponse Do(OrganizationRequest request, Func<OrganizationRequest, OrganizationResponse> func, int maxRetries = 6)
        {
            int retryCount = 0;
            OrganizationResponse reuslt = null;
            Guid? retryRquestIdentifier = Guid.NewGuid();
            while (true)
            {
                try
                {
                    reuslt = func(request);
                    return reuslt;
                }
                catch (FaultException<OrganizationServiceFault> ex)
                    when (IsTransientError(ex))
                {
                    TimeSpan delay = new TimeSpan();
                    if (++retryCount >= maxRetries)
                    {
                        throw;
                    }

                    if (ex.Detail.ErrorCode == RateLimitExceededErrorCode && ex.Detail.ErrorDetails.ContainsKey("Retry-After"))
                    {
                        // Use Retry-After delay when specified
                        delay = (TimeSpan)ex.Detail.ErrorDetails["Retry-After"];
                    }
                    else
                    {
                        // else use exponential backoff delay
                        delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                    }

                    Thread.Sleep(delay);
                }
                catch (TimeoutException ex)
                {
                    if (++retryCount >= maxRetries)
                    {
                        throw ex;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
                }
                catch (CommunicationException ex) when (!(ex is FaultException<OrganizationServiceFault>))
                {
                    if (++retryCount >= maxRetries)
                    {
                        throw ex;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
                }
            }
        }

        private static bool IsTransientError(FaultException<OrganizationServiceFault> ex)
        {
            // You can add more transient fault codes to retry here
            if (ex.Detail.ErrorCode == RateLimitExceededErrorCode ||
                ex.Detail.ErrorCode == TimeLimitExceededErrorCode ||
                ex.Detail.ErrorCode == ConcurrencyLimitExceededErrorCode)
            {
                return true;
            }

            return false;
        }
    }
}

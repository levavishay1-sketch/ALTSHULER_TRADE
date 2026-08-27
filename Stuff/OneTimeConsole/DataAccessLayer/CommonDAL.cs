using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneTimeConsole.DataAccessLayer
{
    public class CommonDAL : CrmBaseDAL<Entity>
    {
        public CommonDAL(GlobalContext globalContext, string entityName = null) : base(globalContext, entityName) { }

        public void UpdateRetryParallel<T>(List<T> entitiesToUpdate) where T : Entity
        {
            GlobalContext.LogEntry();

            const int maxRetries = 3;
            ConcurrentBag<string> failedUpdatesBag = new ConcurrentBag<string>();

            entitiesToUpdate.AsParallel().ForAll((entity) =>
            {
                int retries = 0;
                bool isSuccess = false;
                while (retries < maxRetries && !isSuccess)
                {
                    try
                    {
                        Update(entity);
                        isSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        string error;
                        error = $"\nFailedEntityToUpdateId:{entity.Id}\n{ex}";
                        failedUpdatesBag.Add(error);
                    }
                    finally
                    {
                        retries++;
                    }
                }
            });

            if (failedUpdatesBag.Count > 0)
            {
                string entitiesFailedToUpdateDetails = string.Join("\n", failedUpdatesBag);
                GlobalContext.Log.Info($"\n{failedUpdatesBag.Count} EntitiesFailedToUpdateDetails:\n{entitiesFailedToUpdateDetails}");
            }
            else
            {
                GlobalContext.Log.Info("Update Success");
            }
        }

        public List<ExecuteMultipleResponse> ExecuteMultipleRequestsInChunks<TEntity>(List<TEntity> entityList, RequestType crmRequestType, int chunksAmount = 10) where TEntity : Entity
        {
            var listOfchunkedList = entityList.ToChunks(chunksAmount);
            ConcurrentBag<ExecuteMultipleResponse> responses = new ConcurrentBag<ExecuteMultipleResponse>();
            Parallel.ForEach(listOfchunkedList, (chunkedList) =>
            {
                OrganizationRequestCollection requestsCollection = new OrganizationRequestCollection();

                foreach (var entity in chunkedList)
                {
                    string requestName = GetRequestNameByType(crmRequestType);

                    OrganizationRequest request = new OrganizationRequest(requestName);
                    if (crmRequestType == RequestType.Delete)
                    {
                        request["Target"] = entity.ToEntityReference();
                    }
                    else
                    {
                        request["Target"] = entity;
                    }

                    requestsCollection.Add(request);
                }

                responses.Add(this.ExecuteMultipleRequests(requestsCollection));
            });

            return responses.ToList();
        }

        private string GetRequestNameByType(RequestType crmRequestType)
        {
            if (!Enum.IsDefined(typeof(RequestType), crmRequestType))
            {
                throw new Exception(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.InvalidRequest));
            }
            return crmRequestType.ToString();
        }
    }
}
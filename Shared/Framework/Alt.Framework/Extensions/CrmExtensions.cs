using Alt.DataModel.Crm.Entities;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Framework.Extensions
{
    public static class CrmExtensions
    {
        public static EntityCollection ToEntityCollection<T>(this IEnumerable<T> source) where T : Entity
        {
            EntityCollection collection = new EntityCollection();
            foreach (var item in source)
            {
                collection.Entities.Add(item);
            }
            return collection.Entities.Count > 0 ? collection : null;
        }

        public static IEnumerable<ActivityParty> ToActivityPartyCollection(this IEnumerable<EntityReference> list)
        {
            List<ActivityParty> collection = null;
            if (list != null)
            {
                collection = new List<ActivityParty>();
                foreach (var entityReference in list)
                {
                    collection.Add(new ActivityParty() { PartyId = entityReference });
                }
            }
            return collection != null && collection.Count > 0 ? collection : null;
        }


        public static IEnumerable<ActivityParty> ToActivityPartyCollection(this IEnumerable<Entity> entities)
        {
            List<ActivityParty> collection = new List<ActivityParty>();
            foreach (var toEntity in entities)
            {
                ActivityParty activityParty = new ActivityParty();
                if (toEntity.LogicalName == null && toEntity["addressused"] != null)
                {
                    activityParty.AddressUsed = toEntity["addressused"] as string;
                }
                else
                {
                    if (toEntity.Id == Guid.Empty)
                    {
                        var entityKey = toEntity.KeyAttributes?.FirstOrDefault();
                        if (!entityKey.Equals(default(KeyValuePair<string, object>)))
                        {
                            activityParty.PartyId = new EntityReference(toEntity.LogicalName);
                            activityParty.PartyId.KeyAttributes = new KeyAttributeCollection() { entityKey.Value };
                        }
                        else
                        {
                            throw new Exception("invalid entity for actitvy party mapping");
                        }
                    }
                    else { activityParty.PartyId = new EntityReference(toEntity.LogicalName, toEntity.Id); }

                }
                collection.Add(activityParty);
            }
            return collection.Count > 0 ? collection : null;
        }


        private static T GetFirstByLogicalName<T>(this EntityCollection collection, string logicalName) where T : Entity
        {
            return collection?.Entities?.Select(x => x.ToEntity<T>()).Where(x => x.LogicalName == logicalName).FirstOrDefault<T>();
        }

        public static List<EntityReference> ConvertActivityPartyToEntityReference(this EntityCollection collection)
        {
            List<EntityReference> convertedList = null;
            if (collection != null)
            {
                convertedList = new List<EntityReference>();
                foreach (var party in collection.Entities)
                {
                    if (party.LogicalName == ActivityParty.EntityLogicalName)
                    {
                        convertedList.Add(party.GetAttributeValue<EntityReference>("partyid"));
                    }
                }
            }
            return convertedList;
        }

        public static EntityCollection ConvertEntityReferenceToActivityPartyEntityCollection(this IEnumerable<EntityReference> collection)
        {
            EntityCollection entityCollection = null;
            if (collection != null)
            {
                entityCollection = new EntityCollection();
                foreach (var entityReference in collection)
                {
                    Entity activityParty = new Entity("activityparty");
                    activityParty.Attributes["partyid"] = entityReference;
                    entityCollection.Entities.Add(activityParty);
                }
            }
            return entityCollection;
        }

        public static EntityCollection ConvertEntityIEnumerableToActivityPartyEntityCollection(this IEnumerable<Entity> collection)
        {
            EntityCollection entityCollection = null;
            if (collection != null)
            {
                entityCollection = new EntityCollection();
                foreach (var entityRecord in collection)
                {
                    Entity activityParty = new Entity("activityparty");
                    activityParty.Attributes["partyid"] = entityRecord.ToEntityReference();
                    entityCollection.Entities.Add(activityParty);
                }
            }
            return entityCollection;
        }

        public static EntityCollection ConvertEntityReferenceToActivityPartyEntityCollection(this EntityReference entityReference)
        {
            EntityCollection entityCollection = null;
            if (entityReference != null)
            {
                entityCollection = new EntityCollection();
                Entity activityParty = new Entity("activityparty");
                activityParty.Attributes["partyid"] = entityReference;
                entityCollection.Entities.Add(activityParty);
            }
            return entityCollection;
        }

        public static EntityCollection FilterOnlyActivityPartyRecords(this EntityCollection entityCollection)
        {
            EntityCollection validToCollection = new EntityCollection();
            foreach (var party in entityCollection.Entities)
            {
                if (party.LogicalName == ActivityParty.EntityLogicalName)
                {
                    var activityParty = new ActivityParty();
                    var partyId = party.GetAttributeValue<EntityReference>("partyid");
                    var addressUsed = party.GetAttributeValue<string>("addressused");

                    if (partyId != null)
                    {
                        activityParty.PartyId = partyId;
                        validToCollection.Entities.Add(activityParty);
                    }
                    else if (addressUsed != null)
                    {
                        activityParty.AddressUsed = addressUsed;
                        validToCollection.Entities.Add(activityParty);
                    }
                }
            }
            return validToCollection;
        }

        public static EntityReference GetFirstOrDefaultEntityRefrenceFromEntityCollection(this EntityCollection to, string targetEntityReferenceLogicalName = null)
        {
            EntityReference firstOrDefaultEntityRefrence = null;
            if (to != null)
            {
                foreach (var party in to.Entities)
                {
                    if (party.LogicalName == ActivityParty.EntityLogicalName)
                    {
                        var activityParty = party.GetAttributeValue<EntityReference>("partyid");
                        if (targetEntityReferenceLogicalName != null)
                        {
                            if (activityParty.LogicalName == targetEntityReferenceLogicalName)
                            {
                                firstOrDefaultEntityRefrence = activityParty;
                                break;
                            }
                        }
                        else
                        {
                            firstOrDefaultEntityRefrence = activityParty;
                            break;
                        }
                    }
                }
            }
            return firstOrDefaultEntityRefrence;
        }
    }
}

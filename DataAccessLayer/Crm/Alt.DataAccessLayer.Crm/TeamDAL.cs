using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.Crm
{
    public class TeamDAL : CrmBaseDAL<Team>
    {
        public TeamDAL(GlobalContext globalContext) : base(globalContext, Team.EntityLogicalName)
        {
        }

        public List<Team> GetAllTeamsByUserId(Guid systemUserId)
        {
            this.GlobalContext.LogEntry();

            QueryExpression teamsQuery = new QueryExpression(Team.EntityLogicalName);
            teamsQuery.ColumnSet = new ColumnSet(Team.Fields.TeamId);

            ConditionExpression systemUserContdition = new ConditionExpression(SystemUser.Fields.SystemUserId, ConditionOperator.Equal, systemUserId);
            LinkEntity link = teamsQuery.AddLink(TeamMembership.EntityLogicalName, Team.Fields.TeamId, TeamMembership.Fields.TeamId);
            link.LinkCriteria.AddCondition(systemUserContdition);

            return base.GetMultiple(teamsQuery);
        }

        public bool IsTeamMember(Guid teamId, Guid systemUserId)
        {
            QueryExpression query = new QueryExpression(Team.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(Team.Fields.TeamId)
            };
            query.Criteria.AddCondition(new ConditionExpression(Team.Fields.TeamId, ConditionOperator.Equal, teamId));
            LinkEntity link = query.AddLink(TeamMembership.EntityLogicalName, Team.Fields.TeamId, TeamMembership.Fields.TeamId);
            link.LinkCriteria.AddCondition(new ConditionExpression(SystemUser.Fields.SystemUserId, ConditionOperator.Equal, systemUserId));
            var results = base.GetMultiple(query);

            return results.Count > 0;
        }

        public Team GetTeamByCodeWithCache(int? code = null, string cacheKey = null, int cacheInMinutes = 60 )
        {
            GlobalContext.LogEntry();

            string defaultTeamParameterName = "DefaultOwnerTeamCode";
            if (!code.HasValue)
            {
                code = GlobalContext.CacheManager.GetGlobalParameter<int?>(defaultTeamParameterName);
            }

            Team retrievedTeam = GlobalContext.CacheManager.GetCachedItem(cacheKey ?? defaultTeamParameterName,
                () => GetFirstOrDefaultByAttribute(Team.Fields.alt_TeamCodeInt, code, new string[] { Team.Fields.TeamId }), cacheInMinutes);
            return retrievedTeam;
        }
    }
}

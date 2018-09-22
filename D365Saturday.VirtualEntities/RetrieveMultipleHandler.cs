using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Extensions;

namespace D365Saturday.VirtualEntities
{
    public class RetrieveMultipleHandler: RetrieveBase
    {
        protected override void ProcessRetrieveOperation()
        {
            var crmQuery = Context.PluginExecutionContext.InputParameterOrDefault<QueryExpression>("Query");
            var zendeskQuery = $"type:{Context.Mapping.NameMap.ExternalName}";

            if (crmQuery.Criteria != null && crmQuery.Criteria.Conditions.Count != 0)
            {
                foreach (var criteria in crmQuery.Criteria.Conditions)
                {
                    if (criteria.Operator != ConditionOperator.Equal)
                    {
                        throw new InvalidPluginExecutionException($"{criteria.Operator} is not supported");
                    }

                    var sourceFieldName = Context.Mapping.MapAttributeNameExternal(criteria.AttributeName);
                    if (sourceFieldName == "type")
                    {
                        sourceFieldName = "ticket_type";
                    }

                    zendeskQuery += " " + $"{sourceFieldName}:\"{criteria.Values.First()}\"";
                }
            }

            zendeskQuery = "search.json?query=" + Uri.EscapeDataString(zendeskQuery);

            var tickets = Context.ZendeskClient.ExecuteRequest(zendeskQuery);

            var crmTickets = tickets.Value<JArray>("results").Select(t => ConvertToEntity((JObject)t));

            var result = new EntityCollection()
            {
                EntityName = Context.PluginExecutionContext.PrimaryEntityName,
                MoreRecords = false,
                PagingCookie =  string.Empty,
                TotalRecordCount = crmTickets.Count()
            };
            result.Entities.AddRange(crmTickets);

            Context.PluginExecutionContext.OutputParameters["BusinessEntityCollection"] = result;
        }
    }
}

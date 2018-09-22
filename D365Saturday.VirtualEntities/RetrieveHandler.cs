using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Newtonsoft.Json.Linq;

namespace D365Saturday.VirtualEntities
{
    public class RetrieveHandler: RetrieveBase
    {
        protected override void ProcessRetrieveOperation()
        {
            var target = Context.PluginExecutionContext.InputParameterOrDefault<EntityReference>("Target");

            var ticketId = target.Id.ToLong();

            var ticket = Context.ZendeskClient.ExecuteRequest($"tickets/{ticketId}.json").Value<JObject>("ticket");

            Context.PluginExecutionContext.OutputParameters["BusinessEntity"] = ConvertToEntity(ticket);
        }
    }
}

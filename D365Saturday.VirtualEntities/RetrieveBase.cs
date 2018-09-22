using System;
using Newtonsoft.Json.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Data.Mappings;

namespace D365Saturday.VirtualEntities
{
    public abstract class RetrieveBase: IPlugin
    {
        public VirtualEntityPluginContext Context;

        public void Execute(IServiceProvider serviceProvider)
        {
            Context = new VirtualEntityPluginContext(serviceProvider);
            ProcessRetrieveOperation();
        }

        protected abstract void ProcessRetrieveOperation();

        protected Entity ConvertToEntity(JObject ticket)
        {
            var result = new Entity(Context.PluginExecutionContext.PrimaryEntityName);

            foreach (var attributeMap in Context.Mapping.AttributeMap)
            {
                if (attributeMap.IsPrimaryAttributeId)
                {
                    var ticketid = ticket.Value<long>(attributeMap.NameMap.ExternalName);
                    result.Id = ticketid.ToGuid();
                    result[$"{Context.PluginExecutionContext.PrimaryEntityName}id"] = ticketid.ToGuid();
                }
                else
                {
                    if (!ticket.ContainsKey(attributeMap.NameMap.ExternalName))
                        continue;

                    result[attributeMap.NameMap.XrmName] = ticket.Value<string>(attributeMap.NameMap.ExternalName);
                }
            }

            return result;
        }
    }

    public class VirtualEntityPluginContext
    {
        public readonly IPluginExecutionContext PluginExecutionContext;
        public readonly ZendeskClient ZendeskClient;
        public readonly EntityMap Mapping;

        public VirtualEntityPluginContext(IServiceProvider serviceProvider)
        {
            PluginExecutionContext = serviceProvider.Get<IPluginExecutionContext>();
            var serviceFactory = serviceProvider.Get<IOrganizationServiceFactory>();
            var service = serviceFactory.CreateOrganizationService(null);

            var retriever = serviceProvider.Get<IEntityDataSourceRetrieverService>();
            var dataSource = retriever.RetrieveEntityDataSource();
            var instanceName = dataSource.GetAttributeValue<string>("d365_instancename");
            var userName = dataSource.GetAttributeValue<string>("d365_username");
            var token = dataSource.GetAttributeValue<string>("d365_token");

            ZendeskClient = new ZendeskClient(instanceName, userName, token);

            var metadata = service.GetEntityMetadata(PluginExecutionContext.PrimaryEntityName);
            Mapping = EntityMapFactory.Create(metadata, new DefaultTypeMapFactory(), null);

            //var externalName = Mapping.MapAttributeNameExternal("crm_field_name");
        }
    }
}

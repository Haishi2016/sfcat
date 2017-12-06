using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using ServiceBrokerActor.Interfaces;
using OSB;
using sfcat;
using sfcat.OSBCommands;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace ServiceBrokerActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class ServiceBrokerActor : Actor, IServiceBrokerActor
    {
        public ServiceBrokerActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            return this.StateManager.TryAddStateAsync("count", 0);
        }

        private async Task<string> resolveCatalogServiceAddress()
        {
            Uri serviceName = new Uri("fabric:/SFServiceCatalog/CatalogService");
            ServicePartitionResolver resolve = new ServicePartitionResolver(() => new System.Fabric.FabricClient());
            var partition = await resolve.ResolveAsync(serviceName, ServicePartitionKey.Singleton, default(CancellationToken));            
            string address = partition.GetEndpoint().Address.Substring(18, partition.GetEndpoint().Address.Length - 21);
            address = address.Replace("\\", "");
            return address;
        }
        public async Task<bool> Connect(string serviceId, string planId, string instanceId, string parameters, CancellationToken cancellationToken)
        {
            var state = await this.StateManager.TryGetStateAsync<MyState>(instanceId, cancellationToken);
            if (state.HasValue)
            {
                return true;
            }
            else
            {
                string address = await resolveCatalogServiceAddress();
                CreateServiceInstanceCommand command = new CreateServiceInstanceCommand(new Dictionary<string, string>
            {
                {"id", instanceId },
                {"f", parameters}
            }, new ServiceInstanceSchemaChecker(address));
                command.InjectSwitch("CatalogServiceEndpoint", address);
                var conclusion = command.Run();
                if (conclusion is SuccessConclusion)
                {
                    await this.StateManager.SetStateAsync<MyState>(instanceId, new MyState
                    {
                        ServiceId = serviceId,
                        PlanId = planId,
                        InstanceId = instanceId,
                        BindingId = "",
                        Credential = null
                    }, cancellationToken);
                    return true;
                }
                else
                    return false;
            }
        }

        public async Task<List<Tuple<string,string>>> GetBindingCredential(string instanceId, string bindingId, string parameters, CancellationToken cancellationToken)
        {
            var state = await this.StateManager.TryGetStateAsync<MyState>(instanceId, cancellationToken);
            if (state.HasValue)
            {
                if (state.Value.Credential != null)
                    return makeTupleList(state.Value.Credential);
                else
                {
                    string address = await resolveCatalogServiceAddress();
                    CreateBindingCommand command = new CreateBindingCommand(new Dictionary<string, string>
                    {
                        {"instance-id", instanceId},
                        {"id", bindingId },
                        {"f", parameters }
                    }, new ServiceInstanceSchemaChecker(address));
                    command.InjectSwitch("CatalogServiceEndpoint", address);
                    command.Run();

                    ListEntitiesCommand<BindingwithResult> listCommand = new ListEntitiesCommand<BindingwithResult>("binding");
                    listCommand.InjectSwitch("CatalogServiceEndpoint", address);
                    var conclusion = listCommand.Run();
                    var bindings = ((MultiOutputConclusion)conclusion).GetObjectList<BindingwithResult>();
                    if (bindings.Count > 0 && bindings[0].Result != null && !string.IsNullOrEmpty(bindings[0].Result.Credentials))
                    {
                        var newState = new MyState
                        {
                            ServiceId = state.Value.ServiceId,
                            PlanId = state.Value.PlanId,
                            InstanceId = instanceId,
                            BindingId = bindingId,
                            Credential = bindings[0].Result.Credentials
                        };
                        await this.StateManager.SetStateAsync<MyState>(instanceId, newState, cancellationToken);
                        return makeTupleList(bindings[0].Result.Credentials);
                    }
                    else
                        return null;
                }
            }
            else
                return null;
        }
        private List<Tuple<string,string>> makeTupleList(string credential)
        {
            if (!string.IsNullOrEmpty(credential))
            {
                var ret = new List<Tuple<string, string>>();
                var credentialObj = JObject.Parse(credential);
                foreach (var p in credentialObj.Properties())
                {
                    ret.Add(new Tuple<string, string>(p.Name, p.Value.ToString()));
                }
                return ret;
            }
            else
                return null;
        }
    }
    [DataContract]
    internal class MyState
    {
        [DataMember]
        public string InstanceId { get; set; }
        [DataMember]
        public string ServiceId { get; set; }
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string BindingId { get; set; }
        [DataMember]
        public string Credential { get; set; }
    }
}

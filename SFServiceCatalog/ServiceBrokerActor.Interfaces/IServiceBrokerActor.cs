using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using OSB;

namespace ServiceBrokerActor.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IServiceBrokerActor : IActor
    {
        Task<bool> Connect(string serviceId, string planId, string instanceId, string parameters, CancellationToken cancellationToken);

        Task<List<Tuple<string,string>>> GetBindingCredential(string instanceId, string bindingId, string parameters, CancellationToken cancellationToken);
    }
}

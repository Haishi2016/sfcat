using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors.Client;

using Microsoft.ServiceFabric.Actors;
using ServiceBrokerActor.Interfaces;
using System.Threading;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            string viewState = "";
            string instanceId = "mystorage";
            string bindingId = "myBinding";
            var token = default(CancellationToken);
            var actorProxy = ActorProxy.Create<IServiceBrokerActor>(new ActorId("1"), new Uri("fabric:/SFServiceCatalog/ServiceBrokerActorService"));
            var result = actorProxy.Connect(
                serviceId: "2e2fc314-37b6-4587-8127-8f9ee8b33fea",
                planId: "6ddf6b41-fb60-4b70-af99-8ecc4896b3cf",
                instanceId: instanceId,
                parameters: @"{
                    ""service_id"": ""2e2fc314-37b6-4587-8127-8f9ee8b33fea"",
                    ""plan_id"": ""6ddf6b41-fb60-4b70-af99-8ecc4896b3cf"",
                    ""parameters"": {
                        ""resourceGroup"": ""osb-test-group"",
                        ""storageAccountName"": ""cat01"",
                        ""location"": ""eastus"",
                        ""accountType"": ""Standard_LRS""
                    }
                }", 
                cancellationToken: token).Result;

            if (result)
            {
                var binding = actorProxy.GetBindingCredential(instanceId, bindingId, @"{
                    ""service_id"": ""2e2fc314-37b6-4587-8127-8f9ee8b33fea"",
                    ""plan_id"": ""6ddf6b41-fb60-4b70-af99-8ecc4896b3cf""
                }", token).Result;
                if (binding != null)
                {
                    foreach (var tuple in binding)
                    {
                        viewState += "<h2><b>" + tuple.Item1 + "</b><h2>";
                        viewState += "<h3>" + tuple.Item2 + "</h3>";
                    }
                }
            }
            ViewData["Message"] = viewState;
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

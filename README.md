
# Service Fabric Service Catalog
Service Catalog is an add-on service to your Service Fabric cluster that allows you to provision and bind to Azure services through [Open Service Broker API](https://www.openservicebrokerapi.org/). 

# Build and Deploy

## Build, configure, and deploy the service
Clone this repository and build the **SFServiceCatalog** solution. 

Modify the **appsettings.json** file under the **CatalogService** project to point to the OSB API endpoint you want to use:
```json
"ActorSwitches": {
      "OSBEntityCatalogActor": {
        "OSBEndpoint": "<OSB API endpoint>",
        "OSBUser": "<user name>",
        "OSBPassword": "<password>"
      }
},
```
Then, deploy the **SFServiceCatalog** application to your Service Fabric cluster. Now, you've got a Catalog Service on your cluster.

## Build and configure client
Service Catalog is currently managed by a sample command-line tool, **sfcat**. You can build **sfcat** by building the **sfcat** solution under *tools\sfcat* folder. Modify the **app.config** under sfcat to point sfcat to your Catalog Service endpoint:

```xml
 <appSettings>
    <add key="CatalogServiceEndpoint" value="http://localhost:8088"/>
    ...
  </appSettings>
```

# Getting started

With current version, you need to register your OSB endpoint with your Catalog Service using sfcat. We are looking at making this part of the Catalog Service configuration so that you don't have to use sfcat at all.

```bash
sfcat create broker --name <broker name of your choice> --url <OSB endpoint> --user <user name> --password <password>
```

Next, you can:

* [Manage Catalog Service using **sfcat**](docs/sfcat.md)
* [Use Catalog Service from your Service Fabric applciations](docs/programmability.md)

# Known Issues

1. Althoug the API supports it, you can't add multiple OSB API endpoints to the system yet.
2. Be default there are not OSB API endpoint registered. You need to use [**sfcat**](docs/sfcat.md) to register an OSB API endpoint before you can start creating service instances. 

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

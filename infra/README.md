# Azure deployment

This folder contains infrastructure-as-code definitions and helper files to deploy the Airline Booking API to Azure.

## Prerequisites

* Azure CLI `2.53.0` or later.
* An Azure subscription with permissions to create resource groups, App Service plans, Web Apps and Container Registries.
* A PostgreSQL database (managed by Azure Database for PostgreSQL or another provider). Update the `postgresConnectionString` parameter to point at the database.
* Docker installed locally if you plan to build and push images outside of Azure Pipelines.

## Deploying the infrastructure

1. Create (or choose) a resource group: `az group create --name airlinebooking-rg --location westus3`.
2. Update the `infra/parameters.example.json` file with unique resource names and the PostgreSQL connection string, then rename it to `infra/parameters.json`.
3. Deploy with the Azure CLI:

   ```bash
   az deployment group create \
     --resource-group airlinebooking-rg \
     --template-file infra/main.bicep \
     --parameters @infra/parameters.json
   ```

The deployment will create:

* A **Container Registry** where the API image is stored.
* A **Linux App Service plan**.
* A **Linux Web App** configured to run the container image.
* A **managed identity** for the Web App with `AcrPull` permissions on the registry.

After deployment you must push the container image to the registry (either manually or via the provided Azure Pipeline) and configure the Web App to use the new tag.

## Azure Pipelines

The repository includes an `azure-pipelines.yml` definition that:

1. Restores, builds and tests the solution.
2. Publishes the API artifact for traceability.
3. Builds and pushes a container image to Azure Container Registry.
4. Deploys the Bicep template to keep infrastructure in sync.
5. Updates the Web App to run the freshly built container image.

Configure the following pipeline variables or variable group secrets before running the pipeline:

| Variable | Description |
| --- | --- |
| `dockerRegistryServiceConnection` | Service connection with permissions to push to the target ACR. |
| `azureSubscriptionServiceConnection` | Service connection scoped to the resource group for deployments. |
| `resourceGroupName` | Name of the resource group created earlier. |
| `acrName` | Name of the Azure Container Registry. |
| `acrLoginServer` | Login server of the registry (e.g. `myregistry.azurecr.io`). |
| `webAppName` | Name of the Web App to deploy. |
| `appServicePlanName` | Name of the App Service plan. |
| `environmentName` | Tag used to identify the environment (e.g. `production`). |
| `USE_POSTGRES` (optional) | Set to `false` to keep SQLite. |
| `POSTGRES_CONNECTION_STRING` (optional) | Provide the PostgreSQL connection string for production. |

> **Security note:** Store secrets (connection strings, database passwords) in secure variable groups or Azure Key Vault and reference them from the pipeline rather than committing them to source control.

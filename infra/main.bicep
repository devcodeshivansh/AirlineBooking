@description('The Azure region to deploy resources into.')
param location string = resourceGroup().location

@description('Environment name used for resource naming and tagging.')
param environmentName string = 'prod'

@description('Globally unique name for the Azure Container Registry (lowercase, 5-50 alphanumeric characters).')
param acrName string

@description('Name of the Linux App Service plan that will host the web app.')
param appServicePlanName string = '${environmentName}-plan'

@description('SKU for the App Service plan.')
@allowed([
  'B1'
  'B2'
  'B3'
  'S1'
  'S2'
  'S3'
])
param appServicePlanSku string = 'B1'

@description('Name of the Web App that will host the API container. Must be globally unique.')
param webAppName string

@description('Container image repository name (without registry login server).')
param containerImageName string = 'airlinebooking-api'

@description('Container image tag that should be deployed.')
param containerImageTag string = 'latest'

@description('Connection string for the PostgreSQL database used by the API. Leave empty to configure later.')
@secure()
param postgresConnectionString string = ''

@description('When true the API will use PostgreSQL (recommended for Azure). Set to false to continue using SQLite.')
param usePostgres bool = true

var appSettings = [
  {
    name: 'WEBSITES_PORT'
    value: '8080'
  }
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: 'Production'
  }
  {
    name: 'Database__UsePostgres'
    value: string(usePostgres)
  }
]

resource registry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    adminUserEnabled: false
    policies: {
      quarantinePolicy: {
        status: 'disabled'
      }
    }
  }
}

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSku
    tier: appServicePlanSku == 'B1' || appServicePlanSku == 'B2' || appServicePlanSku == 'B3' ? 'Basic' : 'Standard'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
  tags: {
    Environment: environmentName
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    serverFarmId: plan.id
    siteConfig: {
      linuxFxVersion: 'DOCKER|${registry.properties.loginServer}/${containerImageName}:${containerImageTag}'
      appSettings: appSettings
      alwaysOn: true
    }
    clientAffinityEnabled: false
  }
  tags: {
    Environment: environmentName
  }
}

@description('Assign the web app managed identity permissions to pull from the container registry.')
resource webAppAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: registry
  name: guid(webApp.identity.principalId, registry.id, 'acrpull')
  properties: {
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
  }
  dependsOn: [
    webApp
  ]
}

resource postgresConnection 'Microsoft.Web/sites/config@2023-12-01' = if (!empty(postgresConnectionString)) {
  parent: webApp
  name: 'connectionstrings'
  properties: {
    Postgres: {
      value: postgresConnectionString
      type: 'PostgreSQL'
    }
  }
}

output containerRegistryLoginServer string = registry.properties.loginServer
output appServicePrincipalId string = webApp.identity.principalId

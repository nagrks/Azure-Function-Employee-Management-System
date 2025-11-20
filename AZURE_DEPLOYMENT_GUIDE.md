# Azure Deployment Guide for Employee Management Function

## Prerequisites

### 1. Install Azure CLI
Download and install from: https://learn.microsoft.com/en-us/cli/azure/install-azure-cli

**For Windows (PowerShell):**
```powershell
# Using Windows Package Manager
winget install -e --id Microsoft.AzureCLI

# Or download MSI installer from:
# https://aka.ms/installazurecliwindows
```

### 2. Install Azure Functions Core Tools
```powershell
npm install -g azure-functions-core-tools@4 --unsafe-perm=true --allow-root
```

Or download from: https://github.com/Azure/azure-functions-core-tools/releases

### 3. Install .NET 8.0 SDK
Already installed ✓

---

## Azure Deployment Steps

### Step 1: Login to Azure
```powershell
az login
# This will open your browser to authenticate
```

### Step 2: Create Azure Resource Group (if not exists)
```powershell
az group create --name MyProjFolder-rg --location eastus
# Or use your preferred region: westus, westus2, northeurope, etc.
```

### Step 3: Create Storage Account
```powershell
az storage account create `
  --name myprojfolderstorage `
  --resource-group MyProjFolder-rg `
  --location eastus `
  --sku Standard_LRS
```

**Note:** Storage account name must be globally unique (lowercase, no hyphens)

### Step 4: Create App Service Plan
```powershell
az appservice plan create `
  --name MyProjFolder-plan `
  --resource-group MyProjFolder-rg `
  --sku B1 `
  --is-linux
```

### Step 5: Create Function App
```powershell
az functionapp create `
  --resource-group MyProjFolder-rg `
  --consumption-plan-location eastus `
  --runtime dotnet `
  --runtime-version 8.0 `
  --functions-version 4 `
  --name myprojfolder-func `
  --storage-account myprojfolderstorage
```

### Step 6: Configure Database Connection String
```powershell
az functionapp config appsettings set `
  --name myprojfolder-func `
  --resource-group MyProjFolder-rg `
  --settings "ConnectionString=Server=172.29.11.239;Database=ems;Uid=sa;Pwd=Amer1can;"
```

### Step 7: Publish the Function App

**Option A: Using Azure Functions Core Tools**
```powershell
cd D:\dotnet\repos\MyProjFolder

# Build the project
dotnet build

# Publish to Azure
func azure functionapp publish myprojfolder-func --csharp
```

**Option B: Using .NET CLI**
```powershell
cd D:\dotnet\repos\MyProjFolder

# Publish to a local folder
dotnet publish -c Release -o ./publish

# Deploy using Azure Functions Core Tools
func azure functionapp publish myprojfolder-func
```

**Option C: Using Visual Studio (if you have it)**
- Right-click project → Publish
- Select Azure Function App
- Follow the wizard

### Step 8: Verify Deployment
```powershell
# Check function app status
az functionapp show --name myprojfolder-func --resource-group MyProjFolder-rg

# Get the function app URL
az functionapp show --name myprojfolder-func --resource-group MyProjFolder-rg --query "defaultHostName" -o tsv

# View logs
az functionapp log tail --name myprojfolder-func --resource-group MyProjFolder-rg --follow
```

### Step 9: Test the Deployed Function
```powershell
# Get the function URL
$funcUrl = "https://$(az functionapp show -n myprojfolder-func -g MyProjFolder-rg --query 'defaultHostName' -o tsv)/api/employee"

# Get the function key
$funcKey = $(az functionapp keys list -n myprojfolder-func -g MyProjFolder-rg --query 'functionKeys.default' -o tsv)

# Test the endpoint
Invoke-WebRequest -Uri "$funcUrl?code=$funcKey" -Method GET
```

---

## Alternative: ARM Template Deployment

Create `deploy.json`:
```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "appName": {
      "type": "string",
      "defaultValue": "myprojfolder-func",
      "metadata": {
        "description": "Name of the Function App"
      }
    },
    "location": {
      "type": "string",
      "defaultValue": "[resourceGroup().location]",
      "metadata": {
        "description": "Location for all resources"
      }
    }
  },
  "resources": [
    {
      "type": "Microsoft.Storage/storageAccounts",
      "apiVersion": "2021-09-01",
      "name": "[format('st{0}', uniqueString(resourceGroup().id))]",
      "location": "[parameters('location')]",
      "sku": {
        "name": "Standard_LRS"
      },
      "kind": "StorageV2"
    },
    {
      "type": "Microsoft.Web/serverfarms",
      "apiVersion": "2021-02-01",
      "name": "[format('{0}-plan', parameters('appName'))]",
      "location": "[parameters('location')]",
      "sku": {
        "name": "Y1",
        "tier": "Dynamic"
      }
    },
    {
      "type": "Microsoft.Web/sites",
      "apiVersion": "2021-02-01",
      "name": "[parameters('appName')]",
      "location": "[parameters('location')]",
      "kind": "functionapp",
      "dependsOn": [
        "[resourceId('Microsoft.Web/serverfarms', format('{0}-plan', parameters('appName')))]"
      ],
      "properties": {
        "serverFarmId": "[resourceId('Microsoft.Web/serverfarms', format('{0}-plan', parameters('appName')))]"
      }
    }
  ]
}
```

Deploy with:
```powershell
az deployment group create `
  --resource-group MyProjFolder-rg `
  --template-file deploy.json
```

---

## Configuration & Settings

### Required App Settings
```powershell
az functionapp config appsettings set `
  --name myprojfolder-func `
  --resource-group MyProjFolder-rg `
  --settings `
    "AzureWebJobsStorage=DefaultEndpointsProtocol=https;..." `
    "FUNCTIONS_EXTENSION_VERSION=~4" `
    "FUNCTIONS_WORKER_RUNTIME=dotnet" `
    "ConnectionString=Server=172.29.11.239;Database=ems;Uid=sa;Pwd=Amer1can;"
```

### Database Connection Security (Recommended)
Use Azure Key Vault instead of storing secrets in app settings:

```powershell
# Create Key Vault
az keyvault create --resource-group MyProjFolder-rg --name MyProjFolderVault

# Add secret
az keyvault secret set --vault-name MyProjFolderVault --name "ConnectionString" --value "Server=172.29.11.239;Database=ems;Uid=sa;Pwd=Amer1can;"

# Configure function app to use Key Vault reference
az functionapp config appsettings set `
  --name myprojfolder-func `
  --resource-group MyProjFolder-rg `
  --settings "ConnectionString=@Microsoft.KeyVault(SecretUri=https://MyProjFolderVault.vault.azure.net/secrets/ConnectionString/)"
```

---

## Monitoring & Logging

### Enable Application Insights
```powershell
az monitor app-insights component create `
  --app myprojfolder-insights `
  --location eastus `
  --resource-group MyProjFolder-rg `
  --application-type web

# Get the instrumentation key
$instrKey = az monitor app-insights component show `
  --app myprojfolder-insights `
  --resource-group MyProjFolder-rg `
  --query 'instrumentationKey' -o tsv

# Configure function app
az functionapp config appsettings set `
  --name myprojfolder-func `
  --resource-group MyProjFolder-rg `
  --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrKey"
```

### View Logs
```powershell
# Real-time logs
az functionapp log tail --name myprojfolder-func --resource-group MyProjFolder-rg --follow

# Query logs
az functionapp log tail --name myprojfolder-func --resource-group MyProjFolder-rg --tail 100
```

---

## Troubleshooting

### Check function app status
```powershell
az functionapp show --name myprojfolder-func --resource-group MyProjFolder-rg
```

### View deployment logs
```powershell
az functionapp deployment log show --name myprojfolder-func --resource-group MyProjFolder-rg
```

### Restart function app
```powershell
az functionapp restart --name myprojfolder-func --resource-group MyProjFolder-rg
```

### Delete function app and resources
```powershell
az group delete --name MyProjFolder-rg
```

---

## Costs

Typical costs for this deployment:
- **Consumption Plan**: Pay per execution (~$0.20 per 1M requests)
- **Storage Account**: ~$0.50/month for minimal usage
- **Application Insights**: Free tier available (1 GB/month)

Total estimated cost: **$5-20/month** depending on usage

---

## Support Resources

- Azure Functions Documentation: https://learn.microsoft.com/en-us/azure/azure-functions/
- Azure CLI Reference: https://learn.microsoft.com/en-us/cli/azure/
- Azure SDK for .NET: https://github.com/Azure/azure-sdk-for-net
- Troubleshooting: https://learn.microsoft.com/en-us/azure/azure-functions/functions-troubleshooting

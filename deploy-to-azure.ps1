#!/usr/bin/env pwsh
# Azure Function Deployment Script for Employee Management Function
# This script automates the deployment process

param(
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "MyProjFolder-rg",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus",
    
    [Parameter(Mandatory=$false)]
    [string]$FunctionAppName = "myprojfolder-func",
    
    [Parameter(Mandatory=$false)]
    [string]$StorageAccountName = "myprojfolderstorage",
    
    [Parameter(Mandatory=$false)]
    [string]$AppServicePlanName = "MyProjFolder-plan",
    
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString = "Server=172.29.11.239;Database=ems;Uid=sa;Pwd=Amer1can;"
)

# Color functions for better output
function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Cyan
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Write-Warning-Custom {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

# Main deployment script
Write-Info "Azure Function Deployment Script"
Write-Info "=================================="
Write-Host ""

# Step 1: Check prerequisites
Write-Info "Step 1: Checking prerequisites..."
$prerequisites = @("az", "func", "dotnet")
$allGood = $true

foreach ($tool in $prerequisites) {
    try {
        $cmd = (Get-Command $tool -ErrorAction Stop).Source
        Write-Success "$tool is installed"
    } catch {
        Write-Error-Custom "$tool is not installed or not in PATH"
        $allGood = $false
    }
}

if (-not $allGood) {
    Write-Error-Custom "Please install missing prerequisites"
    exit 1
}

# Step 2: Login to Azure
Write-Info "Step 2: Logging in to Azure..."
try {
    $account = az account show --query 'user.name' -o tsv 2>$null
    if ($account) {
        Write-Success "Already logged in as $account"
    } else {
        Write-Info "Opening Azure login browser..."
        az login | Out-Null
        Write-Success "Logged in to Azure"
    }
} catch {
    Write-Error-Custom "Failed to authenticate with Azure"
    exit 1
}

# Step 3: Create Resource Group
Write-Info "Step 3: Creating Resource Group..."
try {
    $rgExists = az group exists --name $ResourceGroup -o tsv
    if ($rgExists -eq "true") {
        Write-Success "Resource group '$ResourceGroup' already exists"
    } else {
        az group create --name $ResourceGroup --location $Location | Out-Null
        Write-Success "Created resource group '$ResourceGroup' in $Location"
    }
} catch {
    Write-Error-Custom "Failed to create resource group"
    exit 1
}

# Step 4: Create Storage Account
Write-Info "Step 4: Creating Storage Account..."
try {
    $storageExists = az storage account list --resource-group $ResourceGroup --query "[?name=='$StorageAccountName'].name" -o tsv
    if ($storageExists) {
        Write-Success "Storage account '$StorageAccountName' already exists"
    } else {
        az storage account create `
            --name $StorageAccountName `
            --resource-group $ResourceGroup `
            --location $Location `
            --sku Standard_LRS | Out-Null
        Write-Success "Created storage account '$StorageAccountName'"
    }
} catch {
    Write-Error-Custom "Failed to create storage account"
    exit 1
}

# Step 5: Create App Service Plan
Write-Info "Step 5: Creating App Service Plan..."
try {
    $planExists = az appservice plan list --resource-group $ResourceGroup --query "[?name=='$AppServicePlanName'].name" -o tsv
    if ($planExists) {
        Write-Success "App Service Plan '$AppServicePlanName' already exists"
    } else {
        az appservice plan create `
            --name $AppServicePlanName `
            --resource-group $ResourceGroup `
            --sku B1 `
            --is-linux | Out-Null
        Write-Success "Created App Service Plan '$AppServicePlanName'"
    }
} catch {
    Write-Warning-Custom "Could not create traditional App Service Plan - will use Consumption Plan"
}

# Step 6: Create Function App
Write-Info "Step 6: Creating Function App..."
try {
    $funcAppExists = az functionapp list --resource-group $ResourceGroup --query "[?name=='$FunctionAppName'].name" -o tsv
    if ($funcAppExists) {
        Write-Success "Function App '$FunctionAppName' already exists"
    } else {
        az functionapp create `
            --resource-group $ResourceGroup `
            --consumption-plan-location $Location `
            --runtime dotnet `
            --runtime-version 8.0 `
            --functions-version 4 `
            --name $FunctionAppName `
            --storage-account $StorageAccountName `
            --os-type Linux | Out-Null
        Write-Success "Created Function App '$FunctionAppName'"
    }
} catch {
    Write-Error-Custom "Failed to create Function App"
    exit 1
}

# Step 7: Configure Connection String
Write-Info "Step 7: Configuring application settings..."
try {
    az functionapp config appsettings set `
        --name $FunctionAppName `
        --resource-group $ResourceGroup `
        --settings "ConnectionString=$ConnectionString" | Out-Null
    Write-Success "Configured connection string"
} catch {
    Write-Error-Custom "Failed to configure connection string"
    exit 1
}

# Step 8: Build Project
Write-Info "Step 8: Building project..."
try {
    dotnet build | Out-Null
    Write-Success "Project built successfully"
} catch {
    Write-Error-Custom "Failed to build project"
    exit 1
}

# Step 9: Publish to Azure
Write-Info "Step 9: Publishing to Azure..."
try {
    Write-Info "This may take a few minutes..."
    func azure functionapp publish $FunctionAppName --csharp --no-bundler
    Write-Success "Published to Azure"
} catch {
    Write-Error-Custom "Failed to publish to Azure"
    exit 1
}

# Step 10: Display deployment info
Write-Info "Step 10: Deployment complete!"
Write-Host ""
Write-Success "Deployment Summary"
Write-Host "==================" -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Green
Write-Host "Function App: $FunctionAppName" -ForegroundColor Green
Write-Host "Location: $Location" -ForegroundColor Green
Write-Host ""

# Get function URL
try {
    $funcUrl = "https://$(az functionapp show -n $FunctionAppName -g $ResourceGroup --query 'defaultHostName' -o tsv)/api/employee"
    Write-Host "Function URL: $funcUrl" -ForegroundColor Cyan
} catch {
    Write-Warning-Custom "Could not retrieve function URL"
}

# Display next steps
Write-Host ""
Write-Info "Next Steps:"
Write-Host "1. Test the endpoint using Postman or curl"
Write-Host "2. Monitor logs: az functionapp log tail --name $FunctionAppName --resource-group $ResourceGroup --follow"
Write-Host "3. View Azure Portal: https://portal.azure.com"
Write-Host ""
Write-Success "Deployment completed successfully!"

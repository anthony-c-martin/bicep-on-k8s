#!/bin/bash

subId=$(az account show --query id 2>/dev/null)
if [ $? -ne 0 ]; then
  az cloud set -n AzureCloud >/dev/null
  az login >/dev/null
  subId=$(az account show --query id 2>/dev/null)
fi

if [ $subId != '"d08e1a72-8180-4ed3-8125-9dff7376b0bd"' ]; then
  az account set -s "d08e1a72-8180-4ed3-8125-9dff7376b0bd" >/dev/null
fi

rgName="bicepbuild"
rgLocation="eastus"

az group create --name $rgName --location $rgLocation

# Save this as the AZURE_CREDENTIALS GitHub secret
az ad sp create-for-rbac --name $rgName --role contributor --scopes "/subscriptions/d08e1a72-8180-4ed3-8125-9dff7376b0bd/resourceGroups/$rgName" --sdk-auth
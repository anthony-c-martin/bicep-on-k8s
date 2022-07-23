#!/bin/bash

subId=$(az account show --query id 2>/dev/null)
if [ $? -ne 0 ]; then
  az cloud set -n AzureCloud >/dev/null
  az login >/dev/null
  subId=$(az account show --query id 2>/dev/null)
fi

if [ $subId != '"28cbf98f-381d-4425-9ac4-cf342dab9753"' ]; then
  az account set -s "28cbf98f-381d-4425-9ac4-cf342dab9753" >/dev/null
fi

rgName="ant-bicepbuild"
rgLoation="westcentralus"

az group create --name $rgName --location $rgLoation

# Save this as the AZURE_CREDENTIALS GitHub secret
az ad sp create-for-rbac --name $rgName --role contributor --scopes "/subscriptions/28cbf98f-381d-4425-9ac4-cf342dab9753/resourceGroups/$rgName" --sdk-auth
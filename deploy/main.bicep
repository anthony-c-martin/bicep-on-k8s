param baseName string
param dnsPrefix string
param linuxAdminUsername string
param sshRSAPublicKey string

var osDiskSizeGB = 0
var agentCount = 1
var agentVmSize = 'Standard_B2s'

var location = resourceGroup().location

resource aks 'Microsoft.ContainerService/managedClusters@2022-04-01' = {
  name: baseName
  location: location
  properties: {
    dnsPrefix: dnsPrefix
    agentPoolProfiles: [
      {
        name: 'agentpool'
        osDisksSizeGB: osDiskSizeGB
        count: agentCount
        vmSize: agentVmSize
        osType: 'Linux'
        mode: 'System'
      }
    ]
    linuxProfile: {
      adminUsername: linuxAdminUsername
      ssh: {
        publicKeys: [
          {
            keyData: sshRSAPublicKey
          }
        ]
      }
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

module kubernetes './modules/kubernetes.bicep' = {
  name: 'buildbicep-deploy'
  params: {
    kubeConfig: aks.listClusterAdminCredential().kubeconfigs[0].value
  }
}

var dnsLabel = kubernetes.outputs.dnsLabel
var normalizedLocation = toLower(replace(location, ' ', ''))

output endpoint string 'http://${dnsLabel}.${normalizedLocation}.cloudapp.azure.com'

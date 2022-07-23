@secure()
param kubeConfig string

import kubernetes as k8s {
  kubeConfig: kubeConfig
  namespace: 'default'
}

var front = {
  name: 'bicepbuild'
  version: 'latest'
  image: 'ghcr.io/anthony-c-martin/bicep-on-k8s:main'
  port: 80
}

resource frontDeploy 'apps/Deployment@v1' = {
  metadata: {
    name: front.name
  }
  spec: {
    selector: {
      matchLabels: {
        app: front.name
        version: front.version
      }
    }
    replicas: 1
    template: {
      metadata: {
        labels: {
          app: front.name
          version: front.version
        }
      }
      spec: {
        containers: [
          {
            name: front.name
            image: front.image
            ports: [
              {
                containerPort: front.port
              }
            ]
          }
        ]
      }
    }
  }
}

@description('Configure front-end service')
resource frontService 'core/Service@v1' = {
  metadata: {
    name: front.name
    annotations: {
      'service.beta.kubernetes.io/azure-dns-label-name': front.name
    }
  }
  spec: {
    type: 'LoadBalancer'
    ports: [
      {
        port: front.port
      }
    ]
    selector: {
      app: front.name
    }
  }
}

output dnsLabel string = front.name

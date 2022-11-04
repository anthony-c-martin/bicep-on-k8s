# bicep-on-k8s
This is a demo of using Bicep extensibility to deploy a dotnet application to a Kubernetes cluster. The application itself allows you to build a `.bicep` file via HTTP request. The build & deployment process is fully automated via GitHub Actions.

## Running locally
1. Open in Codespaces
1. Build: 
    ```sh
    docker build -t bicepbuild:latest ./src
    ```
1. Run: 
    ```sh
    docker run -p 8001:80 bicepbuild:latest ./src
    ```
1. Test locally:
    * Build:
        ```sh
        curl -X POST http://localhost:8001/build \
          -H 'Content-Type: application/json' \
          -d '{"bicepContents": "param foo string"}'
        ```
    * Decompile:
        ```sh
        curl -X POST http://localhost:8001/decompile \
          -H 'Content-Type: application/json' \
          -d '{"jsonContents": "{\n  \"$schema\": \"https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#\",\n  \"contentVersion\": \"1.0.0.0\",\n  \"parameters\": {\n    \"foo\": {\n      \"type\": \"string\"\n    }\n  },\n  \"resources\": []\n}"}'
        ```

## Testing the real service
* Build:
    ```sh
    curl -X POST http://bicepbuild.eastus.cloudapp.azure.com/build \
      -H 'Content-Type: application/json' \
      -d '{"bicepContents": "param foo string"}'
    ```
* Decompile:
    ```sh
    curl -X POST http://bicepbuild.eastus.cloudapp.azure.com/decompile \
      -H 'Content-Type: application/json' \
      -d '{"jsonContents": "{\n  \"$schema\": \"https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#\",\n  \"contentVersion\": \"1.0.0.0\",\n  \"parameters\": {\n    \"foo\": {\n      \"type\": \"string\"\n    }\n  },\n  \"resources\": []\n}"}'
    ```
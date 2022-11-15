# bicep-on-k8s
This is a demo of using Bicep extensibility to deploy a dotnet application to a Kubernetes cluster. The application itself allows you to build a `.bicep` file via HTTP request. The build & deployment process is fully automated via GitHub Actions.

## Setting up the environment
1. Fork this repo, open the forked repo in Codespaces
1. Open [./initial_setup.sh](./initial_setup.sh) and modify the values of `repoOwner`, `repoName`, `rgName` & `rgLocation`
1. Log in to Az CLI
1. Run `./initial_setup.sh <tenantId>, <subscriptionId>`
1. The output should contain 3 env variables `ACR_CLIENT_ID`, `ACR_SUBSCRIPTION_ID` & `ACR_TENANT_ID`. Save each one as a [GitHub Repo Secret](https://docs.github.com/en/actions/security-guides/encrypted-secrets#creating-encrypted-secrets-for-a-repository).
1. Modify [./deploy/main.bicepparam](./deploy/main.bicepparam) to contain desired values for your AKS cluster.
1. Modify the `AKS_RG_NAME` variable in [./.github/workflows/cd.yml](./.github/workflows/cd.yml) to match the value of `rgName` used in step 2.
1. Push to your `main` branch. GitHub Actions should build & deploy the .NET application to an AKS cluster!
1. To test the live service, check the GitHub Actions output for the cluster DNS name. Replace `bicepbuild.eastus.cloudapp.azure.com` with this value in the testing commands below.

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
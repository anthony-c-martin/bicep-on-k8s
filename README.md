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
    ```sh
    curl -X POST http://localhost:8001/Build \
      -H 'Content-Type: application/json' \
      -d '{"bicepContents": "param foo string"}'
    ```

## Testing the real service
```sh
curl -X POST http://bicepbuild.eastus.cloudapp.azure.com/Build \
    -H 'Content-Type: application/json' \
    -d '{"bicepContents": "param foo string"}'
```
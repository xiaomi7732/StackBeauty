# Steps to deploy the image as a Azure Container App

* Refer to: <https://docs.microsoft.com/en-us/azure/container-apps/get-started-existing-container-image?tabs=bash&pivots=container-apps-private-registry>

Use the following environment variable:

```shell
$LOCATION="eastus2"
$RESOURCE_GROUP="stackbeauty-$LOCATION-dev"
$LOG_ANALYTICS_WORKSPACE="stackbeauty-$LOCATION-logs"
$CONTAINERAPPS_ENVIRONMENT="stackbeauty-$LOCATION-dev-env"
$CONTAINERAPP_NAME="stackbeauty-dev"
```

Info for registry
```shell
az acr credential show -n saarsdevhub -o table
```

Info for the container
```shell
$CONTAINER_IMAGE_VERSION="1.0.0-beta2"
$CONTAINER_IMAGE_NAME="saarsdevhub.azurecr.io/spdevapps/stackbeauty`:$CONTAINER_IMAGE_VERSION"
$REGISTRY_LOGIN_SERVER="saarsdevhub.azurecr.io"
$REGISTRY_USERNAME="saarsdevhub"
$REGISTRY_PASSWORD="Redacted"
```

```shell
$CONTAINER_APP_NAME="stackbeauty-dev"
az containerapp create `
  --name $CONTAINER_APP_NAME `
  --resource-group $RESOURCE_GROUP `
  --image "$CONTAINER_IMAGE_NAME" `
  --environment $CONTAINERAPPS_ENVIRONMENT `
  --registry-login-server $REGISTRY_LOGIN_SERVER `
  --registry-username $REGISTRY_USERNAME `
  --registry-password "$REGISTRY_PASSWORD"
```

Update

```shell
$CONTAINER_APP_NAME="stackbeauty-dev"
az containerapp update -n $CONTAINER_APP_NAME -g $RESOURCE_GROUP `
            --image "$CONTAINER_IMAGE_NAME"
```
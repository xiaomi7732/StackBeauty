# Deployment Guide

## Update existing deployment

* Setup Context

```shell
$RELEASE_VERSION="1.0.0-beta6"
```

  * RG related

  ```shell
  $APP_NAME="stackbeauty"
  $LOCATION="eastus2"
  $ENVIRONMENT="dev"
  $RESOURCE_GROUP="$APP_NAME-$LOCATION-$ENVIRONMENT"
  ```

  * Container App

  ```shell
  $LOCAL_IMAGE_TAG="$APP_NAME"
  $REPO_IMAGE_NAME="saarsdevhub.azurecr.io/spdevapps/$APP_NAME"
  $REPO_IMAGE_TAG="$REPO_IMAGE_NAME`:$RELEASE_VERSION"
  $CONTAINER_APP_NAME="$APP_NAME-$ENVIRONMENT"
  ```

  * Logging

  ```shell
  $LOG_ANALYTICS_WORKSPACE="stackbeauty-$LOCATION-logs"
  $APP_INSIGHTS_NAME="$APP_NAME-insights-$ENVIRONMENT"
  ```

* Run deployment steps, in powershell enabled shell:

  * Build the image
  ```shell
  dotnet clean -c Release -o .\app\ .\NetStackBeautifier.WebAPI\
  dotnet publish -c Release -o .\app\ .\NetStackBeautifier.WebAPI\
  docker build . -t $LOCAL_IMAGE_TAG
  docker tag stackbeauty:latest $REPO_IMAGE_TAG
  docker push $REPO_IMAGE_TAG
  ```

  * Revision the app
  ```shell
  az containerapp update -n $CONTAINER_APP_NAME `
    -g $RESOURCE_GROUP `
    --image "$REPO_IMAGE_TAG" `
    --environment-variables "ApplicationInsights__ConnectionString=secretref:insights-connection-string"
  ```

## New deployment

//TODO: TBD

* Add application insights component

```shell


```

```shell
az monitor app-insights component create --app $APP_INSIGHTS_NAME  `
  --location $LOCATION `
  --kind web `
  -g $RESOURCE_GROUP `
  --application-type web `
  --workspace $LOG_ANALYTICS_WORKSPACE


$APPINSIGHTS_CONNECTIONSTRING=az monitor app-insights component show --app "$APP_INSIGHTS_NAME" -g "$RESOURCE_GROUP" --query connectionString -o tsv
az containerapp update -n $CONTAINER_APP_NAME `
  -g $RESOURCE_GROUP `
  --secrets "insights-connection-string=$APPINSIGHTS_CONNECTIONSTRING"
```
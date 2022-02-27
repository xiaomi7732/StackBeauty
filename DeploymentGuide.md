# Deployment Guide

## Update existing deployment

* Setup Context

```shell
$RELEASE_VERSION="1.0.0-beta3"
```

```shell
$LOCAL_IMAGE_TAG="stackbeauty"
$REPO_IMAGE_NAME="saarsdevhub.azurecr.io/spdevapps/stackbeauty"
$REPO_IMAGE_TAG="$REPO_IMAGE_NAME`:$RELEASE_VERSION"

$CONTAINER_APP_NAME="stackbeauty-dev"
$LOCATION="eastus2"
$RESOURCE_GROUP="stackbeauty-$LOCATION-dev"
```

* Run deployment steps, in powershell enabled shell:

```shell
dotnet clean -c Release -o .\app\ .\NetStackBeautifier.WebAPI\
dotnet publish -c Release -o .\app\ .\NetStackBeautifier.WebAPI\
docker build . -t $LOCAL_IMAGE_TAG
docker tag stackbeauty:latest $REPO_IMAGE_TAG
docker push $REPO_IMAGE_TAG
```

```shell
az containerapp update -n $CONTAINER_APP_NAME `
   -g $RESOURCE_GROUP `
   --image "$REPO_IMAGE_TAG"
```

## New deployment

//TODO: TBD
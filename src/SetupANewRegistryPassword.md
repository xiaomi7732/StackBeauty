# Setup a new registry password

* Check this out for how to generate a password for pulling: <https://github.com/xiaomi7732/ACRManager/blob/main/PushPullStackBeauty.md>. Note to change the permissions from pull/push to pull only.

1. Add a reistry password as a secret

    ```powershell
    $ContainerApp='stackbeauty-dev'
    $ContainerAppResourceGroup='stackbeauty-eastus2-dev'
    $ACRName='saarsdevhub'
    $ACRServer="$ACRName.azurecr.io"
    $UserName="stackbeauty-readonly-token"
    $Password="The password you get from the token generation"

    az containerapp registry set -n "$ContainerApp" -g "$ContainerAppResourceGroup" --server "$ACRServer" --username "$UserName" --password "$Password"
    ```

    ```powershell
    Output:
    Updating existing registry.
    Adding registry password as a secret with name "saarsdevhubazurecrio-stackbeauty-readonly-token"

    [
    {
        "identity": "",
        "passwordSecretRef": "saarsdevhubazurecrio-stackbeauty-readonly-token",
        "server": "saarsdevhub.azurecr.io",
        "username": "stackbeauty-readonly-token"
    }
    ]
    ```

1. Revision the app

    ```powershell
    $RELEASE_VERSION="20241104.01"
    $APP_NAME="stackbeauty"
    $LOCATION="eastus2"
    $ENVIRONMENT="dev"
    $RESOURCE_GROUP="$APP_NAME-$LOCATION-$ENVIRONMENT"
    $LOCAL_IMAGE_TAG="$APP_NAME"
    $REPO_IMAGE_NAME="saarsdevhub.azurecr.io/spdevapps/$APP_NAME"
    $REPO_IMAGE_TAG="$REPO_IMAGE_NAME`:$RELEASE_VERSION"
    $CONTAINER_APP_NAME="$APP_NAME-$ENVIRONMENT"
    $LOG_ANALYTICS_WORKSPACE="stackbeauty-$LOCATION-logs"
    $APP_INSIGHTS_NAME="$APP_NAME-insights-$ENVIRONMENT"
    az containerapp update -n $CONTAINER_APP_NAME `
        -g $RESOURCE_GROUP `
        --image "$REPO_IMAGE_TAG" `
        --set-env-vars "ApplicationInsights__ConnectionString=secretref:insights-connection-string"
    ```

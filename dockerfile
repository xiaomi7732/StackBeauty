  # syntax=docker/dockerfile:1
  FROM mcr.microsoft.com/dotnet/aspnet:6.0.2-alpine3.14-amd64
  COPY ./app App/
  WORKDIR /App
  ENTRYPOINT ["dotnet", "NetStackBeautifier.WebAPI.dll"]
  
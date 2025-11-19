FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Application/Application.csproj Application/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY WebAPI/WebAPI.csproj WebAPI/

RUN dotnet restore WebAPI/WebAPI.csproj

COPY . .

WORKDIR /src/WebAPI
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /src/WebAPI/out .
ENTRYPOINT ["dotnet", "WebAPI.dll"]
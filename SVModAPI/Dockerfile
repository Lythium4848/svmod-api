# Use official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY SVModAPI/SVModAPI.csproj SVModAPI/
RUN dotnet restore SVModAPI/SVModAPI.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/SVModAPI
RUN dotnet publish -c Release -o /app --no-restore

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "SVModAPI.dll" ]

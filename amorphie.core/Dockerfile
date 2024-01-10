FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /src

COPY ["amorphie.core/amorphie.core.csproj", "."]
RUN dotnet restore "amorphie.core.csproj"
COPY . .
WORKDIR "/src/amorphie.core"
     
RUN dotnet build "amorphie.core.csproj" -c Release -o /app/build
WORKDIR "/src/amorphie.core"
RUN dotnet pack /p:Version=1.1.21 -c Release -o /app/build 
RUN dotnet nuget push /app/build/*.nupkg --source https://api.nuget.org/v3/index.json --api-key "NUGETORG_TOKEN"

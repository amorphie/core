FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /src

COPY ["Amorphie.Core.Cache/Amorphie.Core.Cache.csproj", "."]
RUN dotnet restore "Amorphie.Core.Cache.csproj"
COPY . .
WORKDIR "/src/Amorphie.Core.Cache"
  
RUN dotnet build "Amorphie.Core.Cache.csproj" -c Release -o /app/build
RUN dotnet pack /p:Version=1.0.1 -c Release --no-restore -o /app/build 
RUN dotnet nuget push /app/build/*.nupkg --source https://api.nuget.org/v3/index.json --api-key "NUGETORG_TOKEN"

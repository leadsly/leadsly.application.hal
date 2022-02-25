 FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
 WORKDIR /app 

 FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

 WORKDIR /src
 COPY Leadsly.Application.Hal.sln ./
 COPY "Hal/*.csproj" ./Hal/
 COPY "Domain/*.csproj" ./Domain/
 COPY "PageObjects/*.csproj" ./PageObjects/

 RUN dotnet restore
 COPY . .

 WORKDIR "/src/Hal"
 RUN dotnet build -c Release -o /app

 WORKDIR "/src/Domain"
 RUN dotnet build -c Release -o /app

 WORKDIR "/src/PageObjects"
 RUN dotnet build -c Release -o /app

 FROM build AS publish
 RUN dotnet publish -c Release -o /app

 FROM base AS final
 WORKDIR /app
 COPY --from=publish /app .
 ENTRYPOINT ["dotnet", "Hal.dll"]
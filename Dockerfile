FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src
COPY "./Leadsly.Application.Hal.sln" .
COPY "./Leadsly.Application.Model/*.csproj" ./Leadsly.Application.Model/
COPY "./Domain/*.csproj" ./Domain/
COPY "./PageObjects/*.csproj" ./PageObjects/
COPY "./Infrastructure/*.csproj" ./Infrastructure/
COPY "./Hal/*.csproj" ./Hal/

RUN dotnet restore
COPY . .

WORKDIR "/src/Hal"
RUN dotnet build -c Release -o /app

WORKDIR "/src/Leadsly.Application.Model"
RUN dotnet build -c Release -o /app

WORKDIR "/src/Domain"
RUN dotnet build -c Release -o /app

WORKDIR "/src/PageObjects"
RUN dotnet build -c Release -o /app

WORKDIR "/src/Infrastructure"
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

ENV AWS_ACCESS_KEY_ID=AKIA2KIVUGORHZXNMOVT
ENV AWS_REGION=us-east-1
ENV AWS_SECRET_ACCESS_KEY=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
ENV HAL_ID=7e0c10e4-3f52-4389-97e8-427b6efa51c1

WORKDIR /app

EXPOSE 443
EXPOSE 80

ENTRYPOINT ["dotnet", "Hal.dll"]
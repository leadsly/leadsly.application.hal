FROM selenium/standalone-chrome:99.0.4844.84 as selenium
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app
EXPOSE 5020

WORKDIR /src
COPY "./Leadsly.Application.Hal/Leadsly.Application.Hal.sln" ./Leadsly.Application.Hal/
COPY "../Leadsly.Application.Model/*.csproj" ./Leadsly.Application.Model/
COPY "./Leadsly.Application.Hal/Domain/*.csproj" ./Leadsly.Application.Hal/Domain/
COPY "./Leadsly.Application.Hal/PageObjects/*.csproj" ./Leadsly.Application.Hal/PageObjects/
COPY "./Leadsly.Application.Hal/Infrastructure/*.csproj" ./Leadsly.Application.Hal/Infrastructure/
COPY "./Leadsly.Application.Hal/Hal/*.csproj" ./Leadsly.Application.Hal/Hal/

WORKDIR /src/Leadsly.Application.Hal
RUN dotnet restore

WORKDIR /src
COPY . .

WORKDIR "/src/Leadsly.Application.Hal/Hal"
RUN dotnet build -c Release -o /../../app

WORKDIR "/src/Leadsly.Application.Model"
RUN dotnet build -c Release -o /../../app

WORKDIR "/src/Leadsly.Application.Hal/Domain"
RUN dotnet build -c Release -o /../../app

WORKDIR "/src/Leadsly.Application.Hal/PageObjects"
RUN dotnet build -c Release -o /../../app

WORKDIR "/src/Leadsly.Application.Hal/Infrastructure"
RUN dotnet build -c Release -o /../../app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

ENV AWS_ACCESS_KEY_ID=AKIA2KIVUGORHZXNMOVT
ENV AWS_REGION=us-east-1
ENV AWS_SECRET_ACCESS_KEY=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
ENV HAL_ID=62adfec0-ffff-41dd-b532-d51247f21187-id

ENTRYPOINT ["dotnet", "Hal.dll"]
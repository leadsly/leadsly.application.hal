FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

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
ENV HAL_ID=7e0c10e4-3f52-4389-97e8-427b6efa51c1

WORKDIR /leadsly_chrome_profiles
COPY "./Leadsly.Application.Hal/leadsly_default_chrome_profile" ./leadsly_default_chrome_profile
RUN chmod a+rw /leadsly_chrome_profiles \
  && chmod a+rw /leadsly_chrome_profiles/leadsly_default_chrome_profile
VOLUME /leadsly_chrome_profiles

WORKDIR /app

ENTRYPOINT ["dotnet", "Hal.dll"]
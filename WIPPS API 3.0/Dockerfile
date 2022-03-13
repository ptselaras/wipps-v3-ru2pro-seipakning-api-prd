#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1-buster AS build
WORKDIR /src
COPY ["WIPPS API 3.0/WIPPS API 3.0.csproj", "WIPPS API 3.0/"]
COPY ["EmailService/EmailService.csproj", "EmailService/"]
RUN dotnet restore "WIPPS API 3.0/WIPPS API 3.0.csproj"
COPY . .
WORKDIR "/src/WIPPS API 3.0"
RUN dotnet build "WIPPS API 3.0.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WIPPS API 3.0.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN apt-get update -qq && apt-get -y install libgdiplus libc6-dev
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WIPPS API 3.0.dll"]
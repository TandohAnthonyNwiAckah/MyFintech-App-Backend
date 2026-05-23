FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8020

# Install wkhtmltopdf and its dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    wkhtmltopdf \
    libxrender1 \
    libxtst6 \
    libxi6 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["myfintech.csproj", "./"]
RUN dotnet restore "myfintech.csproj"
COPY . .
RUN dotnet build "myfintech.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "myfintech.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create uploads directory and set permissions
RUN mkdir -p uploads/files uploads/temp && chmod -R 777 uploads

ENTRYPOINT ["dotnet", "myfintech.dll"]

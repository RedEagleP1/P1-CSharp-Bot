# Use SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Bot/Bot.csproj", "Bot/"]
COPY ["Bot-Application/Bot-Application.csproj", "Bot-Application/"]
COPY ["Bot-Infrastructure/Bot-Infrastructure.csproj", "Bot-Infrastructure/"]
RUN dotnet restore "Bot/Bot.csproj"

# Copy remaining source code
COPY . .

# Build app
WORKDIR "/src/Bot"
RUN dotnet build "Bot.csproj" -c Release -o /app/build

# Publish app
FROM build AS publish
RUN dotnet publish "Bot.csproj" -c Release -o /app/publish

# Final stage/image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=publish /app/publish .

# TODO Add env variables and read them from the command line arguments
# Create directory for config
RUN mkdir -p /app/config

# Copy config file (needs to be added at runtime)
# COPY appsettings.json /app/config/

ENTRYPOINT ["dotnet", "Bot.dll"]
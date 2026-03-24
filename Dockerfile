# Stage 1: Build the code
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /source

# Copy EVERYTHING from the local host folder into the container
COPY . .

# Navigate to the code and compile it
WORKDIR /source/src/WazeBotDiscord
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Run the bot
FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine
WORKDIR /app
COPY --from=build /app/publish .

# Start the bot
ENTRYPOINT ["dotnet", "WazeBotDiscord.dll"]

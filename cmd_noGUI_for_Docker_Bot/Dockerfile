# Use the official .NET SDK image as a build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["cmd_noGUI_for_Docker_Bot.csproj", "./"]
RUN dotnet restore "cmd_noGUI_for_Docker_Bot.csproj"
COPY . .
RUN dotnet build "cmd_noGUI_for_Docker_Bot.csproj" -c Release -o /app/build
RUN dotnet publish "cmd_noGUI_for_Docker_Bot.csproj" -c Release -o /app/publish

# Use the official .NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "cmd_noGUI_for_Docker_Bot.dll"]
